using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactGUI.Logging.Core;

public static partial class CompactorLog
{

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting compression in directory: {Directory} with algorithm: {Algorithm} and using {MaxParallelism} threads")]
    public static partial void StartingCompression(ILogger logger, string directory, string algorithm, int maxParallelism);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Building working files list for directory: {Directory}")]
    public static partial void BuildingWorkingFilesList(ILogger logger, string directory);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Processing file: {FileName} ({UncompressedSize} bytes)")]
    public static partial void ProcessingFile(ILogger logger, string fileName, long uncompressedSize);

    [LoggerMessage(Level = LogLevel.Warning, Message = "File compression failed for: {FileName} with error: {ErrorMessage}")]
    public static partial void FileCompressionFailed(ILogger logger, string fileName, string errorMessage);

    [LoggerMessage(Level = LogLevel.Information, Message = "Compression paused.")]
    public static partial void CompressionPaused(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Compression resumed.")]
    public static partial void CompressionResumed(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Compression canceled.")]
    public static partial void CompressionCanceled(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Compression completed successfully in {TimeTaken}s.")]
    public static partial void CompressionCompleted(ILogger logger, double timeTaken);

    [LoggerMessage(Level = LogLevel.Error, Message = "Compression failed with error: {ErrorMessage}")]
    public static partial void CompressionFailed(ILogger logger, string errorMessage);

}
