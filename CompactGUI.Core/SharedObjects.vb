Public Class AnalysedFileDetails

    Public FileName As String
    Public UncompressedSize As Long
    Public CompressedSize As Long
    Public CompressionMode As CompressionAlgorithm
    Public FileInfo As IO.FileInfo
End Class


'Used to track efficiency of compression and built results for submission to wiki
Public Class ExtensionResult

    Public Property extension As String
    Public Property uncompressedBytes As Long
    Public Property compressedBytes As Long
    Public Property totalFiles As Integer
    ReadOnly Property cRatio As Decimal
        Get
            Return Math.Round(compressedBytes / uncompressedBytes, 2)
        End Get
    End Property

End Class

Public Enum CompressionAlgorithm
    NO_COMPRESSION = -2
    LZNT1 = -1
    XPRESS4K = 0
    LZX = 1
    XPRESS8K = 2
    XPRESS16K = 3
End Enum