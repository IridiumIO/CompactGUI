Imports System.Net.Http
Imports System.Text.Json

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


    Shared Async Function SubmitToWiki(folderpath As String, analysisResults As List(Of FileDetails), poorlyCompressedFiles As List(Of ExtensionResults), compressionMode As Integer) As Task(Of Boolean)

        Dim wikiSubmitURI = "https://docs.google.com/forms/d/e/1FAIpQLSdQyMwHIfldsuKKdDYBE9DNEyro8bidBDInq8EafGogFu382A/formResponse?entry.1019946248=%3CCompactGUI3%3E"

        Dim activeFolder = folderpath
        Dim ret = Await Task.Run(Function() GetSteamNameAndIDFromFolder(folderpath))

        Dim before = analysisResults.Sum(Function(res) res.UncompressedSize)
        Dim after = analysisResults.Sum(Function(res) res.CompressedSize)

        Dim steamsubmitdata As New SteamSubmissionData With {
            .UID = getUID(),
            .SteamID = ret.appID,
            .GameName = ret.gameName,
            .FolderName = ret.installDir,
            .BeforeBytes = before,
            .AfterBytes = after,
            .CompressionMode = compressionMode,
            .PoorlyCompressedExt = poorlyCompressedFiles}

        Dim jstring = JsonSerializer.Serialize(steamsubmitdata)
        Dim response = Await SubmitURLForm(wikiSubmitURI, jstring)

        If Not response Then

            Dim msgFailed As New ModernWpf.Controls.ContentDialog With {
                .Title = "Failed to submit result",
                .Content = $"Please check your internet connection and try again",
                .CloseButtonText = "Close"
                }
            Await msgFailed.ShowAsync()
            Return False

        End If

        Dim msgSuccess As New ModernWpf.Controls.ContentDialog With {
            .Title = "Thank you for submitting your result",
            .Content = $"UID: {steamsubmitdata.UID & vbCrLf}Game: {steamsubmitdata.GameName & vbCrLf}SteamID: {steamsubmitdata.SteamID & vbCrLf}Compression: {Compactor.CompressionLevel(steamsubmitdata.CompressionMode)}",
            .CloseButtonText = "OK"
            }

        Await msgSuccess.ShowAsync()

        Return True

    End Function


    Shared Async Function SubmitURLForm(url As String, submissionstring As String) As Task(Of Boolean)
        Try

            Dim httpC As New HttpClient
            Dim resp = Await httpC.GetAsync(New Uri(url & submissionstring))
            Return resp.StatusCode
            If resp.StatusCode <> 200 Then Return False
            Return True
        Catch ex As Exception
            Return 0
        End Try

    End Function

End Class
