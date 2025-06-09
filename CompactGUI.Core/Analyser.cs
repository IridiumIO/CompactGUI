using System.Collections.Concurrent;
using System.Diagnostics;

namespace CompactGUI.Core;

public class Analyser
{

    public string FolderName { get; set; }
    public long UncompressedBytes { get; set; }
    public long CompressedBytes { get; set; }
    public bool ContainsCompressedFiles { get; set; }
    public List<AnalysedFileDetails> FileCompressionDetailsList { get; set; } = new List<AnalysedFileDetails>();


    public Analyser(string folder)
    {
        FolderName = folder;
        UncompressedBytes = 0;
        CompressedBytes = 0;
        ContainsCompressedFiles = false;
    }


    public async Task<Boolean?> AnalyseFolder(CancellationToken cancellationToken)
    {
        try
        {
            var allFiles = await Task.Run(() => Directory.EnumerateFiles(FolderName, "*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true }).AsShortPathNames(), cancellationToken).ConfigureAwait(false);
            var fileDetails = allFiles
                .AsParallel()
                .WithCancellation(cancellationToken)
                .Select(AnalyseFile)
                .OfType<AnalysedFileDetails>()
                .ToList();

            CompressedBytes = fileDetails.Sum(f => f.CompressedSize);
            UncompressedBytes = fileDetails.Sum(f => f.UncompressedSize);
            ContainsCompressedFiles = fileDetails.Any(f => f.CompressionMode != WOFCompressionAlgorithm.NO_COMPRESSION);

            FileCompressionDetailsList = fileDetails;

            return ContainsCompressedFiles;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }



    }


    private AnalysedFileDetails? AnalyseFile(string file)
    {
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
        catch (IOException)
        {
            return null;
        }
    }


    public List<ExtensionResult> GetPoorlyCompressedExtensions()
    {
        // Only use PLINQ if the list is large enough to benefit from parallel processing
        IEnumerable<AnalysedFileDetails> query = FileCompressionDetailsList.Count <= 10000
            ? FileCompressionDetailsList
            : FileCompressionDetailsList.AsParallel();

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

}


