Public Class AnalysedFileDetails

    Public Property FileName As String
    Public Property UncompressedSize As Long
    Public Property CompressedSize As Long
    Public Property CompressionMode As WOFCompressionAlgorithm
    Public Property FileInfo As IO.FileInfo
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

Public Structure CompressionProgress
    Public ProgressPercent As Integer
    Public FileName As String

    Public Sub New(_progressPercent As Integer, _fileName As String)
        ProgressPercent = _progressPercent
        FileName = _fileName
    End Sub

End Structure



Public Enum CompressionMode
    XPRESS4K
    XPRESS8K
    XPRESS16K
    LZX
    None
End Enum

Public Enum WOFCompressionAlgorithm
    NO_COMPRESSION = -2
    LZNT1 = -1
    XPRESS4K = 0
    LZX = 1
    XPRESS8K = 2
    XPRESS16K = 3
End Enum