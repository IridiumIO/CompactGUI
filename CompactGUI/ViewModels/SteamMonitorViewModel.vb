Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.IO
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Windows.Threading
Imports System.Windows.Input

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
    Private ReadOnly _trackedSteamGames As New HashSet(Of SteamDetailedResult)
    Private ReadOnly _selectedGames As New List(Of SteamDetailedResult)
    Private Shared ReadOnly SteamImageClient As New HttpClient()
    Private _activeFolder As StandardFolder
    Private _activeGame As SteamDetailedResult
    Private _cancelRequested As Boolean
    Private _hasLoaded As Boolean
    Private _libraryFilter As String
    Private _statusFilter As SteamGameStatus?
    Private _recommendedActionFilter As SteamRecommendedAction?

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HasNoGames))>
    <NotifyCanExecuteChangedFor(NameOf(RefreshAllCommand))>
    Private _isLoading As Boolean

    <ObservableProperty>
    <NotifyCanExecuteChangedFor(NameOf(RefreshAllCommand))>
    Private _isAnalysingGames As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HasError))>
    Private _errorMessage As String

    <ObservableProperty>
    Private _searchText As String

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HasSelectedGame))>
    Private _selectedGame As SteamDetailedResult

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HasSelectedGames))>
    Private _selectedGameCount As Integer

    <ObservableProperty>
    <NotifyCanExecuteChangedFor(NameOf(AddSelectedToQueueCommand))>
    Private _useRecommendedCompressionLevel As Boolean = True

    <ObservableProperty>
    Private _queueCompressionMode As Core.CompressionMode = Core.CompressionMode.XPRESS4K

    <ObservableProperty>
    Private _useGlobalSkiplist As Boolean

    <ObservableProperty>
    Private _useSmartSkiplist As Boolean

    Public ReadOnly Property HasSelectedGame As Boolean
        Get
            Return SelectedGame IsNot Nothing
        End Get
    End Property

    Public ReadOnly Property HasSelectedGames As Boolean
        Get
            Return SelectedGameCount > 0
        End Get
    End Property

    Public ReadOnly Property HasActiveFilters As Boolean
        Get
            Return _libraryFilter IsNot Nothing OrElse _statusFilter.HasValue OrElse _recommendedActionFilter.HasValue
        End Get
    End Property

    Public ReadOnly Property SteamGamesData As New ObservableCollection(Of SteamDetailedResult)
    Public ReadOnly Property LibraryLocations As New ObservableCollection(Of SteamLibraryFilterOption)
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

    Public ReadOnly Property TotalSaved As Long
        Get
            Return SteamGamesData.Where(Function(game) game.IsCompressed).Sum(Function(game) Math.Max(0L, game.UncompressedBytes - game.CurrentFolderSize))
        End Get
    End Property

    Public ReadOnly Property TotalCanBeSaved As Long
        Get
            Return SteamGamesData.Where(Function(game) Not game.IsCompressed AndAlso game.RecommendedCompressionMode.HasValue AndAlso game.HasCompressionEstimate).Sum(Function(game) Math.Max(0L, game.ExpectedCompressionSavings))
        End Get
    End Property

    Public Sub New(wikiService As IWikiService, watcher As Watcher.Watcher, compressableFolderService As CompressableFolderService, analyserLogger As ILogger(Of Core.Analyser), navigationService As INavigationService, settingsService As ISettingsService)
        _wikiService = wikiService
        _watcher = watcher
        _compressableFolderService = compressableFolderService
        _analyserLogger = analyserLogger
        _navigationService = navigationService
        _settingsService = settingsService
        QueueCompressionMode = settingsService.AppSettings.SelectedCompressionMode
        UseGlobalSkiplist = settingsService.AppSettings.SkipNonCompressable
        UseSmartSkiplist = settingsService.AppSettings.SkipUserNonCompressable
        FilteredSteamGames = CollectionViewSource.GetDefaultView(SteamGamesData)
        FilteredSteamGames.Filter = AddressOf FilterGames
        AddHandler SteamGamesData.CollectionChanged, AddressOf OnSteamGamesCollectionChanged
    End Sub

    Private Sub OnSteamGamesCollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
        If e.Action = NotifyCollectionChangedAction.Reset Then
            For Each game In _trackedSteamGames
                RemoveHandler game.PropertyChanged, AddressOf OnSteamGamePropertyChanged
            Next
            _trackedSteamGames.Clear()
            LibraryLocations.Clear()
        Else
            If e.OldItems IsNot Nothing Then
                For Each game As SteamDetailedResult In e.OldItems
                    RemoveHandler game.PropertyChanged, AddressOf OnSteamGamePropertyChanged
                    _trackedSteamGames.Remove(game)
                Next
            End If

            If e.NewItems IsNot Nothing Then
                For Each game As SteamDetailedResult In e.NewItems
                    If _trackedSteamGames.Add(game) Then AddHandler game.PropertyChanged, AddressOf OnSteamGamePropertyChanged
                    If Not LibraryLocations.Any(Function(location) String.Equals(location.DisplayPath, game.DisplayPath, StringComparison.OrdinalIgnoreCase)) Then LibraryLocations.Add(New SteamLibraryFilterOption(game.DisplayPath, FilterLibraryCommand))
                Next
            End If
        End If

        If Not IsLoading Then
            OnPropertyChanged(NameOf(HasNoGames))
            NotifySavingsTotalsChanged()
        End If
    End Sub

    Private Sub OnSteamGamePropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        If IsAnalysingGames Then Return

        Select Case e.PropertyName
            Case Nothing, String.Empty, NameOf(SteamDetailedResult.UncompressedBytes), NameOf(SteamDetailedResult.CurrentFolderSize), NameOf(SteamDetailedResult.IsCompressed), NameOf(SteamDetailedResult.RecommendedCompressionMode), NameOf(SteamDetailedResult.ExpectedCompressionSavings), NameOf(SteamDetailedResult.HasCompressionEstimate)
                NotifySavingsTotalsChanged()
        End Select

        If _statusFilter.HasValue OrElse _recommendedActionFilter.HasValue Then FilteredSteamGames.Refresh()
    End Sub

    Private Sub NotifySavingsTotalsChanged()
        OnPropertyChanged(NameOf(TotalSaved))
        OnPropertyChanged(NameOf(TotalCanBeSaved))
    End Sub

    Private Sub OnSearchTextChanged(value As String)
        FilteredSteamGames.Refresh()
    End Sub

    <RelayCommand>
    Private Async Function RefreshAll() As Task
        SelectedGame = Nothing
        _selectedGames.Clear()
        SelectedGameCount = 0
        SteamGamesData.Clear()
        _hasLoaded = False
        Await LoadGamesAsync()
    End Function

    Private Function CanRefreshAll() As Boolean
        Return Not IsLoading AndAlso Not IsAnalysingGames AndAlso _activeGame Is Nothing
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
        _compressableFolderService.CancelEstimation(_activeFolder)
        If _activeFolder.FolderActionState = ActionState.Working AndAlso _activeFolder.Compressor IsNot Nothing Then
            Dim activeOperations = _activeFolder.Compressor.Cancel()
            game.SetStatus("Finishing {0} active file operations; do not power off.".LTF(activeOperations))
        End If
    End Sub

    'TODO: DIsable string.equals linting so rsharper stops being mad
    Private Function FilterGames(value As Object) As Boolean
        Dim game = TryCast(value, SteamDetailedResult)
        If game Is Nothing Then Return False

        If Not String.IsNullOrWhiteSpace(SearchText) Then
            Dim search = SearchText.Trim()
            Dim normalizedSearch = NormalizeSearchText(search)
            If game.GameName.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0 AndAlso Not NormalizeSearchText(game.GameName).Contains(normalizedSearch) AndAlso Not game.AppID.ToString().Contains(search) Then Return False
        End If

        If _libraryFilter IsNot Nothing AndAlso Not String.Equals(game.DisplayPath, _libraryFilter, StringComparison.OrdinalIgnoreCase) Then Return False
        If _statusFilter.HasValue AndAlso game.Status <> _statusFilter.Value Then Return False
        If _recommendedActionFilter.HasValue AndAlso game.RecommendedActionCategory <> _recommendedActionFilter.Value Then Return False
        Return True
    End Function

    Friend Sub UpdateSelectedGames(selectedItems As IList)
        _selectedGames.Clear()
        _selectedGames.AddRange(selectedItems.Cast(Of SteamDetailedResult))
        SelectedGameCount = _selectedGames.Count
        AddSelectedToQueueCommand.NotifyCanExecuteChanged()
    End Sub

    <RelayCommand>
    Private Sub AddSelectedToQueue()
        Dim requests = _selectedGames.Select(Function(game)
                                                 Dim mode = If(UseRecommendedCompressionLevel, game.RecommendedCompressionMode.Value, QueueCompressionMode)
                                                 Dim options As New CompressionOptions With {
                                                     .SelectedCompressionMode = mode,
                                                     .SkipPoorlyCompressedFileTypes = UseGlobalSkiplist,
                                                     .SkipUserSubmittedFiletypes = UseSmartSkiplist,
                                                     .SkipListEnabled = UseGlobalSkiplist OrElse UseSmartSkiplist,
                                                     .WatchFolderForChanges = True
                                                 }
                                                 Return New SteamQueueItem(game.GamePath, options)
                                             End Function).ToList()
        WeakReferenceMessenger.Default.Send(New SteamGamesAddedToQueueMessage(requests))
    End Sub

    Private Function CanAddSelectedToQueue() As Boolean
        Return _selectedGames.Count > 0 AndAlso (Not UseRecommendedCompressionLevel OrElse _selectedGames.All(Function(game) game.RecommendedCompressionMode.HasValue))
    End Function

    <RelayCommand>
    Private Sub FilterLibrary(path As String)
        _libraryFilter = If(String.Equals(_libraryFilter, path, StringComparison.OrdinalIgnoreCase), Nothing, path)
        RefreshFilters()
    End Sub

    <RelayCommand>
    Private Sub FilterStatus(status As SteamGameStatus)
        _statusFilter = If(_statusFilter = status, CType(Nothing, SteamGameStatus?), status)
        RefreshFilters()
    End Sub

    <RelayCommand>
    Private Sub FilterRecommendedAction(action As SteamRecommendedAction)
        _recommendedActionFilter = If(_recommendedActionFilter = action, CType(Nothing, SteamRecommendedAction?), action)
        RefreshFilters()
    End Sub

    <RelayCommand>
    Private Sub ClearFilters()
        _libraryFilter = Nothing
        _statusFilter = Nothing
        _recommendedActionFilter = Nothing
        RefreshFilters()
    End Sub

    Private Sub RefreshFilters()
        OnPropertyChanged(NameOf(HasActiveFilters))
        FilteredSteamGames.Refresh()
    End Sub

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
        Dim gamesToAnalyse As New List(Of SteamDetailedResult)
        Dim displayedGames As New List(Of SteamDetailedResult)
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
            Dim watchedByPath = _watcher.WatchedFolders.GroupBy(Function(folder) folder.Folder, StringComparer.OrdinalIgnoreCase).ToDictionary(Function(group) group.Key, Function(group) group.First(), StringComparer.OrdinalIgnoreCase)

            For Each game In games.OrderBy(Function(item) item.GameName)
                If Not Directory.Exists(game.InstallDirectory) Then Continue For

                Dim databaseResult As DatabaseCompressionResult = Nothing
                databaseByAppId.TryGetValue(game.AppID, databaseResult)

                Dim watchedFolder As Watcher.WatchedFolder = Nothing
                watchedByPath.TryGetValue(game.InstallDirectory, watchedFolder)
                Dim detailedResult = CreateDetailedResult(game, databaseResult, watchedFolder)
                Dim requiresAnalysis = Not TryApplyWatchedAnalysis(detailedResult, game, watchedFolder)

                If requiresAnalysis Then gamesToAnalyse.Add(detailedResult)
                If requiresAnalysis OrElse detailedResult.CurrentFolderSize > 0 Then
                    displayedGames.Add(detailedResult)
                    SteamGamesData.Add(detailedResult)
                End If
            Next

            _hasLoaded = True
            IsAnalysingGames = gamesToAnalyse.Count > 0
            IsLoading = False
            OnPropertyChanged(NameOf(HasNoGames))
            NotifySavingsTotalsChanged()

            Await Dispatcher.Yield(DispatcherPriority.Background)

            For Each game In displayedGames
                imageLoadTasks.Add(LoadGameHeaderAsync(game, steamFolder))
            Next

            For Each game In gamesToAnalyse
                Await AnalyseGameAsync(game)
                If game.CurrentFolderSize = 0 Then SteamGamesData.Remove(game)
            Next
        Catch ex As Exception
            ErrorMessage = $"Steam games could not be loaded: {ex.Message}"
        Finally
            _hasLoaded = True
            IsLoading = False
            IsAnalysingGames = False
            OnPropertyChanged(NameOf(HasNoGames))
            NotifySavingsTotalsChanged()
            If _statusFilter.HasValue OrElse _recommendedActionFilter.HasValue Then FilteredSteamGames.Refresh()
        End Try

        If imageLoadTasks.Count > 0 Then Await Task.WhenAll(imageLoadTasks)
    End Function

    Private Shared Function TryApplyWatchedAnalysis(game As SteamDetailedResult, steamGame As SteamACFResult, watchedFolder As Watcher.WatchedFolder) As Boolean
        If watchedFolder Is Nothing OrElse watchedFolder.HasTargetChanged Then Return False
        If watchedFolder.LastCheckedDate <= DateTime.UnixEpoch OrElse steamGame.LastUpdated > watchedFolder.LastCheckedDate Then Return False
        If watchedFolder.LastUncompressedSize <= 0 OrElse watchedFolder.LastCheckedSize <= 0 Then Return False

        game.UpdateAnalysis(watchedFolder.LastUncompressedSize, watchedFolder.LastCheckedSize, watchedFolder.CompressionLevel, False)
        Return True
    End Function

    Private Async Function LoadGameHeaderAsync(game As SteamDetailedResult, steamFolder As DirectoryInfo) As Task
        If game.AppID = 0 Then Return

        Dim imageDirectory = Path.Combine(_settingsService.DataFolder.FullName, "SteamCache")
        Dim imagePath = Path.Combine(imageDirectory, $"{game.AppID}_header.jpg")

        Try
            Directory.CreateDirectory(imageDirectory)

            If File.Exists(imagePath) Then
                Try
                    game.HeaderImage = CreateFadedHeaderImage(LoadImageFromDisk(imagePath))
                    Return
                Catch ex As Exception
                    File.Delete(imagePath)
                End Try
            End If

            Await _imageDownloadGate.WaitAsync()
            Try
                If File.Exists(imagePath) Then
                    game.HeaderImage = CreateFadedHeaderImage(LoadImageFromDisk(imagePath))
                    Return
                End If

                Dim steamCachedHeader = FindSteamCachedHeader(steamFolder, game.AppID)
                If steamCachedHeader IsNot Nothing Then
                    Try
                        Dim cachedImageData = Await File.ReadAllBytesAsync(steamCachedHeader)
                        game.HeaderImage = CreateFadedHeaderImage(LoadImageFromMemoryStream(cachedImageData))
                        Return
                    Catch ex As Exception
                        Diagnostics.Debug.WriteLine($"Failed to use Steam's cached header for {game.AppID}: {ex.Message}")
                    End Try
                End If

                Dim imageData = Await TryDownloadImageAsync($"https://steamcdn-a.akamaihd.net/steam/apps/{game.AppID}/header.jpg")
                Dim headerImage As BitmapImage = Nothing
                If imageData IsNot Nothing Then
                    Try
                        headerImage = CreateFadedHeaderImage(LoadImageFromMemoryStream(imageData))
                    Catch ex As Exception
                        imageData = Nothing
                    End Try
                End If

                If imageData Is Nothing Then
                    Dim storeHeaderUrl = Await GetStoreHeaderUrlAsync(game.AppID)
                    imageData = Await TryDownloadImageAsync(storeHeaderUrl)
                    If imageData IsNot Nothing Then headerImage = CreateFadedHeaderImage(LoadImageFromMemoryStream(imageData))
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


    Private Shared Function CreateFadedHeaderImage(source As BitmapImage) As BitmapImage
        If source Is Nothing OrElse source.PixelWidth = 0 OrElse source.PixelHeight = 0 Then Return source

        Const fadeEndRatio As Double = 0.69
        Dim converted As New FormatConvertedBitmap(source, PixelFormats.Bgra32, Nothing, 0)
        Dim pixelWidth = converted.PixelWidth
        Dim pixelHeight = converted.PixelHeight
        Dim stride = pixelWidth * 4
        Dim pixels((stride * pixelHeight) - 1) As Byte
        converted.CopyPixels(pixels, stride, 0)

        Dim fadeEnd = Math.Max(1, CInt(Math.Ceiling(pixelWidth * fadeEndRatio)))
        For x = 0 To pixelWidth - 1
            Dim fade = If(x >= fadeEnd, 0.0, 1.0 - CDbl(x) / fadeEnd)
            For y = 0 To pixelHeight - 1
                Dim alphaIndex = (y * stride) + (x * 4) + 3
                pixels(alphaIndex) = CByte(Math.Round(pixels(alphaIndex) * fade))
            Next
        Next

        Dim fadedSource = BitmapSource.Create(pixelWidth, pixelHeight, source.DpiX, source.DpiY, PixelFormats.Bgra32, Nothing, pixels, stride)
        Dim encoder As New PngBitmapEncoder()
        encoder.Frames.Add(BitmapFrame.Create(fadedSource))

        Dim fadedImage As New BitmapImage()
        Using stream As New MemoryStream()
            encoder.Save(stream)
            stream.Position = 0
            fadedImage.BeginInit()
            fadedImage.CacheOption = BitmapCacheOption.OnLoad
            fadedImage.StreamSource = stream
            fadedImage.EndInit()
        End Using

        If fadedImage.CanFreeze Then fadedImage.Freeze()
        Return fadedImage
    End Function

    Private Shared Function FindSteamCachedHeader(steamFolder As DirectoryInfo, appId As Integer) As String
        If steamFolder Is Nothing Then Return Nothing

        Dim appCacheDirectory = Path.Combine(steamFolder.FullName, "appcache", "librarycache", appId.ToString())
        If Not Directory.Exists(appCacheDirectory) Then Return Nothing

        Try
            Dim searchFolders = {appCacheDirectory}.Concat(Directory.EnumerateDirectories(appCacheDirectory))

            Return searchFolders.
                    SelectMany(Function(folderPath) Directory.EnumerateFiles(folderPath, "*header*", SearchOption.TopDirectoryOnly)).
                    Where(Function(imageFile) Path.GetExtension(imageFile).Equals(".jpg", StringComparison.OrdinalIgnoreCase) OrElse
                                                Path.GetExtension(imageFile).Equals(".jpeg", StringComparison.OrdinalIgnoreCase) OrElse
                                                Path.GetExtension(imageFile).Equals(".png", StringComparison.OrdinalIgnoreCase)).
                    FirstOrDefault()
        Catch ex As Exception
            Return Nothing
        End Try
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

        Dim detailedResult = New SteamDetailedResult(game.GameName, game.InstallDirectory, game.AppID, game.LastUpdated, game.HasPendingUpdate, watchedFolder, wikiResults, poorlyCompressedFiles)
        If watchedFolder?.SkipList IsNot Nothing Then detailedResult.CompressionOptions.SkipList = New List(Of String)(watchedFolder.SkipList)
        Return detailedResult
    End Function

    Private Async Function AnalyseGameAsync(game As SteamDetailedResult) As Task
        Using analyser As New Core.Analyser(game.GamePath, _analyserLogger)
            Dim analysedFiles = Await analyser.GetAnalysedFilesAsync(CancellationToken.None)
            If analysedFiles Is Nothing Then Return
            Dim compressionLevel = If(analyser.ContainsCompressedFiles, analysedFiles.Max(Function(file) file.CompressionMode), Core.WOFCompressionAlgorithm.NO_COMPRESSION)
            game.UpdateAnalysis(analyser.UncompressedBytes, analyser.CompressedBytes, compressionLevel, analyser.IsDirectStorage)
            If game.SelectedCompressionOption IsNot Nothing Then
                game.HasInsufficientFreeSpace = Not Core.SharedMethods.HasSufficientFreeSpaceForCompression(game.GamePath,
                                                                                                               analysedFiles,
                                                                                                               Core.WOFHelper.WOFConvertCompressionLevel(game.SelectedCompressionOption.Mode),
                                                                                                               Array.Empty(Of String)())
            End If
            game.HasInsufficientFreeSpaceForUncompression = Not Core.SharedMethods.HasSufficientFreeSpaceForCompression(game.GamePath,
                                                                                                                             analysedFiles,
                                                                                                                             Core.WOFCompressionAlgorithm.NO_COMPRESSION,
                                                                                                                             Array.Empty(Of String)(),
                                                                                                                             reserveUncompressedSize:=True)
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

    <RelayCommand> 'TODO: Fix the issue with creating a new StandardFolder - this does not allow the extensions to show up in the skiplist. Need to retain a thin version of the analyser results for this. 
    Private Sub EditSkipList(game As SteamDetailedResult)
        If game Is Nothing OrElse game.IsWorking Then Return

        Using folder As New StandardFolder(game.GamePath)
            folder.WikiPoorlyCompressedFiles = game.WikiPoorlyCompressedFiles
            folder.CompressionOptions = game.CompressionOptions

            Dim editor As New Settings_skiplistflyout(folder) With {.Owner = Application.Current.MainWindow}
            editor.ShowDialog()
        End Using
    End Sub

    <RelayCommand>
    Private Sub ClearSelection()
        SelectedGame = Nothing
    End Sub

    Private Async Function RunGameOperationAsync(game As SteamDetailedResult, uncompress As Boolean) As Task

        Await _operationGate.WaitAsync()
        Try
            If (uncompress AndAlso Not game.CanUncompress) OrElse (Not uncompress AndAlso Not game.CanCompress) Then Return
            _cancelRequested = False
            game.SetWorking(True, If(uncompress, "Uncompressing...".LT(), "Compressing...".LT()))
            Await RunFolderOperationAsync(game, uncompress)
            If _cancelRequested Then game.SetStatus("Operation cancelled.".LT())
        Catch ex As Exception
            game.SetWorking(False, If(_cancelRequested OrElse TypeOf ex Is OperationCanceledException, "Operation cancelled.".LT(), "Operation failed: {0}".LTF(ex.Message)))
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
            folder.CompressionOptions = game.CompressionOptions.Clone()
            Dim analysisResult = Await _compressableFolderService.AnalyseFolderAsync(folder)
            If analysisResult = -1 Then Throw New UnauthorizedAccessException("CompactGUI does not have permission to modify this folder.")
            If analysisResult <> 0 OrElse _cancelRequested Then Throw New OperationCanceledException()

            Dim isCurrentlyCompressed = folder.AnalysisResults.Any(Function(file) file.CompressionMode <> Core.WOFCompressionAlgorithm.NO_COMPRESSION)
            Dim succeeded As Boolean

            If uncompress Then
                If Not isCurrentlyCompressed Then Throw New InvalidOperationException("This game is not currently compressed.")
                If folder.HasInsufficientFreeSpaceForUncompression Then Return
                succeeded = Await _compressableFolderService.UncompressFolder(folder)
            Else
                If game.SelectedCompressionOption Is Nothing Then Throw New InvalidOperationException("This game does not have a selected compression mode.")
                folder.CompressionOptions.SelectedCompressionMode = game.SelectedCompressionOption.Mode
                folder.HasInsufficientFreeSpace = Not _compressableFolderService.HasSufficientFreeSpace(folder)
                game.HasInsufficientFreeSpace = folder.HasInsufficientFreeSpace
                If folder.HasInsufficientFreeSpace Then Return
                succeeded = Await _compressableFolderService.CompressFolder(folder)
                Await _compressableFolderService.AnalyseFolderAsync(folder)
            End If

            Dim compressionLevel = If(folder.AnalysisResults.Any(Function(file) file.CompressionMode <> Core.WOFCompressionAlgorithm.NO_COMPRESSION), folder.AnalysisResults.Max(Function(file) file.CompressionMode), Core.WOFCompressionAlgorithm.NO_COMPRESSION)
            game.UpdateAnalysis(folder.UncompressedBytes, folder.CompressedBytes, compressionLevel, folder.IsDirectStorage)

            If folder.Analyser IsNot Nothing Then
                Dim completedAnalyser = folder.Analyser
                _watcher.UpdateWatched(folder.FolderName, completedAnalyser, Not uncompress AndAlso succeeded)
            End If

            If succeeded AndAlso Not uncompress Then game.SetLastCompactGuiUpdate(DateTime.Now)
            If Not succeeded Then
                game.SetStatus("The operation did not complete successfully.")
            End If
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
        If Not Directory.Exists(steamAppsPath) Then Return

        Dim normalizedSteamAppsPath = Core.SharedMethods.NormalizeLocalPath(steamAppsPath)
        If Not knownPaths.Add(normalizedSteamAppsPath) Then Return
        results.Add(New SteamLibraryACFEntry(normalizedSteamAppsPath))
    End Sub

    Private Class RawSteamLibraryEntry
        Public Property Path As String
    End Class

