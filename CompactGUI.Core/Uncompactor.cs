
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using CompactGUI.Logging.Core;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CompactGUI.Core;

public sealed class Uncompactor : ICompressor, IDisposable
{

    private readonly IReadOnlyList<string> filesList;
    private readonly ILogger<Uncompactor> _logger;
    private readonly ParallelFileRunner<string> runner;

    public Uncompactor(IReadOnlyList<string> filesList, ILogger<Uncompactor>? logger = null)
    {
        this.filesList = filesList ?? throw new ArgumentNullException(nameof(filesList));
        _logger = logger ?? NullLogger<Uncompactor>.Instance;
        runner = new ParallelFileRunner<string>(new ParallelFileRunnerOptions<string>
        {
            GetFileName = file => file,
            GetProgressWeight = _ => 1,
            ProcessFile = WOFDecompressFile,
            OnProcessing = file => UncompactorLog.ProcessingFile(_logger, file),
            GetRecoveryOrder = GetFileLength,
            ProgressInterval = TimeSpan.FromMilliseconds(200)
        });
    }

    public async Task<bool> RunAsync(IProgress<CompressionProgress>? progressMonitor = null, int maxParallelism = 1)
    {
        int totalFiles = filesList.Count;
        int failedFileCount = 0;
        if (maxParallelism <= 0) maxParallelism = Environment.ProcessorCount;

        UncompactorLog.StartingDecompression(_logger, totalFiles, maxParallelism);
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            failedFileCount = await runner.RunAsync(filesList, totalFiles, maxParallelism, progressMonitor).ConfigureAwait(false);
        }
        catch (OperationCanceledException) {
            UncompactorLog.DecompressionCanceled(_logger);
            runner.ReportProgress(progressMonitor, force: true);
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
            runner.ReportProgress(progressMonitor, force: true);
            return true;
        }

        UncompactorLog.DecompressionCompleted(_logger, Math.Round(sw.Elapsed.TotalSeconds, 3));
        runner.ReportProgress(progressMonitor, force: true);
        return true;

    }

    private unsafe FileOperationResult WOFDecompressFile(string file)
    {
        try
        {
            using (SafeFileHandle fs = File.OpenHandle(file))
            {
                uint bytesReturned;
                if (PInvoke.DeviceIoControl(fs, WOFHelper.FSCTL_DELETE_EXTERNAL_BACKING, null, 0, null, 0, &bytesReturned, null)) return FileOperationResult.Success;

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
        runner.Pause();
    }


    public void Resume()
    {
        runner.Resume();
        UncompactorLog.DecompressionResumed(_logger);
    }


    public int Cancel() => runner.Cancel();


    public void Dispose() => runner.Dispose();







}
