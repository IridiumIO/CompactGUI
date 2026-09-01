Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging

Imports Gameloop.Vdf
Imports Gameloop.Vdf.JsonConverter

Imports CompactGUI.Core.Settings

Imports Microsoft.Extensions.Logging

Imports Wpf.Ui

Public Class SteamMonitorViewModel : Inherits ObservableObject

    Private ReadOnly _wikiService As IWikiService
    Private ReadOnly _watcher As Watcher.Watcher
    Private ReadOnly _compressableFolderService As CompressableFolderService
    Private ReadOnly _analyserLogger As ILogger(Of Core.Analyser)
    Private ReadOnly _navigationService As INavigationService
    Private ReadOnly _settingsService As ISettingsService
    Private ReadOnly _operationGate As New SemaphoreSlim(1, 1)
    Private ReadOnly _imageDownloadGate As New SemaphoreSlim(4, 4)
    Private Shared ReadOnly SteamImageClient As New HttpClient()
    Private _activeFolder As StandardFolder
    Private _activeGame As SteamDetailedResult
    Private _cancelRequested As Boolean
    Private _hasLoaded As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HasNoGames))>
    <NotifyCanExecuteChangedFor(NameOf(RefreshAllCommand))>
    Private _isLoading As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HasError))>
    Private _errorMessage As String

    <ObservableProperty>
    Private _searchText As String

    Public ReadOnly Property SteamGamesData As New ObservableCollection(Of SteamDetailedResult)
    Public ReadOnly Property FilteredSteamGames As ICollectionView

    Public ReadOnly Property HasError As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(ErrorMessage)
        End Get
    End Property

    Public ReadOnly Property HasNoGames As Boolean
        Get
            Return _hasLoaded AndAlso Not IsLoading AndAlso SteamGamesData.Count = 0
        End Get
    End Property

    Public Sub New(wikiService As IWikiService, watcher As Watcher.Watcher, compressableFolderService As CompressableFolderService, analyserLogger As ILogger(Of Core.Analyser), navigationService As INavigationService, settingsService As ISettingsService)
        _wikiService = wikiService
        _watcher = watcher
        _compressableFolderService = compressableFolderService
        _analyserLogger = analyserLogger
        _navigationService = navigationService
        _settingsService = settingsService
        FilteredSteamGames = CollectionViewSource.GetDefaultView(SteamGamesData)
        FilteredSteamGames.Filter = AddressOf FilterGames
    End Sub

    Private Sub OnSearchTextChanged(value As String)
        FilteredSteamGames.Refresh()
    End Sub

    <RelayCommand>
    Private Async Function RefreshAll() As Task
        SteamGamesData.Clear()
        _hasLoaded = False
        Await LoadGamesAsync()
    End Function

    Private Function CanRefreshAll() As Boolean
        Return Not IsLoading AndAlso _activeGame Is Nothing
    End Function

    <RelayCommand>
    Private Sub AddToCompressionQueue(game As SteamDetailedResult)
        If game Is Nothing Then Return
        WeakReferenceMessenger.Default.Send(New WatcherAddedFolderToQueueMessage(game.GamePath))
    End Sub

    <RelayCommand>
    Private Sub GoToDatabaseResults(game As SteamDetailedResult)
        If game Is Nothing Then Return
        If _navigationService.Navigate(GetType(DatabasePage)) Then WeakReferenceMessenger.Default.Send(New DatabaseSearchRequestedMessage(game.AppID))
    End Sub

    <RelayCommand>
    Private Sub CancelOperation(game As SteamDetailedResult)
        If game Is Nothing OrElse Not ReferenceEquals(game, _activeGame) Then Return
        _cancelRequested = True
        game.SetStatus("Cancelling...")
        _compressableFolderService.CancelEstimation(_activeFolder)
        If _activeFolder.FolderActionState = ActionState.Working Then _activeFolder.Compressor?.Cancel()
    End Sub

    Private Function FilterGames(value As Object) As Boolean
        If String.IsNullOrWhiteSpace(SearchText) Then Return True

        Dim game = TryCast(value, SteamDetailedResult)
        If game Is Nothing Then Return False

        Dim search = SearchText.Trim()
        Dim normalizedSearch = NormalizeSearchText(search)
        Return game.GameName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 OrElse NormalizeSearchText(game.GameName).Contains(normalizedSearch) OrElse game.AppID.ToString().Contains(search)
    End Function

    Private Shared Function NormalizeSearchText(value As String) As String
        If String.IsNullOrEmpty(value) Then Return String.Empty
        Return New String(value.Where(Function(character) Char.IsLetterOrDigit(character) OrElse Char.IsWhiteSpace(character)).ToArray()).ToLowerInvariant()
    End Function

    <RelayCommand>
    Private Sub Sort(parameter As Object)
        FilteredSteamGames.SortDescriptions.Clear()

        Select Case parameter?.ToString()
            Case "GameNameAsc"
                FilteredSteamGames.SortDescriptions.Add(New SortDescription(NameOf(SteamDetailedResult.GameName), ListSortDirection.Ascending))
            Case "GameNameDesc"
                FilteredSteamGames.SortDescriptions.Add(New SortDescription(NameOf(SteamDetailedResult.GameName), ListSortDirection.Descending))
            Case "StatusAsc"
                FilteredSteamGames.SortDescriptions.Add(New SortDescription(NameOf(SteamDetailedResult.StatusMessage), ListSortDirection.Ascending))
            Case "StatusDesc"
                FilteredSteamGames.SortDescriptions.Add(New SortDescription(NameOf(SteamDetailedResult.StatusMessage), ListSortDirection.Descending))
            Case "CurrentSizeAsc"
                FilteredSteamGames.SortDescriptions.Add(New SortDescription(NameOf(SteamDetailedResult.CurrentFolderSize), ListSortDirection.Ascending))
            Case "CurrentSizeDesc"
                FilteredSteamGames.SortDescriptions.Add(New SortDescription(NameOf(SteamDetailedResult.CurrentFolderSize), ListSortDirection.Descending))
            Case "SavingsAsc"
                FilteredSteamGames.SortDescriptions.Add(New SortDescription(NameOf(SteamDetailedResult.DisplayedSavings), ListSortDirection.Ascending))
            Case "SavingsDesc"
                FilteredSteamGames.SortDescriptions.Add(New SortDescription(NameOf(SteamDetailedResult.DisplayedSavings), ListSortDirection.Descending))
        End Select
    End Sub

    Public Async Function LoadGamesAsync() As Task
        If _hasLoaded OrElse IsLoading Then Return

        Dim imageLoadTasks As New List(Of Task)
        IsLoading = True
        ErrorMessage = Nothing

        Try
            Dim steamFolder = GetSteamFolderFromRegistry()
            Dim games = Await Task.Run(Function() GetInstalledSteamGames(steamFolder))
            Dim databaseResults As List(Of DatabaseCompressionResult)

            Try
                databaseResults = Await _wikiService.GetAllDatabaseCompressionResultsAsync()
            Catch ex As Exception
                databaseResults = New List(Of DatabaseCompressionResult)
                ErrorMessage = $"Compression recommendations could not be loaded: {ex.Message}"
            End Try

            Dim databaseByAppId = databaseResults.GroupBy(Function(result) result.SteamID).ToDictionary(Function(group) group.Key, Function(group) group.First())

            For Each game In games.OrderBy(Function(item) item.GameName)
                If Not Directory.Exists(game.InstallDirectory) Then Continue For

                Dim databaseResult As DatabaseCompressionResult = Nothing
                databaseByAppId.TryGetValue(game.AppID, databaseResult)

                Dim watchedFolder = _watcher.WatchedFolders.FirstOrDefault(Function(folder) String.Equals(folder.Folder, game.InstallDirectory, StringComparison.OrdinalIgnoreCase))
                Dim detailedResult = CreateDetailedResult(game, databaseResult, watchedFolder)
                Await AnalyseGameAsync(detailedResult)
                If detailedResult.CurrentFolderSize > 0 Then
                    SteamGamesData.Add(detailedResult)
                    imageLoadTasks.Add(LoadGameHeaderAsync(detailedResult, steamFolder))
                End If
            Next
        Catch ex As Exception
            ErrorMessage = $"Steam games could not be loaded: {ex.Message}"
        Finally
            _hasLoaded = True
            IsLoading = False
        End Try

        If imageLoadTasks.Count > 0 Then Await Task.WhenAll(imageLoadTasks)
    End Function

    Private Async Function LoadGameHeaderAsync(game As SteamDetailedResult, steamFolder As DirectoryInfo) As Task
        If game.AppID = 0 Then Return

        Dim imageDirectory = Path.Combine(_settingsService.DataFolder.FullName, "SteamCache")
        Dim imagePath = Path.Combine(imageDirectory, $"{game.AppID}_header.jpg")

        Try
            Directory.CreateDirectory(imageDirectory)

            If File.Exists(imagePath) Then
                Try
                    game.HeaderImage = LoadImageFromDisk(imagePath)
                    Return
                Catch ex As Exception
                    File.Delete(imagePath)
                End Try
            End If

            Await _imageDownloadGate.WaitAsync()
            Try
                If File.Exists(imagePath) Then
                    game.HeaderImage = LoadImageFromDisk(imagePath)
                    Return
                End If

                Dim steamCachedHeader = FindSteamCachedHeader(steamFolder, game.AppID)
                If steamCachedHeader IsNot Nothing Then
                    Try
                        Dim cachedImageData = Await File.ReadAllBytesAsync(steamCachedHeader)
                        game.HeaderImage = LoadImageFromMemoryStream(cachedImageData)
                        Await File.WriteAllBytesAsync(imagePath, cachedImageData)
                        Return
                    Catch ex As Exception
                        Diagnostics.Debug.WriteLine($"Failed to use Steam's cached header for {game.AppID}: {ex.Message}")
                    End Try
                End If

                Dim imageData = Await TryDownloadImageAsync($"https://steamcdn-a.akamaihd.net/steam/apps/{game.AppID}/header.jpg")
                Dim headerImage As BitmapImage = Nothing
                If imageData IsNot Nothing Then
                    Try
                        headerImage = LoadImageFromMemoryStream(imageData)
                    Catch ex As Exception
                        imageData = Nothing
                    End Try
                End If

                If imageData Is Nothing Then
                    Dim storeHeaderUrl = Await GetStoreHeaderUrlAsync(game.AppID)
                    imageData = Await TryDownloadImageAsync(storeHeaderUrl)
                    If imageData IsNot Nothing Then headerImage = LoadImageFromMemoryStream(imageData)
                End If

                If imageData Is Nothing Then Return
                game.HeaderImage = headerImage
                Await File.WriteAllBytesAsync(imagePath, imageData)
            Finally
                _imageDownloadGate.Release()
            End Try
        Catch ex As Exception
            Diagnostics.Debug.WriteLine($"Failed to load Steam header for {game.AppID}: {ex.Message}")
        End Try
    End Function

    Private Shared Function FindSteamCachedHeader(steamFolder As DirectoryInfo, appId As Integer) As String
        If steamFolder Is Nothing Then Return Nothing

        Dim appCacheDirectory = Path.Combine(steamFolder.FullName, "appcache", "librarycache", appId.ToString())
        If Not Directory.Exists(appCacheDirectory) Then Return Nothing

        Try
            Dim directories = {appCacheDirectory}.Concat(Directory.EnumerateDirectories(appCacheDirectory))
            Return directories.SelectMany(Function(directoryPath) Directory.EnumerateFiles(directoryPath, "*header*", SearchOption.TopDirectoryOnly)).Where(AddressOf IsSupportedHeaderImage).OrderBy(Function(filePath) If(String.Equals(Path.GetFileName(filePath), "library_header.jpg", StringComparison.OrdinalIgnoreCase), 0, 1)).FirstOrDefault()
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return Nothing
        End Try
    End Function

    Private Shared Function IsSupportedHeaderImage(filePath As String) As Boolean
        Select Case Path.GetExtension(filePath).ToLowerInvariant()
            Case ".jpg", ".jpeg", ".png"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Shared Async Function TryDownloadImageAsync(url As String) As Task(Of Byte())
        If String.IsNullOrWhiteSpace(url) Then Return Nothing

        Try
            Using response = Await SteamImageClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
                If Not response.IsSuccessStatusCode Then Return Nothing
                Return Await response.Content.ReadAsByteArrayAsync()
            End Using
        Catch ex As Exception When TypeOf ex Is HttpRequestException OrElse TypeOf ex Is TaskCanceledException
            Return Nothing
        End Try
    End Function

    Private Shared Async Function GetStoreHeaderUrlAsync(appId As Integer) As Task(Of String)
        Dim detailsUrl = $"https://store.steampowered.com/api/appdetails?appids={appId}"

        Try
            Using response = Await SteamImageClient.GetAsync(detailsUrl, HttpCompletionOption.ResponseHeadersRead)
                If Not response.IsSuccessStatusCode Then Return Nothing

                Using responseStream = Await response.Content.ReadAsStreamAsync()
                    Using document = Await JsonDocument.ParseAsync(responseStream)
                        Dim appDetails As JsonElement
                        Dim data As JsonElement
                        Dim headerImage As JsonElement

                        If Not document.RootElement.TryGetProperty(appId.ToString(), appDetails) Then Return Nothing
                        If Not appDetails.TryGetProperty("data", data) Then Return Nothing
                        If Not data.TryGetProperty("header_image", headerImage) Then Return Nothing
                        Return headerImage.GetString()
                    End Using
                End Using
            End Using
        Catch ex As Exception When TypeOf ex Is HttpRequestException OrElse TypeOf ex Is TaskCanceledException OrElse TypeOf ex Is JsonException
            Return Nothing
        End Try
    End Function

    Private Shared Function CreateDetailedResult(game As SteamACFResult, databaseResult As DatabaseCompressionResult, watchedFolder As Watcher.WatchedFolder) As SteamDetailedResult
        Dim wikiResults As WikiCompressionResults = Nothing
        Dim poorlyCompressedFiles As New List(Of String)

        If databaseResult IsNot Nothing Then
            wikiResults = New WikiCompressionResults(New List(Of CompressionResult)) With {
                .XPress4K = databaseResult.Result_X4K,
                .XPress8K = databaseResult.Result_X8K,
                .XPress16K = databaseResult.Result_X16K,
                .LZX = databaseResult.Result_LZX
            }

            poorlyCompressedFiles = databaseResult.PoorlyCompressedExtensions?.Where(Function(item) item.Count > 100 AndAlso Not String.IsNullOrWhiteSpace(item.Extension)).Select(Function(item) item.Extension).ToList()
            If poorlyCompressedFiles Is Nothing Then poorlyCompressedFiles = New List(Of String)
        End If

        Return New SteamDetailedResult(game.GameName, game.InstallDirectory, game.AppID, game.LastUpdated, game.HasPendingUpdate, watchedFolder?.LastCompressedDate, wikiResults, poorlyCompressedFiles)
    End Function

    Private Async Function AnalyseGameAsync(game As SteamDetailedResult) As Task
        Using analyser As New Core.Analyser(game.GamePath, _analyserLogger)
            Dim analysedFiles = Await analyser.GetAnalysedFilesAsync(CancellationToken.None)
            If analysedFiles Is Nothing Then Return
            Dim compressionLevel = If(analyser.ContainsCompressedFiles, analysedFiles.Max(Function(file) file.CompressionMode), Core.WOFCompressionAlgorithm.NO_COMPRESSION)
            game.UpdateAnalysis(analyser.UncompressedBytes, analyser.CompressedBytes, compressionLevel)
        End Using
    End Function

    <RelayCommand>
    Private Async Function Compress(game As SteamDetailedResult) As Task
        If game Is Nothing OrElse Not game.CanCompress Then Return
        Await RunGameOperationAsync(game, False)
    End Function

    <RelayCommand>
    Private Async Function Uncompress(game As SteamDetailedResult) As Task
        If game Is Nothing OrElse Not game.CanUncompress Then Return
        Await RunGameOperationAsync(game, True)
    End Function

    Private Async Function RunGameOperationAsync(game As SteamDetailedResult, uncompress As Boolean) As Task

        Await _operationGate.WaitAsync()
        Try
            If (uncompress AndAlso Not game.CanUncompress) OrElse (Not uncompress AndAlso Not game.CanCompress) Then Return
            _cancelRequested = False
            game.SetWorking(True, If(uncompress, "Uncompressing...", "Compressing..."))
            Await RunFolderOperationAsync(game, uncompress)
            If _cancelRequested Then game.SetStatus("Operation cancelled.")
        Catch ex As Exception
            game.SetWorking(False, If(_cancelRequested OrElse TypeOf ex Is OperationCanceledException, "Operation cancelled.", $"Operation failed: {ex.Message}"))
        Finally
            _cancelRequested = False
            _activeGame = Nothing
            RefreshAllCommand.NotifyCanExecuteChanged()
            _operationGate.Release()
        End Try
    End Function

    Private Async Function RunFolderOperationAsync(game As SteamDetailedResult, uncompress As Boolean) As Task
        Dim folder As New StandardFolder(game.GamePath)
        _activeFolder = folder
        _activeGame = game
        RefreshAllCommand.NotifyCanExecuteChanged()
        Dim backgroundingDisabled As Boolean
        Dim sleepPrevented As Boolean
        Dim operationException As Exception = Nothing

        Try
            Await _watcher.DisableBackgrounding()
            backgroundingDisabled = True
            If _cancelRequested Then Throw New OperationCanceledException()
            Core.SharedMethods.PreventSleep()
            sleepPrevented = True

            folder.WikiPoorlyCompressedFiles = game.WikiPoorlyCompressedFiles
            folder.CompressionOptions.SkipUserSubmittedFiletypes = folder.WikiPoorlyCompressedFiles.Count > 0
            Dim analysisResult = Await _compressableFolderService.AnalyseFolderAsync(folder)
            If analysisResult = -1 Then Throw New UnauthorizedAccessException("CompactGUI does not have permission to modify this folder.")
            If analysisResult <> 0 OrElse _cancelRequested Then Throw New OperationCanceledException()

            Dim isCurrentlyCompressed = folder.AnalysisResults.Any(Function(file) file.CompressionMode <> Core.WOFCompressionAlgorithm.NO_COMPRESSION)
            Dim succeeded As Boolean

            If uncompress Then
                If Not isCurrentlyCompressed Then Throw New InvalidOperationException("This game is not currently compressed.")
                succeeded = Await _compressableFolderService.UncompressFolder(folder)
            Else
                If Not game.RecommendedCompressionMode.HasValue Then Throw New InvalidOperationException("This game does not have a compression recommendation.")
                folder.CompressionOptions.SelectedCompressionMode = game.RecommendedCompressionMode.Value
                succeeded = Await _compressableFolderService.CompressFolder(folder)
                Await _compressableFolderService.AnalyseFolderAsync(folder)
            End If

            Dim compressionLevel = If(folder.AnalysisResults.Any(Function(file) file.CompressionMode <> Core.WOFCompressionAlgorithm.NO_COMPRESSION), folder.AnalysisResults.Max(Function(file) file.CompressionMode), Core.WOFCompressionAlgorithm.NO_COMPRESSION)
            game.UpdateAnalysis(folder.UncompressedBytes, folder.CompressedBytes, compressionLevel)

            If folder.Analyser IsNot Nothing Then
                Dim completedAnalyser = folder.Analyser
                _watcher.UpdateWatched(folder.FolderName, completedAnalyser, Not uncompress AndAlso succeeded)
            End If

            If succeeded AndAlso Not uncompress Then game.SetLastCompactGuiUpdate(DateTime.Now)
            If Not succeeded Then game.SetStatus("The operation did not complete successfully.")
        Catch ex As Exception
            operationException = ex
        Finally
            folder.Dispose()
            _activeFolder = Nothing
            If sleepPrevented Then Core.SharedMethods.RestoreSleep()
            game.SetWorking(False)
        End Try

        If backgroundingDisabled Then Await _watcher.EnableBackgrounding()
        If operationException IsNot Nothing Then Throw operationException
    End Function

    Private Shared Function GetInstalledSteamGames(steamFolder As DirectoryInfo) As List(Of SteamACFResult)
        If steamFolder Is Nothing Then Return New List(Of SteamACFResult)

        Dim games As New List(Of SteamACFResult)
        For Each library In GetSteamLibraries(steamFolder)
            For Each entry In SteamACFParser.LookupAllSteamGames(New DirectoryInfo(library.Path)).Values
                If Not entry.HasValue Then Continue For

                Dim game = entry.Value
                game.InstallDirectory = Path.Combine(library.Path, "common", game.InstallDirectory)
                games.Add(game)
            Next
        Next

        Return games.GroupBy(Function(game) game.InstallDirectory, StringComparer.OrdinalIgnoreCase).Select(Function(group) group.First()).ToList()
    End Function

    Private Shared Function GetSteamFolderFromRegistry() As DirectoryInfo
        Using regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Valve\Steam")
            Dim steamPath = TryCast(regKey?.GetValue("SteamPath"), String)
            If String.IsNullOrWhiteSpace(steamPath) Then Return Nothing
            Return New DirectoryInfo(steamPath)
        End Using
    End Function

    Private Shared Function GetSteamLibraries(steamFolder As DirectoryInfo) As List(Of SteamLibraryACFEntry)
        Dim results As New List(Of SteamLibraryACFEntry)
        Dim knownPaths As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim primarySteamAppsPath = Path.Combine(steamFolder.FullName, "steamapps")

        AddSteamLibrary(results, knownPaths, primarySteamAppsPath)

        Dim libraryVdfPath = Path.Combine(primarySteamAppsPath, "libraryfolders.vdf")
        If Not File.Exists(libraryVdfPath) Then Return results

        Dim libraryVdf = VdfConvert.Deserialize(File.ReadAllText(libraryVdfPath))
        Dim libraries = libraryVdf.Value.ToJson().ToObject(Of Dictionary(Of String, RawSteamLibraryEntry))()

        For Each library In libraries.Values
            If library IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(library.Path) Then AddSteamLibrary(results, knownPaths, Path.Combine(library.Path, "steamapps"))
        Next

        Return results
    End Function

    Private Shared Sub AddSteamLibrary(results As List(Of SteamLibraryACFEntry), knownPaths As HashSet(Of String), steamAppsPath As String)
        If Not Directory.Exists(steamAppsPath) OrElse Not knownPaths.Add(steamAppsPath) Then Return
        results.Add(New SteamLibraryACFEntry With {.Path = steamAppsPath})
    End Sub

    Private Class RawSteamLibraryEntry
        Public Property Path As String
    End Class

