Imports System.Net.Http
Imports System.Text.Json

Public Class WikiHandler

    Shared ReadOnly filePath = IO.Path.Combine(SettingsHandler.DataFolder.FullName, "databasev2.json")


    Shared Async Function GetUpdatedJSONAsync() As Task

        Dim dlPath As String = "https://raw.githubusercontent.com/IridiumIO/CompactGUI/database/database.json"

        Dim JSONFile As New IO.FileInfo(filePath)

        If JSONFile.Exists AndAlso SettingsHandler.AppSettings.ResultsDBLastUpdated.AddHours(6) >= DateTime.Now Then Return

        Dim httpClient As New HttpClient
        Dim res = Await httpClient.GetStreamAsync(dlPath)

        Using fs As New IO.FileStream(JSONFile.FullName, IO.FileMode.Create)
            Await res.CopyToAsync(fs)
        End Using

        httpClient.Dispose()

        SettingsHandler.AppSettings.ResultsDBLastUpdated = DateTime.Now
        Settings.Save()


    End Function

    Private Shared ReadOnly JsonDefaultSettings As New JsonSerializerOptions With {.IncludeFields = True}
    Shared Async Function ParseData(appid As Integer) As Task(Of (estimatedRatio As Decimal, confidence As Integer, poorlyCompressedList As Dictionary(Of String, Integer), compressionResults As List(Of CompressionResult)))

        Dim JSONFile As New IO.FileInfo(filePath)
        If Not JSONFile.Exists Then Return Nothing

        Dim jStream As IO.FileStream = JSONFile.OpenRead
        Dim parsedSteamWikiResults = Await JsonSerializer.DeserializeAsync(Of List(Of SteamResultsData))(jStream, JsonDefaultSettings).ConfigureAwait(False)
        Dim workingGame = parsedSteamWikiResults.Find(Function(game) game.SteamID = appid)

        If workingGame Is Nothing Then Return Nothing
        Dim estimatedRatio As Decimal
        Dim totaldataPoints As Integer = workingGame.CompressionResults.Sum(Function(x) x.TotalResults)

        For Each compressionResult In workingGame.CompressionResults
            Dim ratio = compressionResult.AfterBytes / compressionResult.BeforeBytes
            estimatedRatio += ratio * compressionResult.TotalResults
        Next

        workingGame.CompressionResults.Sort(Function(x, y) x.CompType.CompareTo(y.CompType))

        'TODO: Adjust this return to account for selected level of aggressiveness in settings
        'Dim poorlyCompressedExt = workingGame.PoorlyCompressedExtensions.Where(Function(k) k.Value > 100).Select(Function(k) k.Key)

        estimatedRatio /= totaldataPoints
        Return (estimatedRatio, workingGame.Confidence, workingGame.PoorlyCompressedExtensions, workingGame.CompressionResults)

    End Function


    Shared Async Function SubmitToWiki(folderpath As String, analysisResults As List(Of Core.AnalysedFileDetails), poorlyCompressedFiles As List(Of Core.ExtensionResult), compressionMode As Integer) As Task(Of Boolean)

        Dim wikiSubmitURI = "https://docs.google.com/forms/d/e/1FAIpQLSdQyMwHIfldsuKKdDYBE9DNEyro8bidBDInq8EafGogFu382A/formResponse?entry.1019946248=%3CCompactGUI3%3E"

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
            .Content = $"UID: {steamsubmitdata.UID & vbCrLf}Game: {steamsubmitdata.GameName & vbCrLf}SteamID: {steamsubmitdata.SteamID & vbCrLf}Compression: {[Enum].GetName(GetType(Core.WOFCompressionAlgorithm), Core.WOFConvertCompressionLevel(compressionMode))}",
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
