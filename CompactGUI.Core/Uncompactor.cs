
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32.SafeHandles;
using System.Collections.Concurrent;
using Windows.Win32;
using CompactGUI.Logging.Core;
using System.Diagnostics;

namespace CompactGUI.Core;

public sealed class Uncompactor : ICompressor, IDisposable
{

    private readonly ManualResetEventSlim pauseGate = new(initialState: true);
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly object cancellationGate = new();
    private readonly ConcurrentDictionary<string, byte> activeFiles = new();
    private int activeFileOperations;
    private bool cancellationRequested;
    private long lastProgressReportTicks;
    private int processedFileCount;

    private readonly ILogger<Uncompactor> _logger;

    public Uncompactor(ILogger<Uncompactor>? logger = null)
    {
        _logger = logger ?? NullLogger<Uncompactor>.Instance;
    }

    public async Task<bool> RunAsync(List<string> filesList, IProgress<CompressionProgress>? progressMonitor = null, int maxParallelism = 1, bool bypassLowDiskSpaceProtection = false)
    {
        int totalFiles = filesList.Count;
        int failedFileCount = 0;
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
                    if (!PauseAndProcessFile(file, totalFiles, progressMonitor, cancellationTokenSource.Token))
                    {
                        Interlocked.Increment(ref failedFileCount);
                    }

                    return ValueTask.CompletedTask;
                });
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

    private bool PauseAndProcessFile(string file, int totalFiles, IProgress<CompressionProgress>? progressMonitor, CancellationToken ctx)
    {
        UncompactorLog.ProcessingFile(_logger, file);
        try
        {
            pauseGate.Wait(ctx);
        }
        catch (OperationCanceledException) { throw; }
        lock (cancellationGate)
        {
            if (cancellationRequested) return true;
            activeFileOperations++;
            activeFiles.TryAdd(file, 0);
        }
        ReportProgress(progressMonitor, totalFiles, file);

        try
        {
            bool succeeded = WOFDecompressFile(file);
            if (succeeded) Interlocked.Increment(ref processedFileCount);

            return succeeded;
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

    private unsafe bool WOFDecompressFile(string file)
    {
        try
        {
            using (SafeFileHandle fs = File.OpenHandle(file))
            {
                uint bytesReturned;
                bool succeeded = PInvoke.DeviceIoControl(fs, WOFHelper.FSCTL_DELETE_EXTERNAL_BACKING, null, 0, null, 0, &bytesReturned, null);
                if (succeeded) return true;

                return false;
            }  
        }
        catch (Exception) { 
            return false; 
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
