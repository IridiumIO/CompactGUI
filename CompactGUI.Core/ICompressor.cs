namespace CompactGUI.Core;

public interface ICompressor : IDisposable
{
    Task<bool> RunAsync(List<String> filesList,
        IProgress<CompressionProgress> progressMonitor = null,
        int maxParallelism = 1);

    void Pause();
    void Resume();
    void Cancel();


}