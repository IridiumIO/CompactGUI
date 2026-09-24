
using CompactGUI.Logging.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using Windows.Win32;

namespace CompactGUI.Core;

public sealed class Compactor : ICompressor, IDisposable
{

    private readonly string workingDirectory;
    private readonly HashSet<string> exclusionList;
    private readonly WOFCompressionAlgorithm wofCompressionAlgorithm;
    private readonly bool bypassLowDiskSpaceProtection;
    private readonly ParallelFileRunner<FileDetails> runner;

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
        runner = new ParallelFileRunner<FileDetails>(new ParallelFileRunnerOptions<FileDetails>
        {
            GetFileName = file => file.FileName,
            GetProgressWeight = file => file.UncompressedSize,
            ProcessFile = file => WOFCompressFile(file.FileName),
            OnProcessing = file => CompactorLog.ProcessingFile(_logger, file.FileName, file.UncompressedSize),
            RecoveryMode = DiskSpaceRecoveryMode.RepeatWhileProgress
        });
    }

    public async Task<bool> RunAsync(IProgress<CompressionProgress>? progressMonitor = null, int maxParallelism = 1)
    {
        if (runner.IsCancellationRequested) return false;

        CompactorLog.BuildingWorkingFilesList(_logger, workingDirectory);
        var workingFiles = await BuildWorkingFilesList().ConfigureAwait(false);
        if (workingFiles is null)
        {
            CompactorLog.CompressionFailed(_logger, "Unable to build the compression file list.");
            return false;
        }

        long totalFilesSize = workingFiles.Sum((f) => f.UncompressedSize);

        if (totalFilesSize == 0)
        {
            CompactorLog.CompressionCompleted(_logger, 0);
            progressMonitor?.Report(new CompressionProgress(100, ""));
            return true;
        }

        var sw = Stopwatch.StartNew();

        maxParallelism = GetWorkerCount(maxParallelism, workingFiles, bypassLowDiskSpaceProtection);
        Debug.WriteLine($"Compactor: Using {maxParallelism} parallel workers for compression.");
        CompactorLog.StartingCompression(_logger, workingDirectory, wofCompressionAlgorithm.ToString(), maxParallelism);
        int failedFileCount;
        try
        {
            failedFileCount = await runner.RunAsync(workingFiles, totalFilesSize, maxParallelism, progressMonitor).ConfigureAwait(false);
        }
        catch (OperationCanceledException){
            CompactorLog.CompressionCanceled(_logger);
            runner.ReportProgress(progressMonitor, force: true);
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
            runner.ReportProgress(progressMonitor, force: true);
            return true;
        }

        
        CompactorLog.CompressionCompleted(_logger, Math.Round(sw.Elapsed.TotalSeconds, 3));
        runner.ReportProgress(progressMonitor, force: true);
        return true;
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

        
        var analysedFiles = await _analyser.GetAnalysedFilesAsync(runner.Token);

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
        runner.Pause();
    }


    public void Resume()
    {
        runner.Resume();
        CompactorLog.CompressionResumed(_logger);
    }


    public int Cancel() => runner.Cancel();


    public void Dispose() => runner.Dispose();


    public readonly record struct FileDetails(string FileName, long UncompressedSize, long AllocatedSize);

}
