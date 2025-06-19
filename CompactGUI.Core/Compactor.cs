
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
    private readonly string[] excludedFileExtensions;
    private readonly WOFCompressionAlgorithm wofCompressionAlgorithm;


    private IntPtr compressionInfoPtr;
    private UInt32 compressionInfoSize;

    private long totalProcessedBytes = 0;
    private readonly SemaphoreSlim pauseSemaphore = new SemaphoreSlim(1, 2);
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();


    public Compactor(string folderPath, WOFCompressionAlgorithm compressionLevel, string[] excludedFileTypes)
    {
        workingDirectory = folderPath;
        excludedFileExtensions = excludedFileTypes;
        wofCompressionAlgorithm = compressionLevel;

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

        var workingFiles = await BuildWorkingFilesList().ConfigureAwait(false);
        long totalFilesSize = workingFiles.Sum((f) => f.UncompressedSize);

        totalProcessedBytes = 0;

        if(maxParallelism <= 0) maxParallelism = Environment.ProcessorCount;
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = maxParallelism, CancellationToken = cancellationTokenSource.Token };

        try
        {
            await Parallel.ForEachAsync(workingFiles, parallelOptions,
                (file, ctx) =>
                {
                    ctx.ThrowIfCancellationRequested();

                    return new ValueTask(PauseAndProcessFile(file, totalFilesSize, cancellationTokenSource.Token, progressMonitor));
                }).ConfigureAwait(false);
        }
        catch (OperationCanceledException){ return false; }
        catch (Exception){ return false; }

        return true;
    }

    private async Task PauseAndProcessFile(FileDetails file, long totalFilesSize, CancellationToken token, IProgress<CompressionProgress> progressMonitor)
    {

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
            Debug.WriteLine(ex.Message);
            return null;
        }
    }

    private async Task<IEnumerable<FileDetails>> BuildWorkingFilesList()
    {
        uint clusterSize = SharedMethods.GetClusterSize(workingDirectory);

        var filesList = new ConcurrentBag<FileDetails>();

        var analyser = new Analyser(workingDirectory);
        var ret = await analyser.AnalyseFolder(cancellationTokenSource.Token);

        Parallel.ForEach(analyser.FileCompressionDetailsList, (fl) =>
        {
            var ft = fl.FileInfo;
            if ((!excludedFileExtensions.Contains(ft?.Extension) || excludedFileExtensions.Contains(fl.FileName))
                && ft.Length > clusterSize
                && fl.CompressionMode != wofCompressionAlgorithm)
            {
                filesList.Add(new FileDetails { FileName = fl.FileName, UncompressedSize = fl.UncompressedSize });
            }
        });

        return filesList.ToList();
    }


    public void Pause()
    {
        pauseSemaphore.Wait(cancellationTokenSource.Token);
    }


    public void Resume()
    {
        if (pauseSemaphore.CurrentCount == 0) pauseSemaphore.Release();  
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


    private readonly record struct FileDetails(string FileName, long UncompressedSize);


}
