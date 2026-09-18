
using CompactGUI.Logging.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32.SafeHandles;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Enumeration;
using Windows.Win32;

namespace CompactGUI.Core;

public sealed class Compactor : ICompressor, IDisposable
{

    private readonly string workingDirectory;
    private readonly HashSet<string> exclusionList;
    private readonly WOFCompressionAlgorithm wofCompressionAlgorithm;
    private readonly bool bypassLowDiskSpaceProtection;


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

    public Compactor(string folderPath, WOFCompressionAlgorithm compressionLevel, string[] excludedFileTypes, Analyser analyser, bool bypassLowDiskSpaceProtection, ILogger<Compactor>? logger = null)
    {
        workingDirectory = folderPath;
        exclusionList = new HashSet<string>(excludedFileTypes, StringComparer.OrdinalIgnoreCase);
        wofCompressionAlgorithm = compressionLevel;
        this.bypassLowDiskSpaceProtection = bypassLowDiskSpaceProtection;
        _logger = logger ?? NullLogger<Compactor>.Instance;
        _analyser = analyser;
    }

    public async Task<bool> RunAsync(IProgress<CompressionProgress>? progressMonitor = null, int maxParallelism = 1)
    {
        if(cancellationTokenSource.IsCancellationRequested) { return false; }

        CompactorLog.BuildingWorkingFilesList(_logger, workingDirectory);
        var workingFiles = await BuildWorkingFilesList().ConfigureAwait(false);
        if (workingFiles is null)
        {
            CompactorLog.CompressionFailed(_logger, "Unable to build the compression file list.");
            return false;
        }

        long totalFilesSize = workingFiles.Sum((f) => f.UncompressedSize);

        totalProcessedBytes = 0;
        if (totalFilesSize == 0)
        {
            CompactorLog.CompressionCompleted(_logger, 0);
            progressMonitor?.Report(new CompressionProgress(100, ""));
            return true;
        }

        int failedFileCount = 0;
        ConcurrentBag<FileDetails> diskSpaceFailures = new();

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

                    FileOperationResult result = PauseAndProcessFile(file, totalFilesSize, cancellationTokenSource.Token, progressMonitor);
                    if (result == FileOperationResult.InsufficientDiskSpace)
                    {
                        diskSpaceFailures.Add(file);
                    }
                    else if (result == FileOperationResult.Failed)
                    {
                        Interlocked.Increment(ref failedFileCount);
                    }

                    return ValueTask.CompletedTask;
                }).ConfigureAwait(false);

            Debug.WriteLine($"Disk space failures: {diskSpaceFailures.Count}");

            // Sequentiall retry any files that failed due to insufficient disk space
            failedFileCount += FileOperationRecovery.Retry(
                diskSpaceFailures,
                file => file.UncompressedSize,
                file => PauseAndProcessFile(file, totalFilesSize, cancellationTokenSource.Token, progressMonitor),
                cancellationTokenSource.Token,
                repeatWhileProgress: true);
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

        if (failedFileCount > 0)
        {
            CompactorLog.CompressionFailed(_logger, $"{failedFileCount} file operation(s) failed.");
            ReportProgress(progressMonitor, totalFilesSize, "", true);
            return true;
        }

        
        CompactorLog.CompressionCompleted(_logger, Math.Round(sw.Elapsed.TotalSeconds, 3));
        ReportProgress(progressMonitor, totalFilesSize, "", true);
        return true;
    }

    private FileOperationResult PauseAndProcessFile(FileDetails file, long totalFilesSize, CancellationToken token, IProgress<CompressionProgress>? progressMonitor)
    {
        CompactorLog.ProcessingFile(_logger, file.FileName, file.UncompressedSize);

        pauseGate.Wait(token);
        lock (cancellationGate)
        {
            if (cancellationRequested) return FileOperationResult.Success;
            activeFileOperations++;
            activeFiles.TryAdd(file.FileName, 0);
        }
        ReportProgress(progressMonitor, totalFilesSize, file.FileName);

        try
        {
            FileOperationResult result = WOFCompressFile(file.FileName);
            if (result == FileOperationResult.Success)
            {
                Interlocked.Add(ref totalProcessedBytes, file.UncompressedSize);
            }
          
            return result;
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

    private void ReportProgress(IProgress<CompressionProgress>? progressMonitor, long totalFilesSize, string fileName, bool force = false)
    {
        long now = Stopwatch.GetTimestamp();
        if (!force && now - Interlocked.Read(ref lastProgressReportTicks) < Stopwatch.Frequency / 10) return;

        Interlocked.Exchange(ref lastProgressReportTicks, now);
        progressMonitor?.Report(new CompressionProgress((int)((double)totalProcessedBytes / totalFilesSize * 100.0), fileName, activeFiles.Keys.ToArray()));
    }

    private unsafe FileOperationResult WOFCompressFile(string filePath)
    {
        const int ErrorCompressionNotBeneficialHResult = unchecked((int)0x80070158);

        try
        {
            using (SafeFileHandle fs = File.OpenHandle(filePath))
            {
                WOFHelper.WOF_FILE_COMPRESSION_INFO_V1 compressionInfo = new()
                {
                    Algorithm = (uint)wofCompressionAlgorithm,
                    Flags = 0
                };

                int result = PInvoke.WofSetFileDataLocation(fs,(uint)WOFHelper.WOF_PROVIDER_FILE, &compressionInfo, (uint)sizeof(WOFHelper.WOF_FILE_COMPRESSION_INFO_V1));

                if (result >= 0 || result == ErrorCompressionNotBeneficialHResult) return FileOperationResult.Success;
                if (FileOperationRecovery.IsInsufficientDiskSpaceHResult(result)) return FileOperationResult.InsufficientDiskSpace;

                return FileOperationResult.Failed;
            }
        }
        catch (Exception)
        {
            return FileOperationResult.Failed;
        }
    }

    public async Task<List<FileDetails>?> BuildWorkingFilesList()
    {
        uint clusterSize = SharedMethods.GetClusterSize(workingDirectory);

        
        var analysedFiles = await _analyser.GetAnalysedFilesAsync(cancellationTokenSource.Token);

        if (analysedFiles is null) return null;

        var excludedFiles = SkipListMatcher.GetExcludedFiles(workingDirectory, analysedFiles.Select(f => f.FileName), exclusionList);

        return [.. analysedFiles
                    .Where(fl =>
                        fl.CompressionMode != wofCompressionAlgorithm
                        && fl.UncompressedSize > clusterSize
                        && !fl.Attributes.HasFlag(FileAttributes.SparseFile)
                        && !excludedFiles.Contains(fl.FileName)
                    )
                    .Select(fl => new FileDetails(fl.FileName, fl.UncompressedSize, fl.CompressedSize))];
    }

    private int GetWorkerCount(int requestedWorkerCount, IReadOnlyList<FileDetails> files, bool bypassLowDiskSpaceProtection)
    {
        int workerCount = requestedWorkerCount <= 0 ? Environment.ProcessorCount : requestedWorkerCount;

        if (bypassLowDiskSpaceProtection) return workerCount;

        long totalAllocatedSize = 0;
        long largestAllocatedSize = 0;
        bool containsDiskImage = false;

        foreach (FileDetails file in files)
        {
            totalAllocatedSize = checked(totalAllocatedSize + file.AllocatedSize);
            largestAllocatedSize = Math.Max(largestAllocatedSize, file.AllocatedSize);
            containsDiskImage |= SharedMethods.IsDiskImage(file.FileName);
        }

        try
        {
            var root = Path.GetPathRoot(workingDirectory);
            if (string.IsNullOrWhiteSpace(root)) return workerCount;

            long reserveBytes = totalAllocatedSize / 2 + largestAllocatedSize;
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
        cancellationTokenSource.Dispose();
        pauseGate.Dispose();
    }


    public readonly record struct FileDetails(string FileName, long UncompressedSize, long AllocatedSize);

}