End Class

Public Class SteamLibraryACFEntry
    Public Property Path As String
End Class

Public Enum SteamGameStatus
    Compressed
    Uncompressed
    RecentlyUpdated
    PendingSteamUpdate
End Enum

Public Class SteamDetailedResult : Inherits ObservableObject

    Private Const MinimumUsefulSaving As Double = 0.05
    Private Const MaximumIncrementalSaving As Double = 0.02

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(DisplayedSavings))>
    Private _uncompressedBytes As Long

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(DisplayedSavings))>
    Private _currentFolderSize As Long

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(IsDisplayingActualSavings), NameOf(DisplayedSavings), NameOf(HasSavingsData), NameOf(CanCompress), NameOf(CanUncompress))>
    Private _isCompressed As Boolean

    <ObservableProperty>
    Private _compressionLevel As Core.WOFCompressionAlgorithm = Core.WOFCompressionAlgorithm.NO_COMPRESSION

    Private ReadOnly _lastSteamUpdate As DateTime
    Private _lastCompactGuiUpdate As DateTime?
    Private ReadOnly _hasPendingSteamUpdate As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(StatusMessage), NameOf(IsDisplayingActualSavings), NameOf(DisplayedSavings), NameOf(HasSavingsData), NameOf(CanCompress))>
    Private _status As SteamGameStatus

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(CanCompress))>
    Private _recommendedCompressionMode As Core.CompressionMode?

    <ObservableProperty>
    Private _recommendedAction As String

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(DisplayedSavings))>
    Private _expectedCompressionSavings As Long

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HasSavingsData))>
    Private _hasCompressionEstimate As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(CanCompress), NameOf(CanUncompress))>
    Private _isWorking As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HasOperationMessage))>
    Private _operationMessage As String

    <ObservableProperty>
    Private _headerImage As BitmapImage

    Public ReadOnly Property GameName As String
    Public ReadOnly Property GamePath As String
    Public ReadOnly Property AppID As Integer
    Public ReadOnly Property WikiCompressionResults As WikiCompressionResults
    Public ReadOnly Property WikiPoorlyCompressedFiles As List(Of String)

    Public ReadOnly Property DisplayPath As String
        Get
            Return New DirectoryInfo(GamePath).Parent?.Parent?.Parent?.FullName
        End Get
    End Property

    Public ReadOnly Property StatusMessage As String
        Get
            Select Case Status
                Case SteamGameStatus.Compressed
                    Return "Compressed"
                Case SteamGameStatus.Uncompressed
                    Return "Uncompressed"
                Case SteamGameStatus.RecentlyUpdated
                    Return "Recently Updated"
                Case SteamGameStatus.PendingSteamUpdate
                    Return "Update Available"
                Case Else
                    Return "Unknown"
            End Select
        End Get
    End Property

    Public ReadOnly Property DisplayedSavings As Long
        Get
            If IsDisplayingActualSavings Then Return Math.Max(0, UncompressedBytes - CurrentFolderSize)
            Return ExpectedCompressionSavings
        End Get
    End Property

    Public ReadOnly Property IsDisplayingActualSavings As Boolean
        Get
            Return IsCompressed AndAlso Status <> SteamGameStatus.RecentlyUpdated
        End Get
    End Property

    Public ReadOnly Property HasSavingsData As Boolean
        Get
            Return IsDisplayingActualSavings OrElse HasCompressionEstimate
        End Get
    End Property

    Public ReadOnly Property HasOperationMessage As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(OperationMessage)
        End Get
    End Property

    Public ReadOnly Property CanCompress As Boolean
        Get
            Return Not IsWorking AndAlso Status <> SteamGameStatus.PendingSteamUpdate AndAlso RecommendedCompressionMode.HasValue AndAlso (Not IsCompressed OrElse Status = SteamGameStatus.RecentlyUpdated)
        End Get
    End Property

    Public ReadOnly Property CanUncompress As Boolean
        Get
            Return Not IsWorking AndAlso IsCompressed
        End Get
    End Property

    Public Sub New(gameName As String, gamePath As String, appId As Integer, lastSteamUpdate As DateTime, hasPendingSteamUpdate As Boolean, lastCompactGuiUpdate As DateTime?, wikiResults As WikiCompressionResults, poorlyCompressedFiles As List(Of String))
        Me.GameName = gameName
        Me.GamePath = gamePath
        Me.AppID = appId
        _lastSteamUpdate = lastSteamUpdate
        _hasPendingSteamUpdate = hasPendingSteamUpdate
        _lastCompactGuiUpdate = lastCompactGuiUpdate
        Me.WikiCompressionResults = wikiResults
        Me.WikiPoorlyCompressedFiles = poorlyCompressedFiles
    End Sub

    Public Sub UpdateAnalysis(uncompressedBytes As Long, currentFolderSize As Long, compressionLevel As Core.WOFCompressionAlgorithm)
        Me.UncompressedBytes = uncompressedBytes
        Me.CurrentFolderSize = currentFolderSize
        Me.CompressionLevel = compressionLevel
        Me.IsCompressed = compressionLevel <> Core.WOFCompressionAlgorithm.NO_COMPRESSION
        SetRecommendation()
        SetGameStatus()
        OperationMessage = Nothing
    End Sub

    Public Sub SetWorking(value As Boolean, Optional status As String = Nothing)
        IsWorking = value
        If status IsNot Nothing Then SetStatus(status)
    End Sub

    Public Sub SetStatus(status As String)
        OperationMessage = status
    End Sub

    Public Sub SetLastCompactGuiUpdate(updated As DateTime)
        _lastCompactGuiUpdate = updated
        SetGameStatus()
    End Sub

    Private Sub SetGameStatus()
        If _hasPendingSteamUpdate Then
            Status = SteamGameStatus.PendingSteamUpdate
        ElseIf _lastCompactGuiUpdate.HasValue AndAlso _lastSteamUpdate > _lastCompactGuiUpdate.Value Then
            Status = SteamGameStatus.RecentlyUpdated
        ElseIf IsCompressed Then
            Status = SteamGameStatus.Compressed
        Else
            Status = SteamGameStatus.Uncompressed
        End If
    End Sub

    Private Sub SetRecommendation()
        Dim candidates As New List(Of (Mode As Core.CompressionMode, Result As CompressionResult)) From {
            (Core.CompressionMode.XPRESS4K, WikiCompressionResults?.XPress4K),
            (Core.CompressionMode.XPRESS8K, WikiCompressionResults?.XPress8K),
            (Core.CompressionMode.XPRESS16K, WikiCompressionResults?.XPress16K),
            (Core.CompressionMode.LZX, WikiCompressionResults?.LZX)
        }

        Dim validResults = candidates.Where(Function(candidate) candidate.Result IsNot Nothing AndAlso candidate.Result.TotalResults > 0 AndAlso candidate.Result.BeforeBytes > 0 AndAlso candidate.Result.AfterBytes > 0).Select(Function(candidate) (candidate.Mode, Savings:=Math.Max(0, 1 - (CDbl(candidate.Result.AfterBytes) / candidate.Result.BeforeBytes)))).ToList()

        If validResults.Count = 0 Then
            RecommendedCompressionMode = Nothing
            RecommendedAction = "No wiki data"
            ExpectedCompressionSavings = 0
            HasCompressionEstimate = False
            Return
        End If

        HasCompressionEstimate = True
        Dim bestSaving = validResults.Max(Function(result) result.Savings)

        If bestSaving < MinimumUsefulSaving Then
            RecommendedCompressionMode = Nothing
            RecommendedAction = "Do not compress"
            ExpectedCompressionSavings = 0
            Return
        End If

        Dim recommendation = validResults.OrderBy(Function(result) CInt(result.Mode)).First(Function(result) bestSaving - result.Savings <= MaximumIncrementalSaving)
        SelectCompressionMode(recommendation.Mode)
    End Sub

    <RelayCommand>
    Private Sub SelectCompressionMode(mode As Core.CompressionMode)
        RecommendedCompressionMode = mode
        RecommendedAction = $"Compress | {GetCompressionModeName(mode)}"

        Dim result = GetCompressionResult(mode)
        HasCompressionEstimate = result IsNot Nothing AndAlso result.TotalResults > 0 AndAlso result.BeforeBytes > 0 AndAlso result.AfterBytes > 0
        ExpectedCompressionSavings = If(HasCompressionEstimate, CLng(Math.Round(UncompressedBytes * Math.Max(0, 1 - (CDbl(result.AfterBytes) / result.BeforeBytes)))), 0)
    End Sub

    Private Function GetCompressionResult(mode As Core.CompressionMode) As CompressionResult
        Select Case mode
            Case Core.CompressionMode.XPRESS4K
                Return WikiCompressionResults?.XPress4K
            Case Core.CompressionMode.XPRESS8K
                Return WikiCompressionResults?.XPress8K
            Case Core.CompressionMode.XPRESS16K
                Return WikiCompressionResults?.XPress16K
            Case Core.CompressionMode.LZX
                Return WikiCompressionResults?.LZX
            Case Else
                Return Nothing
        End Select
    End Function

    Private Shared Function GetCompressionModeName(mode As Core.CompressionMode) As String
        Select Case mode
            Case Core.CompressionMode.XPRESS4K
                Return "X4K"
            Case Core.CompressionMode.XPRESS8K
                Return "X8K"
            Case Core.CompressionMode.XPRESS16K
                Return "X16K"
            Case Core.CompressionMode.LZX
                Return "LZX"
            Case Else
                Return mode.ToString()
        End Select
    End Function

End Class
