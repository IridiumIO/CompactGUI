Public Class AnalysedFileDetails

    Public FileName As String
    Public UncompressedSize As Long
    Public CompressedSize As Long
    Public CompressionMode As CompressionAlgorithm

End Class

Public Enum CompressionAlgorithm
    NO_COMPRESSION = -2
    LZNT1 = -1
    XPRESS4K = 0
    LZX = 1
    XPRESS8K = 2
    XPRESS16K = 3
End Enum