Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading

Imports CompactGUI.Core.Settings

Public Interface IWikiService
    Function GetUpdatedJSONAsync() As Task
    Function ParseData(appid As Integer) As Task(Of (estimatedRatio As Decimal, confidence As Integer, poorlyCompressedList As Dictionary(Of String, Integer), compressionResults As List(Of CompressionResult)))
    Function GetAllDatabaseCompressionResultsAsync() As Task(Of List(Of DatabaseCompressionResult))
    Function SubmitToWiki(folderpath As String, analysisResults As List(Of Core.AnalysedFileDetails), poorlyCompressedFiles As List(Of Core.ExtensionResult), compressionMode As Integer) As Task(Of Boolean)
    Function SubmitURLForm(url As String, submissionstring As String) As Task(Of Boolean)
End Interface

Public Class WikiService : Implements IWikiService

    Private ReadOnly filePath As String
    Private ReadOnly dlPath As String

    Private ReadOnly _settingsService As ISettingsService
    Private ReadOnly _databaseGate As New SemaphoreSlim(1, 1)

    Public Sub New(settingsService As ISettingsService)
        _settingsService = settingsService
        filePath = IO.Path.Combine(_settingsService.DataFolder.FullName, "databasev2.json")

        dlPath = "https://raw.githubusercontent.com/IridiumIO/CompactGUI/database/database.json"

    End Sub

    Async Function GetUpdatedJSONAsync() As Task Implements IWikiService.GetUpdatedJSONAsync
        Debug.WriteLine("Updating JSON file")
        Dim JSONFile As New IO.FileInfo(filePath)

        Await _databaseGate.WaitAsync().ConfigureAwait(False)
        Try
            If JSONFile.Exists AndAlso
               _settingsService.AppSettings.ResultsDBLastUpdated.AddHours(6) >= DateTime.Now AndAlso
               Await IsDatabaseJsonValidAsync(JSONFile.FullName).ConfigureAwait(False) Then
                Return
            End If

            Using httpClient As New HttpClient()
                Using responseStream = Await httpClient.GetStreamAsync(dlPath).ConfigureAwait(False)
                    Using fs As New IO.FileStream(JSONFile.FullName, IO.FileMode.Create)
                        Await responseStream.CopyToAsync(fs).ConfigureAwait(False)
                    End Using
                End Using
            End Using

            If Not Await IsDatabaseJsonValidAsync(JSONFile.FullName).ConfigureAwait(False) Then
                Debug.WriteLine("Downloaded database JSON is invalid.")
                IO.File.Delete(JSONFile.FullName)
                Return
            End If

            _settingsService.AppSettings.ResultsDBLastUpdated = DateTime.Now
            _settingsService.SaveSettings()
            Debug.WriteLine("Updated JSON file")

        Catch ex As Exception When TypeOf ex Is TaskCanceledException OrElse
                                   TypeOf ex Is IO.IOException OrElse
                                   TypeOf ex Is HttpRequestException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Debug.WriteLine($"Could not update database JSON: {ex.Message}")
        Finally
            _databaseGate.Release()
        End Try
    End Function

    Private Async Function IsDatabaseJsonValidAsync(path As String) As Task(Of Boolean)
        If Not IO.File.Exists(path) Then Return False

        Try
            Using stream = IO.File.OpenRead(path)
                Using document = Await JsonDocument.ParseAsync(stream).ConfigureAwait(False)
                    Return document.RootElement.ValueKind = JsonValueKind.Array
                End Using
            End Using
        Catch ex As Exception When TypeOf ex Is JsonException OrElse
                                   TypeOf ex Is IO.IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Debug.WriteLine($"Database JSON is invalid or unreadable: {ex.Message}")
            Return False
        End Try
    End Function

    Private ReadOnly JsonDefaultSettings As New JsonSerializerOptions With {.IncludeFields = True}

    Private Async Function ReadDatabaseAsync() As Task(Of List(Of SteamResultsData))
        Await _databaseGate.WaitAsync().ConfigureAwait(False)
        Try
            If Not IO.File.Exists(filePath) Then Return Nothing

            Using stream = IO.File.OpenRead(filePath)
                Return Await JsonSerializer.DeserializeAsync(Of List(Of SteamResultsData))(stream, JsonDefaultSettings).ConfigureAwait(False)
            End Using
        Catch ex As Exception When TypeOf ex Is JsonException OrElse
                                   TypeOf ex Is IO.IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Debug.WriteLine($"Database JSON is invalid or unreadable: {ex.Message}")
            Return Nothing
        Finally
            _databaseGate.Release()
        End Try
    End Function

    Async Function ParseData(appid As Integer) As Task(Of (estimatedRatio As Decimal, confidence As Integer, poorlyCompressedList As Dictionary(Of String, Integer), compressionResults As List(Of CompressionResult))) Implements IWikiService.ParseData
        Dim parsedSteamWikiResults = Await ReadDatabaseAsync().ConfigureAwait(False)
        If parsedSteamWikiResults Is Nothing Then Return Nothing

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


    Public Async Function GetAllDatabaseCompressionResultsAsync() As Task(Of List(Of DatabaseCompressionResult)) Implements IWikiService.GetAllDatabaseCompressionResultsAsync
        Dim parsedResults = Await ReadDatabaseAsync().ConfigureAwait(False)
        If parsedResults Is Nothing Then Return New List(Of DatabaseCompressionResult)()

        Dim results As New List(Of DatabaseCompressionResult)
        For Each item In parsedResults
            Dim dbResult As New DatabaseCompressionResult With {
                .GameName = item.GameName,
                .SteamID = item.SteamID,
                .Confidence = CType(item.Confidence, DBResultConfidence),
                .Result_X4K = item.CompressionResults.FirstOrDefault(Function(r) r.CompType = 0),
                .Result_X8K = item.CompressionResults.FirstOrDefault(Function(r) r.CompType = 1),
                .Result_X16K = item.CompressionResults.FirstOrDefault(Function(r) r.CompType = 2),
                .Result_LZX = item.CompressionResults.FirstOrDefault(Function(r) r.CompType = 3),
                .PoorlyCompressedExtensions = item.PoorlyCompressedExtensions?.Select(Function(kvp) New DBPoorlyCompressedExtension With {.Extension = kvp.Key, .Count = kvp.Value}).ToList()
            }
            results.Add(dbResult)
        Next

        Return results
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
