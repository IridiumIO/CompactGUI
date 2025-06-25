Imports CommunityToolkit.Mvvm.ComponentModel

Public Class CompressionStatistics : Inherits ObservableObject
    Public Property Id As Guid = Guid.NewGuid()
    Public Property FolderPath As String
    Public Property FolderName As String
    Public Property CompressionDate As DateTime = DateTime.Now
    Public Property OriginalSize As Long
    Public Property CompressedSize As Long
    Public Property CompressionAlgorithm As Integer
    Public Property CompressionTime As TimeSpan
    Public Property FileCount As Integer
    Public Property FileTypeStats As New List(Of FileTypeStatistic)
    
    Public ReadOnly Property SpaceSaved As Long
        Get
            Return OriginalSize - CompressedSize
        End Get
    End Property
    
    Public ReadOnly Property CompressionRatio As Double
        Get
            If OriginalSize = 0 Then Return 0
            Return 1 - (CompressedSize / OriginalSize)
        End Get
    End Property
    
    Public ReadOnly Property CompressionRatioPercent As Double
        Get
            Return CompressionRatio * 100
        End Get
    End Property
    
    Public ReadOnly Property SpaceSavedFormatted As String
        Get
            Return FormatBytes(SpaceSaved)
        End Get
    End Property
    
    Public ReadOnly Property OriginalSizeFormatted As String
        Get
            Return FormatBytes(OriginalSize)
        End Get
    End Property
    
    Public ReadOnly Property CompressedSizeFormatted As String
        Get
            Return FormatBytes(CompressedSize)
        End Get
    End Property
    
    Private Function FormatBytes(bytes As Long) As String
        Dim sizes() As String = {"B", "KB", "MB", "GB", "TB"}
        Dim order As Integer = 0
        Dim dblBytes As Double = bytes
        
        While dblBytes >= 1024 AndAlso order < sizes.Length - 1
            order += 1
            dblBytes /= 1024
        End While
        
        Return String.Format("{0:0.##} {1}", dblBytes, sizes(order))
    End Function
End Class

Public Class FileTypeStatistic
    Public Property FileExtension As String
    Public Property FileCount As Integer
    Public Property TotalOriginalSize As Long
    Public Property TotalCompressedSize As Long
    
    Public ReadOnly Property CompressionRatio As Double
        Get
            If TotalOriginalSize = 0 Then Return 0
            Return 1 - (TotalCompressedSize / TotalOriginalSize)
        End Get
    End Property
End Class