End Class

Public Class SteamLibraryACFEntry
    Public Property Path As String

    Public Sub New(steamAppsPath As String)
        'normalise drive letter to capitals so it doesn't look bad
        Path = If(String.IsNullOrWhiteSpace(steamAppsPath), String.Empty, Char.ToUpperInvariant(steamAppsPath(0)) & steamAppsPath.Substring(1))
    End Sub

End Class

Public Enum SteamGameStatus
    Compressed
    Uncompressed
    RecentlyUpdated
    PendingSteamUpdate
    Analysing
End Enum

Public Enum SteamRecommendedAction
    None
    Compress
    DoNotCompress
    UpdateInSteam
End Enum

Public Class SteamDetailedResult : Inherits ObservableObject

    Private Const MinimumUsefulSaving As Double = 0.05
    Private Const MaximumIncrementalSaving As Double = 0.02

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(DisplayedSavings), NameOf(DetailSavings), NameOf(SavingsPercentage))>
    Private _uncompressedBytes As Long

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(DisplayedSavings), NameOf(DetailSavings), NameOf(SavingsPercentage))>
    Private _currentFolderSize As Long

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(IsDisplayingActualSavings), NameOf(DisplayedSavings), NameOf(DetailSavings), NameOf(SavingsPercentage), NameOf(HasSavingsData), NameOf(CanCompress), NameOf(CanUncompress), NameOf(RecommendedActionCategory))>
    Private _isCompressed As Boolean

    <ObservableProperty>
    Private _compressionLevel As Core.WOFCompressionAlgorithm = Core.WOFCompressionAlgorithm.NO_COMPRESSION

    Private ReadOnly _lastSteamUpdate As DateTime
    Private _lastCompactGuiUpdate As DateTime?
    Private ReadOnly _hasPendingSteamUpdate As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(StatusMessage), NameOf(IsDisplayingActualSavings), NameOf(DisplayedSavings), NameOf(HasSavingsData), NameOf(CanCompress), NameOf(RecommendedActionCategory))>
    Private _status As SteamGameStatus

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(CanCompress))>
    Private _recommendedCompressionMode As Core.CompressionMode?

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(CanCompress))>
    Private _selectedCompressionOption As SteamCompressionOption

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(CanCompress), NameOf(RecommendedActionCategory))>
    Private _isCompressionRecommended As Boolean

    <ObservableProperty>
    Private _recommendedAction As String

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(DisplayedSavings), NameOf(DetailSavings), NameOf(SavingsPercentage))>
    Private _expectedCompressionSavings As Long

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HasSavingsData))>
    Private _hasCompressionEstimate As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(CanCompress), NameOf(CanUncompress))>
    Private _isWorking As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(CanCompress))>
    Private _hasInsufficientFreeSpace As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(CanUncompress))>
    Private _hasInsufficientFreeSpaceForUncompression As Boolean

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HasOperationMessage))>
    Private _operationMessage As String

    <ObservableProperty>
    Private _headerImage As BitmapImage

    <ObservableProperty>
    Private _isDirectStorage As Boolean

    Public ReadOnly Property GameName As String
    Public ReadOnly Property GamePath As String
    Public ReadOnly Property AppID As Integer
    Public ReadOnly Property WikiCompressionResults As WikiCompressionResults
    Public ReadOnly Property WikiPoorlyCompressedFiles As List(Of String)
    Public ReadOnly Property CompressionOptions As New CompressionOptions
    Public ReadOnly Property CompressionModeOptions As New ObservableCollection(Of SteamCompressionOption)
    Public ReadOnly Property WatchlistEntry As Watcher.WatchedFolder

    Public ReadOnly Property IsWatched As Boolean
        Get
            Return WatchlistEntry IsNot Nothing
        End Get
    End Property

    Public ReadOnly Property DisplayPath As String
        Get
            Return New DirectoryInfo(GamePath).Parent?.Parent?.Parent?.FullName
        End Get
    End Property

    Public ReadOnly Property StatusMessage As String
        Get
            Select Case Status
                Case SteamGameStatus.Compressed
                    Return "Compressed".LT()
                Case SteamGameStatus.Uncompressed
                    Return "Uncompressed".LT()
                Case SteamGameStatus.RecentlyUpdated
                    Return "Recently Updated".LT()
                Case SteamGameStatus.PendingSteamUpdate
                    Return "Update Available".LT()
                Case SteamGameStatus.Analysing
                    Return "Analysing...".LT()
                Case Else
                    Return "Unknown".LT()
            End Select
        End Get
    End Property

    Public ReadOnly Property DisplayedSavings As Long
        Get
            If IsDisplayingActualSavings Then Return Math.Max(0, UncompressedBytes - CurrentFolderSize)
            Return ExpectedCompressionSavings
        End Get
    End Property

    Public ReadOnly Property DetailSavings As Long
        Get
            If IsCompressed Then Return Math.Max(0, UncompressedBytes - CurrentFolderSize)
            Return ExpectedCompressionSavings
        End Get
    End Property

    Public ReadOnly Property SavingsPercentage As Double
        Get
            If UncompressedBytes <= 0 Then Return 0
            Return Math.Clamp(CDbl(DetailSavings) / UncompressedBytes * 100, 0, 100)
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
            Return Not IsWorking AndAlso Status <> SteamGameStatus.PendingSteamUpdate AndAlso IsCompressionRecommended AndAlso SelectedCompressionOption IsNot Nothing AndAlso (Not IsCompressed OrElse Status = SteamGameStatus.RecentlyUpdated)
        End Get
    End Property

    Public ReadOnly Property RecommendedActionCategory As SteamRecommendedAction
        Get
            If Status = SteamGameStatus.PendingSteamUpdate Then Return SteamRecommendedAction.UpdateInSteam
            If IsCompressed AndAlso Status <> SteamGameStatus.RecentlyUpdated Then Return SteamRecommendedAction.None
            If IsCompressionRecommended Then Return SteamRecommendedAction.Compress
            If HasCompressionEstimate Then Return SteamRecommendedAction.DoNotCompress
            Return SteamRecommendedAction.None
        End Get
    End Property

    Public ReadOnly Property CanUncompress As Boolean
        Get
            Return Not IsWorking AndAlso IsCompressed AndAlso Not HasInsufficientFreeSpaceForUncompression
        End Get
    End Property

    Public Sub New(gameName As String, gamePath As String, appId As Integer, lastSteamUpdate As DateTime, hasPendingSteamUpdate As Boolean, watchedFolder As Watcher.WatchedFolder, wikiResults As WikiCompressionResults, poorlyCompressedFiles As List(Of String))
        Me.GameName = gameName
        Me.GamePath = gamePath
        Me.AppID = appId
        _lastSteamUpdate = lastSteamUpdate
        _hasPendingSteamUpdate = hasPendingSteamUpdate
        WatchlistEntry = watchedFolder
        _lastCompactGuiUpdate = watchedFolder?.LastCompressedDate
        Me.WikiCompressionResults = wikiResults
        Me.WikiPoorlyCompressedFiles = poorlyCompressedFiles
        CompressionOptions.SkipUserSubmittedFiletypes = poorlyCompressedFiles.Count > 0
        Status = SteamGameStatus.Analysing
        RecommendedAction = "Analysing...".LT()
    End Sub

    Public Sub UpdateAnalysis(uncompressedBytes As Long, currentFolderSize As Long, compressionLevel As Core.WOFCompressionAlgorithm, isDirectStorage As Boolean)
        Me.UncompressedBytes = uncompressedBytes
        Me.CurrentFolderSize = currentFolderSize
        Me.CompressionLevel = compressionLevel
        Me.IsCompressed = compressionLevel <> Core.WOFCompressionAlgorithm.NO_COMPRESSION
        Me.IsDirectStorage = isDirectStorage
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
            IsCompressionRecommended = False
            SetCompressionOptions(Nothing)
            RecommendedAction = "No wiki data".LT()
            ExpectedCompressionSavings = 0
            HasCompressionEstimate = False
            Return
        End If

        HasCompressionEstimate = True
        Dim bestSaving = validResults.Max(Function(result) result.Savings)

        If bestSaving < MinimumUsefulSaving Then
            RecommendedCompressionMode = Nothing
            IsCompressionRecommended = False
            SetCompressionOptions(Nothing)
            RecommendedAction = "Do not compress".LT()
            ExpectedCompressionSavings = 0
            Return
        End If

        Dim recommendation = validResults.OrderBy(Function(result) CInt(result.Mode)).First(Function(result) bestSaving - result.Savings <= MaximumIncrementalSaving)
        RecommendedCompressionMode = recommendation.Mode
        IsCompressionRecommended = True
        SetCompressionOptions(recommendation.Mode)
    End Sub

    <RelayCommand>
    Private Sub SelectCompressionMode(mode As Core.CompressionMode)
        Dim selectedOption = CompressionModeOptions.FirstOrDefault(Function(item) item.Mode = mode)
        If selectedOption Is Nothing Then Return

        If ReferenceEquals(SelectedCompressionOption, selectedOption) Then
            ApplyCompressionMode(mode)
        Else
            SelectedCompressionOption = selectedOption
        End If
    End Sub

    Private Sub OnSelectedCompressionOptionChanged(value As SteamCompressionOption)
        If value IsNot Nothing Then ApplyCompressionMode(value.Mode)
    End Sub

    Private Sub SetCompressionOptions(recommendedMode As Core.CompressionMode?)
        SelectedCompressionOption = Nothing
        CompressionModeOptions.Clear()

        For Each mode In {Core.CompressionMode.XPRESS4K, Core.CompressionMode.XPRESS8K, Core.CompressionMode.XPRESS16K, Core.CompressionMode.LZX}
            Dim displayName = GetCompressionModeDisplayName(mode)
            If recommendedMode.HasValue AndAlso mode = recommendedMode.Value Then displayName &= " - " & "recommended".LT()
            CompressionModeOptions.Add(New SteamCompressionOption(mode, displayName))
        Next

        If recommendedMode.HasValue Then SelectedCompressionOption = CompressionModeOptions.First(Function(item) item.Mode = recommendedMode.Value)
    End Sub

    Private Sub ApplyCompressionMode(mode As Core.CompressionMode)
        RecommendedAction = "Compress | {0}".LTFC("Steam Library Compression Mode Button", GetCompressionModeName(mode))

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

    Private Shared Function GetCompressionModeDisplayName(mode As Core.CompressionMode) As String
        Select Case mode
            Case Core.CompressionMode.XPRESS4K
                Return "XPRESS 4K"
            Case Core.CompressionMode.XPRESS8K
                Return "XPRESS 8K"
            Case Core.CompressionMode.XPRESS16K
                Return "XPRESS 16K"
            Case Core.CompressionMode.LZX
                Return "LZX"
            Case Else
                Return mode.ToString()
        End Select
    End Function

End Class

Public Class SteamCompressionOption
    Public ReadOnly Property Mode As Core.CompressionMode
    Public ReadOnly Property DisplayName As String

    Public Sub New(mode As Core.CompressionMode, displayName As String)
        Me.Mode = mode
        Me.DisplayName = displayName
    End Sub
End Class

Public Class SteamLibraryFilterOption
    Public ReadOnly Property DisplayPath As String
    Public ReadOnly Property Command As ICommand

    Public Sub New(displayPath As String, command As ICommand)
        Me.DisplayPath = displayPath
        Me.Command = command
    End Sub
End Class
