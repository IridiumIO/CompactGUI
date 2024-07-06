
' Object to get results from existing wiki file
Public Class SteamResultsData

    Public SteamID As Integer
    Public GameName As String
    Public FolderName As String
    Public Confidence As Integer '0=Low, 1=Moderate, 2=High
    Public CompressionResults As New List(Of CompressionResult)
    Public PoorlyCompressedExtensions As Dictionary(Of String, Integer)

End Class

