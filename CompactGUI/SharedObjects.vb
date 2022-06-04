''' <summary>
''' Shared objects between multiple code regions
''' </summary>


Public Class ActiveFolder

    Public folderName As String
    Public analysisResults As List(Of FileDetails)
    Public poorlyCompressedFiles As List(Of ExtensionResults)
    Public steamAppID As Integer

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
    Public Property PoorlyCompressedExt As List(Of ExtensionResults)

End Class


' Object to get results from existing wiki file
Public Class SteamResultsData

    Public SteamID As Integer
    Public GameName As String
    Public FolderName As String
    Public Confidence As Integer '0=Low, 1=Moderate, 2=High
    Public CompressionResults As New List(Of CompressionResult)

End Class


' Used to hold compression results from parsed existing wiki file (above)
Public Class CompressionResult

    Public CompType As String
    Public BeforeBytes As Long
    Public AfterBytes As Long
    Public TotalResults As Integer

End Class


' For each file in a folder, holds both pre-and post-compression results
Public Class FileDetails

    Public FileName As String
    Public UncompressedSize As Long
    Public CompressedSize As Long


End Class

'Used to track efficiency of compression and built results for submission to wiki
Public Class ExtensionResults

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