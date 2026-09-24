namespace CompactGUI.Core;

public interface ICompressor : IDisposable
{
    Task<bool> RunAsync(IProgress<CompressionProgress>? progressMonitor = null, int maxParallelism = 1);

    void Pause();
    void Resume();
    int Cancel();


}
