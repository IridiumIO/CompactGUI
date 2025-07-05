
using CompactGUI.Logging.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32.SafeHandles;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using Windows.Win32;

namespace CompactGUI.Core;

public class Compactor : ICompressor, IDisposable
{

    private readonly string workingDirectory;
    private readonly HashSet<string> excludedFileExtensions;
    private readonly WOFCompressionAlgorithm wofCompressionAlgorithm;


    private IntPtr compressionInfoPtr;
    private UInt32 compressionInfoSize;

    private long totalProcessedBytes = 0;
    private readonly SemaphoreSlim pauseSemaphore = new SemaphoreSlim(1, 2);
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private ILogger<Compactor> _logger;

    public Compactor(string folderPath, WOFCompressionAlgorithm compressionLevel, string[] excludedFileTypes, ILogger<Compactor>? logger = null)
    {
        workingDirectory = folderPath;
        excludedFileExtensions = new HashSet<string>(excludedFileTypes);
        wofCompressionAlgorithm = compressionLevel;
        _logger = logger ?? NullLogger<Compactor>.Instance;
        InitializeCompressionInfoPointer();
    }


    private void InitializeCompressionInfoPointer()
    {
        var _EFInfo = new WOFHelper.WOF_FILE_COMPRESSION_INFO_V1 { Algorithm = (UInt32)wofCompressionAlgorithm, Flags = 0 };
        compressionInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_EFInfo));
        compressionInfoSize = (UInt32)Marshal.SizeOf(_EFInfo);
        Marshal.StructureToPtr(_EFInfo, compressionInfoPtr, true);

    }

    public async Task<bool> RunAsync(List<string> filesList, IProgress<CompressionProgress> progressMonitor = null, int maxParallelism = 1)
    {
        if(cancellationTokenSource.IsCancellationRequested) { return false; }

        CompactorLog.BuildingWorkingFilesList(_logger, workingDirectory);
        var workingFiles = await BuildWorkingFilesList().ConfigureAwait(false);
        long totalFilesSize = workingFiles.Sum((f) => f.UncompressedSize);

        totalProcessedBytes = 0;

        var sw = Stopwatch.StartNew();

        if (maxParallelism <= 0) maxParallelism = Environment.ProcessorCount;
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = maxParallelism, CancellationToken = cancellationTokenSource.Token };

        CompactorLog.StartingCompression(_logger, workingDirectory, wofCompressionAlgorithm.ToString(), maxParallelism);
        try
        {
           await Parallel.ForEachAsync(workingFiles, parallelOptions,
                (file, ctx) =>
                {
                    ctx.ThrowIfCancellationRequested();

                    return new ValueTask(PauseAndProcessFile(file, totalFilesSize, cancellationTokenSource.Token, progressMonitor));
                }).ConfigureAwait(false);
        }
        catch (OperationCanceledException){
            CompactorLog.CompressionCanceled(_logger);
            return false; 
        }
        catch (Exception ex){ 
            CompactorLog.CompressionFailed(_logger, ex.Message);
            return false; 
        }
        finally { sw.Stop();}


        
        CompactorLog.CompressionCompleted(_logger, Math.Round(sw.Elapsed.TotalSeconds, 3));
        return true;
    }

    private async Task PauseAndProcessFile(FileDetails file, long totalFilesSize, CancellationToken token, IProgress<CompressionProgress> progressMonitor)
    {
        CompactorLog.ProcessingFile(_logger, file.FileName, file.UncompressedSize);

        await pauseSemaphore.WaitAsync(token).ConfigureAwait(false);
        pauseSemaphore.Release();

        var res = WOFCompressFile(file.FileName);
        Interlocked.Add(ref totalProcessedBytes, file.UncompressedSize);
        progressMonitor?.Report(new CompressionProgress((int)((double)totalProcessedBytes / totalFilesSize * 100.0), file.FileName));

    }

    private unsafe int? WOFCompressFile(string filePath)
    {
        try
        {
            using (SafeFileHandle fs = File.OpenHandle(filePath))
            {
                return PInvoke.WofSetFileDataLocation(fs, (uint)WOFHelper.WOF_PROVIDER_FILE, compressionInfoPtr.ToPointer(), compressionInfoSize);
            }
        }
        catch (Exception ex)
        {
            CompactorLog.FileCompressionFailed(_logger, filePath, ex.Message);
            return null;
        }
    }

    public async Task<IEnumerable<FileDetails>> BuildWorkingFilesList()
    {
        uint clusterSize = SharedMethods.GetClusterSize(workingDirectory);

        var analyser = new Analyser(workingDirectory, NullLogger<Analyser>.Instance);
        var ret = await analyser.AnalyseFolder(cancellationTokenSource.Token);

        var filesList = analyser.FileCompressionDetailsList
            .Where(fl =>
                fl.CompressionMode != wofCompressionAlgorithm
                && fl.UncompressedSize > clusterSize
                && ((fl.FileInfo != null && !excludedFileExtensions.Contains(fl.FileInfo.Extension)) || excludedFileExtensions.Contains(fl.FileName))
            )
            .Select(fl => new FileDetails(fl.FileName, fl.UncompressedSize))
            .ToList();

        return filesList;
    }




    public void Pause()
    {
        CompactorLog.CompressionPaused(_logger);
        pauseSemaphore.Wait(cancellationTokenSource.Token);
    }


    public void Resume()
    {
        if (pauseSemaphore.CurrentCount == 0) pauseSemaphore.Release();  
        CompactorLog.CompressionResumed(_logger);
    }


    public void Cancel()
    {
        Resume();
        cancellationTokenSource.Cancel();
    }


    public void Dispose()
    {
        cancellationTokenSource?.Dispose();
        pauseSemaphore?.Dispose();
        if (compressionInfoPtr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(compressionInfoPtr);
            compressionInfoPtr = IntPtr.Zero;
        }
    }


    public readonly record struct FileDetails(string FileName, long UncompressedSize);


}
