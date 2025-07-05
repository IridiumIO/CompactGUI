using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactGUI.Logging.Watcher;

public static partial class WatcherLog
{
    // Watcher events
    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher started.")]
    public static partial void WatcherStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Backgrounding disabled.")]
    public static partial void BackgroundingDisabled(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Backgrounding enabled.")]
    public static partial void BackgroundingEnabled(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Initializing watched folders.")]
    public static partial void InitializingWatchedFolders(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Parsing watchers (ParseAll={ParseAll}).")]
    public static partial void ParsingWatchers(ILogger logger, bool parseAll);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Parsing single watcher: {Folder}.")]
    public static partial void ParsingSingleWatcher(ILogger logger, string folder);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Watcher - Removing {Count} folders that do not exist from watcher list.")]
    public static partial void RemovingNonexistentFolders(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Watcher - Failed to deserialize watcher JSON: {ErrorMessage}")]
    public static partial void DeserializeWatcherJsonFailed(ILogger logger, string errorMessage);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - System idle detected.")]
    public static partial void SystemIdleDetected(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - System not idle.")]
    public static partial void SystemNotIdle(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Watcher - Folder {Folder} changed and will be compressed")]
    public static partial void FolderChanged(ILogger logger, string folder);









    // BackgroundCompactor events
    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Background compacting started.")]
    public static partial void BackgroundCompactingStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Background compacting finished.")]
    public static partial void BackgroundCompactingFinished(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Watcher - Compacting folder: {FolderName}.")]
    public static partial void CompactingFolder(ILogger logger, string folderName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Skipping folder (recently modified): {FolderName}.")]
    public static partial void SkippingRecentlyModifiedFolder(ILogger logger, string folderName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Finished compacting folder: {FolderName}.")]
    public static partial void FinishedCompactingFolder(ILogger logger, string folderName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Watcher - Pausing background compactor.")]
    public static partial void PausingBackgroundCompactor(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Watcher - Resuming background compactor.")]
    public static partial void ResumingBackgroundCompactor(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Watcher - Error during background compacting: {ErrorMessage}")]
    public static partial void BackgroundCompactingError(ILogger logger, string errorMessage);
}
