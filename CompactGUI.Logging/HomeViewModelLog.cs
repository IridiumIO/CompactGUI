using Microsoft.Extensions.Logging;

namespace CompactGUI.Logging;

public static partial class HomeViewModelLog
{

    [LoggerMessage(Level = LogLevel.Information,Message = "Loading folders from {FolderPaths}")]
    public static partial void AddingFolders(ILogger logger, IEnumerable<string> folderPaths);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid folders found: {InvalidFolders} with messages {messages}")]
    public static partial void InvalidFolders(ILogger logger, List<string> invalidFolders, IEnumerable<String> messages);


    [LoggerMessage(Level = LogLevel.Information, Message = "Getting estimated compression for {FolderPath} with uncompressed size {uncompressedSize}.")]
    public static partial void GettingEstimatedCompression(ILogger logger, string folderPath, long uncompressedSize);



    [LoggerMessage(Level = LogLevel.Information, Message = "Compressing folder {folderName}.")]
    public static partial void CompressingFolder(ILogger logger, string folderName);


    [LoggerMessage(Level = LogLevel.Information, Message = "Adding folder {folderName} to watcher")]
    public static partial void AddingFolderToWatcher(ILogger logger, string folderName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting batch compression of {count} folders")]
    public static partial void StartingBatchCompression(ILogger logger, int count);


}
