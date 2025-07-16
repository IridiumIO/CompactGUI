using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics;
using CompactGUI.Logging.Core;

namespace CompactGUI.Core;

public sealed class Analyser : IDisposable
{

    public string FolderName { get; set; }
    private ILogger<Analyser> _logger;
    private readonly FolderChangeMonitor _folderMonitor;
    public bool HasFolderChanged => _folderMonitor.HasChanged;
    public DateTime LastFolderChanged => _folderMonitor.LastChanged;


    public Analyser(string folder, ILogger<Analyser> logger)
    {
        FolderName = folder;
        _logger = logger;

        _folderMonitor = new FolderChangeMonitor(folder);
        _folderMonitor.Changed += (s, e) =>
            _logger.LogInformation("Folder change detected by FolderChangeMonitor for {FolderName}", FolderName);
    }


    public long CompressedBytes;
    public long UncompressedBytes;
    public bool ContainsCompressedFiles;

    private static long GetTotalCompressedBytes(List<AnalysedFileDetails> fileCompressionDetailsList)
    {
        return fileCompressionDetailsList.Sum(f => f.CompressedSize);
    }
    private static long GetTotalUncompressedBytes(List<AnalysedFileDetails> fileCompressionDetailsList)
    {
        return fileCompressionDetailsList.Sum(f => f.UncompressedSize);
    }
    private static bool GetContainsCompressedFiles(List<AnalysedFileDetails> fileCompressionDetailsList)
    {
        return fileCompressionDetailsList.Any(f => f.CompressionMode != WOFCompressionAlgorithm.NO_COMPRESSION);
    }


    private List<AnalysedFileDetails>? _analysedFileDetails;

    public async ValueTask<List<AnalysedFileDetails>?> GetAnalysedFilesAsync(CancellationToken token)
    {
        if (_analysedFileDetails != null && !HasFolderChanged)
        {
            _logger.LogInformation("Returning cached analysed files for folder {FolderName}", FolderName);
            return _analysedFileDetails;
        }

        _logger.LogInformation("Analysing folder {FolderName} for the first time or after a change", FolderName);
        _analysedFileDetails = await AnalyseFolder(token).ConfigureAwait(false);
        return _analysedFileDetails;
    }


    private async Task<List<AnalysedFileDetails>?> AnalyseFolder(CancellationToken cancellationToken)
    {

        List<AnalysedFileDetails>? AnalysedFileDetails;

        AnalyserLog.StartingAnalysis(_logger, FolderName);
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            var allFiles = await Task.Run(() => Directory.EnumerateFiles(FolderName, "*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint }).AsShortPathNames(), cancellationToken).ConfigureAwait(false);
            var fileDetails = allFiles
                .AsParallel()
                .WithCancellation(cancellationToken)
                .Select(AnalyseFile)
                .OfType<AnalysedFileDetails>()
                .ToList();

            AnalysedFileDetails = fileDetails;
        }
        catch (Exception ex)
        {
            AnalyserLog.AnalysisFailed(_logger, FolderName, ex.Message);
            return null;
        }
        finally { sw.Stop(); }
        
        _folderMonitor.Reset();

        CompressedBytes = GetTotalCompressedBytes(AnalysedFileDetails);
        UncompressedBytes = GetTotalUncompressedBytes(AnalysedFileDetails);
        ContainsCompressedFiles = GetContainsCompressedFiles(AnalysedFileDetails);
        AnalyserLog.AnalysisCompleted(_logger, FolderName, Math.Round(sw.Elapsed.TotalSeconds, 3), CompressedBytes, UncompressedBytes, ContainsCompressedFiles);

        return AnalysedFileDetails;


    }


    private AnalysedFileDetails? AnalyseFile(string file)
    {
        AnalyserLog.ProcessingFile(_logger, file);
        try
        {
            FileInfo fileInfo = new FileInfo(file);
            long uncompressedSize = fileInfo.Length;
            long compressedSize = SharedMethods.GetFileSizeOnDisk(file);
            compressedSize = compressedSize < 0 ? 0 : compressedSize;
            WOFCompressionAlgorithm compressionMode = (compressedSize == uncompressedSize)
                ? WOFCompressionAlgorithm.NO_COMPRESSION
                : WOFHelper.DetectCompression(fileInfo);

            return new AnalysedFileDetails { FileName = file, CompressedSize = compressedSize, UncompressedSize = uncompressedSize, CompressionMode = compressionMode, FileInfo = fileInfo };
        }
        catch (IOException ex)
        {
            AnalyserLog.ProcessingFileFailed(_logger, file, ex.Message);
            return null;
        }
    }


    public List<ExtensionResult> GetPoorlyCompressedExtensions()
    {
        // Only use PLINQ if the list is large enough to benefit from parallel processing
        IEnumerable<AnalysedFileDetails> query = _analysedFileDetails?.Count <= 10000
            ? _analysedFileDetails
            : _analysedFileDetails.AsParallel();

        return query
                .Where(fl => fl.UncompressedSize > 0)
                .GroupBy(fl => Path.GetExtension(fl.FileName), StringComparer.OrdinalIgnoreCase)
                .Select(g => new ExtensionResult
                {
                    Extension = g.Key,
                    TotalFiles = g.Count(),
                    CompressedBytes = g.Sum(fl => fl.CompressedSize),
                    UncompressedBytes = g.Sum(fl => fl.UncompressedSize)
                })
                .Where(r => r.CRatio > 0.95)
                .ToList();

    }

    public void Dispose()
    {
        _folderMonitor.Dispose();
        _analysedFileDetails?.Clear();
    }
}


