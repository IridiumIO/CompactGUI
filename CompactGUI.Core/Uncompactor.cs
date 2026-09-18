
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32.SafeHandles;
using System.Collections.Concurrent;
using Windows.Win32;
using CompactGUI.Logging.Core;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CompactGUI.Core;

public sealed class Uncompactor : ICompressor, IDisposable
{

    private readonly IReadOnlyList<string> filesList;
    private readonly ManualResetEventSlim pauseGate = new(initialState: true);
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly object cancellationGate = new();
    private readonly ConcurrentDictionary<string, byte> activeFiles = new();
    private int activeFileOperations;
    private bool cancellationRequested;
    private long lastProgressReportTicks;
    private int processedFileCount;

    private readonly ILogger<Uncompactor> _logger;

    public Uncompactor(IReadOnlyList<string> filesList, ILogger<Uncompactor>? logger = null)
    {
        this.filesList = filesList ?? throw new ArgumentNullException(nameof(filesList));
        _logger = logger ?? NullLogger<Uncompactor>.Instance;
    }

    public async Task<bool> RunAsync(IProgress<CompressionProgress>? progressMonitor = null, int maxParallelism = 1)
    {
        int totalFiles = filesList.Count;
        int failedFileCount = 0;
        ConcurrentBag<string> diskSpaceFailures = new();
        if (maxParallelism <= 0) maxParallelism = Environment.ProcessorCount;
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = maxParallelism, CancellationToken = cancellationTokenSource.Token };
        Interlocked.Exchange(ref processedFileCount, 0);

        UncompactorLog.StartingDecompression(_logger, totalFiles, maxParallelism);
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            await Parallel.ForEachAsync(filesList, parallelOptions,
                (file, ctx) =>
                {
                    ctx.ThrowIfCancellationRequested();
                    FileOperationResult result = PauseAndProcessFile(file, totalFiles, progressMonitor, cancellationTokenSource.Token);
                    if (result == FileOperationResult.InsufficientDiskSpace)
                    {
                        diskSpaceFailures.Add(file);
                    }
                    else if (result == FileOperationResult.Failed)
                    {
                        Interlocked.Increment(ref failedFileCount);
                    }

                    return ValueTask.CompletedTask;
                });

            failedFileCount += FileOperationRecovery.Retry(
                diskSpaceFailures,
                GetFileLength,
                file => PauseAndProcessFile(file, totalFiles, progressMonitor, cancellationTokenSource.Token),
                cancellationTokenSource.Token,
                repeatWhileProgress: false);
        }
        catch (OperationCanceledException) {
            UncompactorLog.DecompressionCanceled(_logger);
            ReportProgress(progressMonitor, totalFiles, "", true);
            return false; 
        }
        catch (Exception ex)
        {
            UncompactorLog.DecompressionFailed(_logger, ex.Message);
            return false;
        }
        finally { sw.Stop(); }

        if (failedFileCount > 0)
        {
            UncompactorLog.DecompressionFailed(_logger, $"{failedFileCount} file operation(s) failed.");
            ReportProgress(progressMonitor, totalFiles, "", true);
            return true;
        }

        UncompactorLog.DecompressionCompleted(_logger, Math.Round(sw.Elapsed.TotalSeconds, 3));
        ReportProgress(progressMonitor, totalFiles, "", true);
        return true;

    }

    private FileOperationResult PauseAndProcessFile(string file, int totalFiles, IProgress<CompressionProgress>? progressMonitor, CancellationToken ctx)
    {
        UncompactorLog.ProcessingFile(_logger, file);
        try
        {
            pauseGate.Wait(ctx);
        }
        catch (OperationCanceledException) { throw; }
        lock (cancellationGate)
        {
            if (cancellationRequested) return FileOperationResult.Success;
            activeFileOperations++;
            activeFiles.TryAdd(file, 0);
        }
        ReportProgress(progressMonitor, totalFiles, file);

        try
        {
            FileOperationResult result = WOFDecompressFile(file);
            if (result == FileOperationResult.Success) Interlocked.Increment(ref processedFileCount);

            return result;
        }
        finally
        {
            lock (cancellationGate)
            {
                activeFileOperations--;
                activeFiles.TryRemove(file, out _);
            }
        }

    }

    private void ReportProgress(IProgress<CompressionProgress>? progressMonitor, int totalFiles, string fileName, bool force = false)
    {
        long now = Stopwatch.GetTimestamp();
        if (!force && now - Interlocked.Read(ref lastProgressReportTicks) < Stopwatch.Frequency / 5) return;

        Interlocked.Exchange(ref lastProgressReportTicks, now);
        progressMonitor?.Report(new CompressionProgress((int)(Volatile.Read(ref processedFileCount) / (float)totalFiles * 100), fileName, activeFiles.Keys.ToArray()));
    }

    private unsafe FileOperationResult WOFDecompressFile(string file)
    {
        try
        {
            using (SafeFileHandle fs = File.OpenHandle(file))
            {
                uint bytesReturned;
                if (PInvoke.DeviceIoControl(fs, WOFHelper.FSCTL_DELETE_EXTERNAL_BACKING, null, 0, null, 0, &bytesReturned, null))
                {
                    return FileOperationResult.Success;
                }

                int errorCode = Marshal.GetLastPInvokeError();
                return FileOperationRecovery.IsInsufficientDiskSpaceError(errorCode)
                    ? FileOperationResult.InsufficientDiskSpace
                    : FileOperationResult.Failed;
            }  
        }
        catch (Exception) { 
            return FileOperationResult.Failed;
        }
    }

    private static long GetFileLength(string file)
    {
        try
        {
            return new FileInfo(file).Length;
        }
        catch
        {
            return long.MaxValue;
        }
    }

    public void Pause()
    {
        UncompactorLog.DecompressionPaused(_logger);
        pauseGate.Reset();
    }


    public void Resume()
    {
        pauseGate.Set();
        UncompactorLog.DecompressionResumed(_logger);
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
        pauseGate.Dispose();
        cancellationTokenSource.Dispose();
    }







}
