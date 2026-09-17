namespace CompactGUI.Core;

public interface ICompressor : IDisposable
{
    Task<bool> RunAsync(List<String> filesList, IProgress<CompressionProgress> progressMonitor = null, int maxParallelism = 1, bool bypassLowDiskSpaceProtection = false);

    void Pause();
    void Resume();
    int Cancel();


}
