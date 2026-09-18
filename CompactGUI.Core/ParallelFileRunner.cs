using System.Collections.Concurrent;
using System.Diagnostics;

namespace CompactGUI.Core;

internal enum DiskSpaceRecoveryMode
{
    SinglePass,
    RepeatWhileProgress
}

internal  record ParallelFileRunnerOptions<T>
{
    public required Func<T, string> GetFileName { get; init; }
    public required Func<T, long> GetProgressWeight { get; init; }
    public required Func<T, FileOperationResult> ProcessFile { get; init; }
    public Action<T>? OnProcessing { get; init; }
    public Func<T, long>? GetRecoveryOrder { get; init; }
    public TimeSpan ProgressInterval { get; init; } = TimeSpan.FromMilliseconds(100);
    public DiskSpaceRecoveryMode RecoveryMode { get; init; } = DiskSpaceRecoveryMode.SinglePass;
}


//apparently i should seal everything https://www.meziantou.net/performance-benefits-of-sealed-class.htm
internal sealed class ParallelFileRunner<T>(ParallelFileRunnerOptions<T> options) : IDisposable
{
    private readonly Func<T, long> recoveryOrderSelector = options.GetRecoveryOrder ?? options.GetProgressWeight;
    private readonly long progressIntervalTicks = (long)(options.ProgressInterval.TotalSeconds * Stopwatch.Frequency);
    private readonly ManualResetEventSlim pauseGate = new(initialState: true);
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly object cancellationGate = new();
    private readonly ConcurrentDictionary<string, byte> activeFiles = new();
    private int activeFileOperations;
    private bool cancellationRequested;
    private long processedWeight;
    private long totalWeight;
    private long lastProgressReportTicks;

    public CancellationToken Token => cancellationTokenSource.Token;
    public bool IsCancellationRequested => cancellationTokenSource.IsCancellationRequested;

    public async Task<int> RunAsync(IReadOnlyCollection<T> files, long totalWeight, int maxParallelism, IProgress<CompressionProgress>? progressMonitor)
    {
        cancellationTokenSource.Token.ThrowIfCancellationRequested();
        this.totalWeight = totalWeight;
        Interlocked.Exchange(ref processedWeight, 0);
        Interlocked.Exchange(ref lastProgressReportTicks, 0);

        if (files.Count == 0 || totalWeight == 0)
        {
            progressMonitor?.Report(new CompressionProgress(100, ""));
            return 0;
        }

        int failedFileCount = 0;
        ConcurrentBag<T> diskSpaceFailures = new();
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = maxParallelism, CancellationToken = cancellationTokenSource.Token };

        await Parallel.ForEachAsync(files, parallelOptions, (file, ctx) =>
        {
            ctx.ThrowIfCancellationRequested();

            FileOperationResult result = PauseAndProcessFile(file, progressMonitor, cancellationTokenSource.Token);
            if (result == FileOperationResult.InsufficientDiskSpace) diskSpaceFailures.Add(file);
            else if (result == FileOperationResult.Failed) Interlocked.Increment(ref failedFileCount);

            return ValueTask.CompletedTask;
        }).ConfigureAwait(false);

        Debug.WriteLine($"Disk space failures: {diskSpaceFailures.Count}");
        return failedFileCount + RetryDiskSpaceFailures(diskSpaceFailures, progressMonitor);
    }

    private FileOperationResult PauseAndProcessFile(T file, IProgress<CompressionProgress>? progressMonitor, CancellationToken token)
    {
        options.OnProcessing?.Invoke(file);
        pauseGate.Wait(token);

        string fileName = options.GetFileName(file);
        lock (cancellationGate)
        {
            if (cancellationRequested) return FileOperationResult.Success;
            activeFileOperations++;
            activeFiles.TryAdd(fileName, 0);
        }
        ReportProgress(progressMonitor, fileName);

        try
        {
            FileOperationResult result = options.ProcessFile(file);
            if (result == FileOperationResult.Success) Interlocked.Add(ref processedWeight, options.GetProgressWeight(file));
            return result;
        }
        finally
        {
            lock (cancellationGate)
            {
                activeFileOperations--;
                activeFiles.TryRemove(fileName, out _);
            }
        }
    }

    private int RetryDiskSpaceFailures(IEnumerable<T> failures, IProgress<CompressionProgress>? progressMonitor)
    {
        List<T> pending = [.. failures.OrderBy(recoveryOrderSelector)];
        int failedFileCount = 0;

        while (pending.Count > 0)
        {
            bool madeProgress = false;
            List<T> remaining = new(pending.Count);

            foreach (T file in pending)
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                FileOperationResult result = PauseAndProcessFile(file, progressMonitor, cancellationTokenSource.Token);
                if (result == FileOperationResult.Success) madeProgress = true;
                else if (result == FileOperationResult.InsufficientDiskSpace) remaining.Add(file);
                else failedFileCount++;
            }

            if (remaining.Count == 0) break;

            if (options.RecoveryMode == DiskSpaceRecoveryMode.SinglePass || !madeProgress)
            {
                failedFileCount += remaining.Count;
                break;
            }

            pending = remaining;
        }

        return failedFileCount;
    }

    public void ReportProgress(IProgress<CompressionProgress>? progressMonitor, string fileName = "", bool force = false)
    {
        if (totalWeight == 0)
        {
            if (force) progressMonitor?.Report(new CompressionProgress(100, fileName, activeFiles.Keys.ToArray()));
            return;
        }

        long now = Stopwatch.GetTimestamp();
        if (!force && now - Interlocked.Read(ref lastProgressReportTicks) < progressIntervalTicks) return;

        Interlocked.Exchange(ref lastProgressReportTicks, now);
        progressMonitor?.Report(new CompressionProgress((int)((double)Interlocked.Read(ref processedWeight) / totalWeight * 100.0), fileName, activeFiles.Keys.ToArray()));
    }

    public void Pause() => pauseGate.Reset();
    public void Resume() => pauseGate.Set();

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
}
