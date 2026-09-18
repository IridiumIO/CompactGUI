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
}
