
Imports Microsoft.Toolkit.Mvvm.ComponentModel
''' <summary>
''' Shared objects between multiple code regions
''' </summary>
Public Class ActiveFolder : Inherits ObservableObject

    Public Property folderName As String
    Public Property analysisResults As List(Of Core.AnalysedFileDetails)
    Public Property poorlyCompressedFiles As List(Of Core.ExtensionResult)
    Public Property steamAppID As Integer
    Public Property WikiPoorlyCompressedFiles As List(Of String)
    Public Property UncompressedBytes As Long
    Public Property CompressedBytes As Long
    Public Property SelectedCompressionMode = 0

    Public Property IsFreshlyCompressed As Boolean = False

    Public ReadOnly Property CompressionRatio As Decimal
        Get
            If UncompressedBytes = 0 OrElse CompressedBytes = 0 Then Return 0
            Return CompressedBytes / UncompressedBytes
        End Get
    End Property

End Class


' Object used to build submission data to send online after compression
Public Class SteamSubmissionData
    Public Property UID As String
    Public Property SteamID As Integer
    Public Property GameName As String
    Public Property FolderName As String
    Public Property CompressionMode As Integer
    Public Property BeforeBytes As Long
    Public Property AfterBytes As Long
    Public Property PoorlyCompressedExt As List(Of Core.ExtensionResult)

End Class


' Object to get results from existing wiki file
Public Class SteamResultsData

    Public SteamID As Integer
    Public GameName As String
    Public FolderName As String
    Public Confidence As Integer '0=Low, 1=Moderate, 2=High
    Public CompressionResults As New List(Of CompressionResult)
    Public PoorlyCompressedExtensions As Dictionary(Of String, Integer)

End Class


' Used to hold compression results from parsed existing wiki file (above)
Public Class CompressionResult

    Public CompType As Integer
    Public BeforeBytes As Long
    Public AfterBytes As Long
    Public TotalResults As Integer

End Class

