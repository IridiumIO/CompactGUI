using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactGUI.Logging.Core;

public static partial class UncompactorLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting decompression of {FileCount} files with {MaxParallelism} threads")]
    public static partial void StartingDecompression(ILogger logger, int fileCount, int maxParallelism);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Processing file: {FileName}")]
    public static partial void ProcessingFile(ILogger logger, string fileName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "File decompression failed for: {FileName} with error: {ErrorMessage}")]
    public static partial void FileDecompressionFailed(ILogger logger, string fileName, string errorMessage);

    [LoggerMessage(Level = LogLevel.Information, Message = "Decompression paused.")]
    public static partial void DecompressionPaused(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Decompression resumed.")]
    public static partial void DecompressionResumed(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Decompression canceled.")]
    public static partial void DecompressionCanceled(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Decompression completed successfully in {TimeTaken}s.")]
    public static partial void DecompressionCompleted(ILogger logger, double timeTaken);
}
