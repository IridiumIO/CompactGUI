Imports System.Net.Http
Imports System.Text.Json

Public Interface IWikiService
    Function GetUpdatedJSONAsync() As Task
    Function ParseData(appid As Integer) As Task(Of (estimatedRatio As Decimal, confidence As Integer, poorlyCompressedList As Dictionary(Of String, Integer), compressionResults As List(Of CompressionResult)))
    Function SubmitToWiki(folderpath As String, analysisResults As List(Of Core.AnalysedFileDetails), poorlyCompressedFiles As List(Of Core.ExtensionResult), compressionMode As Integer) As Task(Of Boolean)
    Function SubmitURLForm(url As String, submissionstring As String) As Task(Of Boolean)
End Interface

Public Class WikiService : Implements IWikiService

    Private ReadOnly filePath = IO.Path.Combine(SettingsHandler.DataFolder.FullName, "databasev2.json")
    Private ReadOnly dlPath As String = "https://raw.githubusercontent.com/IridiumIO/CompactGUI/database/database.json"


    Async Function GetUpdatedJSONAsync() As Task Implements IWikiService.GetUpdatedJSONAsync
        Debug.WriteLine("Updating JSON file")
        Dim JSONFile As New IO.FileInfo(filePath)

        If JSONFile.Exists AndAlso SettingsHandler.AppSettings.ResultsDBLastUpdated.AddHours(6) >= DateTime.Now Then Return

        Dim httpClient As New HttpClient

        Try

            Dim res = Await httpClient.GetStreamAsync(dlPath)

            Using fs As New IO.FileStream(JSONFile.FullName, IO.FileMode.Create)
                Await res.CopyToAsync(fs)
            End Using

        Catch ex As IO.IOException
            Debug.WriteLine("Could not update JSON file: file is in use.")
            Return
        Catch ex As HttpRequestException
            Debug.WriteLine($"Unable to reach endpoint. Likely no internet connection")
            Return
        Finally
            HttpClient.Dispose()
        End Try


        SettingsHandler.AppSettings.ResultsDBLastUpdated = DateTime.Now
        Settings.Save()
        Debug.WriteLine("Updated JSON file")

    End Function

    Private ReadOnly JsonDefaultSettings As New JsonSerializerOptions With {.IncludeFields = True}
    Async Function ParseData(appid As Integer) As Task(Of (estimatedRatio As Decimal, confidence As Integer, poorlyCompressedList As Dictionary(Of String, Integer), compressionResults As List(Of CompressionResult))) Implements IWikiService.ParseData
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


    Async Function SubmitToWiki(folderpath As String, analysisResults As List(Of Core.AnalysedFileDetails), poorlyCompressedFiles As List(Of Core.ExtensionResult), compressionMode As Integer) As Task(Of Boolean) Implements IWikiService.SubmitToWiki
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

        Dim snackbar = Application.GetService(Of CustomSnackBarService)()
        If Not response Then
            snackbar.ShowFailedToSubmitToWiki()
            Return False
        End If

        snackbar.ShowSubmittedToWiki(steamsubmitdata, compressionMode)
        Return True

    End Function


    Async Function SubmitURLForm(url As String, submissionstring As String) As Task(Of Boolean) Implements IWikiService.SubmitURLForm
        Try

            Dim httpC As New HttpClient
            Dim resp = Await httpC.GetAsync(New Uri(url & submissionstring))
            Return resp.StatusCode
        Catch ex As Exception
            Return 0
        End Try

    End Function

End Class
