using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactGUI.Logging.Core;

public static partial class AnalyserLog
{

    [LoggerMessage(Level = LogLevel.Debug, Message = "Starting analysis of directory: {Directory}")]
    public static partial void StartingAnalysis(ILogger logger, string directory);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Processing file: {FileName}")]
    public static partial void ProcessingFile(ILogger logger, string fileName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Processing file failed: {FileName} with message: {Message}")]
    public static partial void ProcessingFileFailed(ILogger logger, string fileName, string message);


    [LoggerMessage(Level = LogLevel.Trace, Message = "Processing folder: {FolderName}")]
    public static partial void ProcessingFolder(ILogger logger, string folderName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Analysis failed for: {Path} with error: {ErrorMessage}")]
    public static partial void AnalysisFailed(ILogger logger, string path, string errorMessage);

    [LoggerMessage(Level = LogLevel.Information, Message = "Analysis completed for directory: {Directory} in {TimeTaken}s. Uncompressed Size: {UncompressedBytes}b, CompressedSize: {CompressedBytes}b, ContainsCompressedFiles: {ContainsCompressedFiles}")]
    public static partial void AnalysisCompleted(ILogger logger, string directory, double timeTaken, long compressedBytes, long uncompressedBytes, bool containsCompressedFiles);
}
