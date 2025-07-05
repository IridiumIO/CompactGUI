using Microsoft.Extensions.Logging;

namespace CompactGUI.Logging;

public static partial class SnackbarServiceLog
{

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid Folder {FolderName}: {message}")]
    public static partial void ShowInvalidFoldersMessage(ILogger logger, string FolderName, string message);

    [LoggerMessage(Level = LogLevel.Information, Message = "Showing insufficient permission snackbar for folder: {FolderName}")]
    public static partial void ShowInsufficientPermission(ILogger logger, string folderName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Showing update available snackbar for version: {NewVersion}, pre-release: {IsPreRelease}")]
    public static partial void ShowUpdateAvailable(ILogger logger, string newVersion, bool isPreRelease);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Showing failed to submit to wiki snackbar.")]
    public static partial void ShowFailedToSubmitToWiki(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Showing submitted to wiki snackbar for UID: {UID}, Game: {GameName}, SteamID: {SteamID}, Compression: {CompressionMode}")]
    public static partial void ShowSubmittedToWiki(ILogger logger, string UID, string GameName, string SteamID, string CompressionMode);

    [LoggerMessage(Level = LogLevel.Information, Message = "Showing applied to all folders snackbar.")]
    public static partial void ShowAppliedToAllFolders(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Showing cannot remove folder snackbar.")]
    public static partial void ShowCannotRemoveFolder(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Showing added to queue snackbar.")]
    public static partial void ShowAddedToQueue(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Showing DirectStorage warning for: {DisplayName}")]
    public static partial void ShowDirectStorageWarning(ILogger logger, string displayName);

}
