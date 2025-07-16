namespace CompactGUI.Core;


public sealed class AnalysedFileDetails
{
    public required string FileName { get; set; }
    public long UncompressedSize { get; set; }
    public long CompressedSize { get; set; }
    public WOFCompressionAlgorithm CompressionMode { get; set; }
    public FileInfo? FileInfo { get; set; }
}


public sealed class ExtensionResult
{
    public required string Extension { get; set; }
    public long UncompressedBytes { get; set; }
    public long CompressedBytes { get; set; }
    public int TotalFiles { get; set; }
    public double CRatio => CompressedBytes == 0 ? 0 : Math.Round((double)CompressedBytes / UncompressedBytes, 2);

}




public struct CompressionProgress
{
    public int ProgressPercent;
    public string FileName;

    public CompressionProgress(int progressPercent, string fileName)
    {
        ProgressPercent = progressPercent;
        FileName = fileName;
    }
}


public enum CompressionMode: int
{
    XPRESS4K,
    XPRESS8K,
    XPRESS16K,
    LZX,
    None
}


public enum WOFCompressionAlgorithm: int
{
    NO_COMPRESSION = -2,
    LZNT1 = -1,
    XPRESS4K = 0,
    LZX = 1,
    XPRESS8K = 2,
    XPRESS16K = 3
}