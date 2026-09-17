
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

public sealed class Compactor : ICompressor, IDisposable
{

    private readonly string workingDirectory;
    private readonly HashSet<string> exclusionList;
    private readonly WOFCompressionAlgorithm wofCompressionAlgorithm;


    private IntPtr compressionInfoPtr;
    private UInt32 compressionInfoSize;

    private long totalProcessedBytes = 0;
    private readonly ManualResetEventSlim pauseGate = new(initialState: true);
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly object cancellationGate = new();
    private readonly ConcurrentDictionary<string, byte> activeFiles = new();
    private int activeFileOperations;
    private bool cancellationRequested;
    private long lastProgressReportTicks;

    private ILogger<Compactor> _logger;

    private Analyser _analyser;

    public Compactor(string folderPath, WOFCompressionAlgorithm compressionLevel, string[] excludedFileTypes, Analyser analyser, ILogger<Compactor>? logger = null)
    {
        workingDirectory = folderPath;
        exclusionList = new HashSet<string>(excludedFileTypes, StringComparer.OrdinalIgnoreCase);
        wofCompressionAlgorithm = compressionLevel;
        _logger = logger ?? NullLogger<Compactor>.Instance;
        _analyser = analyser;
        InitializeCompressionInfoPointer();
    }


    private void InitializeCompressionInfoPointer()
    {
        var _EFInfo = new WOFHelper.WOF_FILE_COMPRESSION_INFO_V1 { Algorithm = (UInt32)wofCompressionAlgorithm, Flags = 0 };
        compressionInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_EFInfo));
        compressionInfoSize = (UInt32)Marshal.SizeOf(_EFInfo);
        Marshal.StructureToPtr(_EFInfo, compressionInfoPtr, true);

    }

    public async Task<bool> RunAsync(List<string> filesList, IProgress<CompressionProgress> progressMonitor = null, int maxParallelism = 1, bool bypassLowDiskSpaceProtection = false)
    {
        if(cancellationTokenSource.IsCancellationRequested) { return false; }

        CompactorLog.BuildingWorkingFilesList(_logger, workingDirectory);
        var workingFiles = await BuildWorkingFilesList().ConfigureAwait(false);
        long totalFilesSize = workingFiles.Sum((f) => f.UncompressedSize);

        totalProcessedBytes = 0;

        var sw = Stopwatch.StartNew();

        maxParallelism = GetWorkerCount(maxParallelism, workingFiles, bypassLowDiskSpaceProtection);
        Debug.WriteLine($"Compactor: Using {maxParallelism} parallel workers for compression.");
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
            ReportProgress(progressMonitor, totalFilesSize, "", true);
            return false; 
        }
        catch (Exception ex){ 
            CompactorLog.CompressionFailed(_logger, ex.Message);
            return false; 
        }
        finally { sw.Stop();}


        
        CompactorLog.CompressionCompleted(_logger, Math.Round(sw.Elapsed.TotalSeconds, 3));
        ReportProgress(progressMonitor, totalFilesSize, "", true);
        return true;
    }

    private async Task PauseAndProcessFile(FileDetails file, long totalFilesSize, CancellationToken token, IProgress<CompressionProgress> progressMonitor)
    {
        CompactorLog.ProcessingFile(_logger, file.FileName, file.UncompressedSize);

        pauseGate.Wait(token);
        lock (cancellationGate)
        {
            if (cancellationRequested) return;
            activeFileOperations++;
            activeFiles.TryAdd(file.FileName, 0);
        }
        ReportProgress(progressMonitor, totalFilesSize, file.FileName);

        try
        {
            var res = WOFCompressFile(file.FileName);
            Interlocked.Add(ref totalProcessedBytes, file.UncompressedSize);
        }
        finally
        {
            lock (cancellationGate)
            {
                activeFileOperations--;
                activeFiles.TryRemove(file.FileName, out _);
            }
        }

    }

    private void ReportProgress(IProgress<CompressionProgress> progressMonitor, long totalFilesSize, string fileName, bool force = false)
    {
        long now = Stopwatch.GetTimestamp();
        if (!force && now - Interlocked.Read(ref lastProgressReportTicks) < Stopwatch.Frequency / 10) return;

        Interlocked.Exchange(ref lastProgressReportTicks, now);
        progressMonitor?.Report(new CompressionProgress((int)((double)totalProcessedBytes / totalFilesSize * 100.0), fileName, activeFiles.Keys.ToArray()));
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

        
        var analysedFiles = await _analyser.GetAnalysedFilesAsync(cancellationTokenSource.Token);

        if (analysedFiles is null) return Enumerable.Empty<FileDetails>();

        var excludedFiles = SkipListMatcher.GetExcludedFiles(workingDirectory, analysedFiles.Select(f => f.FileName), exclusionList);

        return analysedFiles
            .Where(fl =>
                fl.CompressionMode != wofCompressionAlgorithm
                && fl.UncompressedSize > clusterSize
                && !fl.Attributes.HasFlag(FileAttributes.SparseFile)
                && !excludedFiles.Contains(fl.FileName)
            )
            .Select(fl => new FileDetails(fl.FileName, fl.UncompressedSize, fl.CompressedSize))
            .ToList();
    }

    private int GetWorkerCount(int requestedWorkerCount, IEnumerable<FileDetails> files, bool bypassLowDiskSpaceProtection)
    {
        int workerCount = requestedWorkerCount <= 0 ? Environment.ProcessorCount : requestedWorkerCount;

        if (bypassLowDiskSpaceProtection) return workerCount;

        var fileList = files.ToList();
        bool containsDiskImage = fileList.Any(file => new[] { ".vhd", ".vhdx", ".vmdk", ".qcow2", ".img", ".iso" }.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase));

        try
        {
            var root = Path.GetPathRoot(workingDirectory);
            if (string.IsNullOrWhiteSpace(root)) return workerCount;

            long reserveBytes = fileList.Sum(file => file.AllocatedSize) / 2 + fileList.Select(file => file.AllocatedSize).DefaultIfEmpty().Max();
            bool lowFreeSpace = new DriveInfo(root).AvailableFreeSpace < reserveBytes * 2;
            return lowFreeSpace || containsDiskImage ? 1 : workerCount;
        }
        catch (IOException)
        {
            return containsDiskImage ? 1 : workerCount;
        }
    }




    public void Pause()
    {
        CompactorLog.CompressionPaused(_logger);
        pauseGate.Reset();
    }


    public void Resume()
    {
        pauseGate.Set();
        CompactorLog.CompressionResumed(_logger);
    }


    public int Cancel()
    {
        int activeOperations;
        lock (cancellationGate)
        {
            cancellationRequested = true;
            activeOperations = activeFileOperations;
        }
        pauseGate.Set();
        cancellationTokenSource.Cancel();
        return activeOperations;
    }


    public void Dispose()
    {
        cancellationTokenSource?.Dispose();
        pauseGate?.Dispose();
        if (compressionInfoPtr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(compressionInfoPtr);
            compressionInfoPtr = IntPtr.Zero;
        }
    }


    public readonly record struct FileDetails(string FileName, long UncompressedSize, long AllocatedSize);


}
