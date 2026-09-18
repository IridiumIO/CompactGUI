namespace CompactGUI.Core;

internal enum FileOperationResult
{
    Success,
    InsufficientDiskSpace,
    Failed
}

internal static class FileOperationRecovery
{
    private const int ErrorHandleDiskFull = 39;
    private const int ErrorDiskFull = 112;
    private const int ErrorDiskQuotaExceeded = 1295;

    public static bool IsInsufficientDiskSpaceError(int errorCode) =>
        errorCode is ErrorHandleDiskFull or ErrorDiskFull or ErrorDiskQuotaExceeded;

    public static bool IsInsufficientDiskSpaceHResult(int hResult) =>
        IsInsufficientDiskSpaceError(hResult & 0xFFFF)
        && (hResult & unchecked((int)0xFFFF0000)) == unchecked((int)0x80070000);

    public static int Retry<T>(
        IEnumerable<T> failures,
        Func<T, long> sizeSelector,
        Func<T, FileOperationResult> processFile,
        CancellationToken token,
        bool repeatWhileProgress)
    {
        List<T> pending = [.. failures.OrderBy(sizeSelector)];
        int failedFileCount = 0;

        while (pending.Count > 0)
        {
            bool madeProgress = false;
            List<T> remaining = new(pending.Count);

            foreach (T file in pending)
            {
                token.ThrowIfCancellationRequested();

                FileOperationResult result = processFile(file);
                if (result == FileOperationResult.Success) madeProgress = true;
                else if (result == FileOperationResult.InsufficientDiskSpace) remaining.Add(file);
                else failedFileCount++;
            }

            if (remaining.Count == 0) break;

            if (!repeatWhileProgress || !madeProgress)
            {
                failedFileCount += remaining.Count;
                break;
            }

            pending = remaining;
        }

        return failedFileCount;
    }
}
