
using Microsoft.Win32.SafeHandles;
using System.Collections.Concurrent;
using Windows.Win32;

namespace CompactGUI.Core;

public class Uncompactor : ICompressor, IDisposable
{

    private SemaphoreSlim pauseSemaphore = new SemaphoreSlim(1, 2);
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private ConcurrentDictionary<string, int> processedFileCount = new ConcurrentDictionary<string, int>();


    public async Task<bool> RunAsync(List<string> filesList, IProgress<CompressionProgress>? progressMonitor = null, int maxParallelism = 1)
    {
        int totalFiles = filesList.Count;
        if (maxParallelism <= 0) maxParallelism = Environment.ProcessorCount;
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = maxParallelism };
        processedFileCount.Clear();

        try
        {
            await Parallel.ForEachAsync(filesList, parallelOptions,
                (file, ctx) =>
                {
                    ctx.ThrowIfCancellationRequested();
                    return new ValueTask(PauseAndProcessFile(file, totalFiles, progressMonitor, cancellationTokenSource.Token));
                });
        }
        catch (OperationCanceledException) { return false; }

        return true;

    }

    private async Task PauseAndProcessFile(string file, int totalFiles, IProgress<CompressionProgress>? progressMonitor, CancellationToken ctx)
    {
        try
        {
            await pauseSemaphore.WaitAsync(ctx).ConfigureAwait(false);
            pauseSemaphore.Release();
        }
        catch (OperationCanceledException) { throw; }
        ctx.ThrowIfCancellationRequested();

        var _ = WOFDecompressFile(file);
        processedFileCount.TryAdd(file, 1);
        progressMonitor?.Report(new CompressionProgress(
                (int)(processedFileCount.Count / (float)totalFiles * 100),
                file)
        );

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
        catch (Exception) { return null; }
    }

    public void Pause()
    {
        pauseSemaphore.Wait();
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
        pauseSemaphore.Dispose();
        cancellationTokenSource.Dispose();
    }







}
