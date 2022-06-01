Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Text.Json.Serialization

Public Class WikiHandler

    Shared filePath As String = "./CGUI.json"

    Shared Async Function ParseData(appid As Integer) As Task(Of (estimatedRatio As Decimal, confidence As Integer))

        Dim jStream As IO.FileStream = IO.File.OpenRead(filePath)
        Dim parsedSteamWikiResults = Await JsonSerializer.DeserializeAsync(Of List(Of SteamResultsData))(jStream, New JsonSerializerOptions With {.IncludeFields = True}).ConfigureAwait(False)

        Dim workingGame = parsedSteamWikiResults.Find(Function(game) game.SteamID = appid)

        If workingGame Is Nothing Then Return Nothing

        Dim estimatedRatio As Decimal

        Dim totaldataPoints As Integer = workingGame.CompressionResults.Sum(Function(x) x.TotalResults)

        For Each compressionResult In workingGame.CompressionResults
            Dim ratio = compressionResult.AfterBytes / compressionResult.BeforeBytes

            estimatedRatio += ratio * compressionResult.TotalResults

        Next

        estimatedRatio /= totaldataPoints

        Return (estimatedRatio, workingGame.Confidence)


    End Function

End Class


Public Class SteamResultsData

    Public SteamID As Integer
    Public GameName As String
    Public FolderName As String
    Public Confidence As Integer '0=Low, 1=Moderate, 2=High
    Public CompressionResults As New List(Of CompressionResult)

End Class

Public Class CompressionResult

    Public CompType As String
    Public BeforeBytes As Long
    Public AfterBytes As Long
    Public TotalResults As Integer

End Class
