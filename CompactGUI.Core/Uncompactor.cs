
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
    private int activeFileOperations;
    private bool cancellationRequested;
    private ConcurrentDictionary<string, int> processedFileCount = new ConcurrentDictionary<string, int>();

    private readonly ILogger<Uncompactor> _logger;

    public Uncompactor(ILogger<Uncompactor>? logger = null)
    {
        _logger = logger ?? NullLogger<Uncompactor>.Instance;
    }

    public async Task<bool> RunAsync(List<string> filesList, IProgress<CompressionProgress>? progressMonitor = null, int maxParallelism = 1)
    {
        int totalFiles = filesList.Count;
        if (maxParallelism <= 0) maxParallelism = Environment.ProcessorCount;
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = maxParallelism, CancellationToken = cancellationTokenSource.Token };
        processedFileCount.Clear();

        UncompactorLog.StartingDecompression(_logger, totalFiles, maxParallelism);
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            await Parallel.ForEachAsync(filesList, parallelOptions,
                (file, ctx) =>
                {
                    ctx.ThrowIfCancellationRequested();
                    return new ValueTask(PauseAndProcessFile(file, totalFiles, progressMonitor, cancellationTokenSource.Token));
                });
        }
        catch (OperationCanceledException) {
            UncompactorLog.DecompressionCanceled(_logger);
            return false; 
        }
        finally { sw.Stop(); }

        UncompactorLog.DecompressionCompleted(_logger, Math.Round(sw.Elapsed.TotalSeconds, 3));
        return true;

    }

    private async Task PauseAndProcessFile(string file, int totalFiles, IProgress<CompressionProgress>? progressMonitor, CancellationToken ctx)
    {
        UncompactorLog.ProcessingFile(_logger, file);
        try
        {
            pauseGate.Wait(ctx);
        }
        catch (OperationCanceledException) { throw; }
        lock (cancellationGate)
        {
            if (cancellationRequested) return;
            activeFileOperations++;
        }

        try
        {
            var _ = WOFDecompressFile(file);
            processedFileCount.TryAdd(file, 1);
            progressMonitor?.Report(new CompressionProgress(
                    (int)(processedFileCount.Count / (float)totalFiles * 100),
                    file)
            );
        }
        finally
        {
            lock (cancellationGate) activeFileOperations--;
        }

    }

    private unsafe bool? WOFDecompressFile(string file)
    {
        try
        {
            using (SafeFileHandle fs = File.OpenHandle(file))
            {
                var res = PInvoke.DeviceIoControl(fs, WOFHelper.FSCTL_DELETE_EXTERNAL_BACKING, null, 0, null, 0, null, null);
                return res;
            }  
        }
        catch (Exception ex) { 
            UncompactorLog.FileDecompressionFailed(_logger, file, ex.Message);
            return null; 
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
