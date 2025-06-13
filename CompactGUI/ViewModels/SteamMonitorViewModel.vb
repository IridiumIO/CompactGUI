Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Threading

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Imports Gameloop.Vdf
Imports Gameloop.Vdf.JsonConverter

Public Class SteamMonitorViewModel : Inherits ObservableObject

    Private ReadOnly _wikiService As IWikiService
    Private ReadOnly _watcher As Watcher.Watcher
    Private ReadOnly _operationGate As New SemaphoreSlim(1, 1)
    Private _hasLoaded As Boolean
    Private _isLoading As Boolean
    Private _errorMessage As String
    Private _searchText As String

    Public ReadOnly Property SteamGamesData As New ObservableCollection(Of SteamDetailedResult)
    Public ReadOnly Property FilteredSteamGames As ICollectionView

    Public Property SearchText As String
        Get
            Return _searchText
        End Get
        Set(value As String)
            If Not SetProperty(_searchText, value) Then Return
            FilteredSteamGames.Refresh()
        End Set
    End Property

    Public Property IsLoading As Boolean
        Get
            Return _isLoading
        End Get
        Private Set(value As Boolean)
            If _isLoading = value Then Return
            _isLoading = value
            OnPropertyChanged()
            OnPropertyChanged(NameOf(HasNoGames))
        End Set
    End Property

    Public Property ErrorMessage As String
        Get
            Return _errorMessage
        End Get
        Private Set(value As String)
            If _errorMessage = value Then Return
            _errorMessage = value
            OnPropertyChanged()
            OnPropertyChanged(NameOf(HasError))
        End Set
    End Property

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

    Public ReadOnly Property CompressCommand As IAsyncRelayCommand(Of SteamDetailedResult)
    Public ReadOnly Property UncompressCommand As IAsyncRelayCommand(Of SteamDetailedResult)
    Public ReadOnly Property SortCommand As IRelayCommand(Of Object)

    Public Sub New(wikiService As IWikiService, watcher As Watcher.Watcher)
        _wikiService = wikiService
        _watcher = watcher
        FilteredSteamGames = CollectionViewSource.GetDefaultView(SteamGamesData)
        FilteredSteamGames.Filter = AddressOf FilterGames
        CompressCommand = New AsyncRelayCommand(Of SteamDetailedResult)(AddressOf CompressGameAsync)
        UncompressCommand = New AsyncRelayCommand(Of SteamDetailedResult)(AddressOf UncompressGameAsync)
        SortCommand = New RelayCommand(Of Object)(AddressOf SortGames)
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

    Private Sub SortGames(parameter As Object)
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

        IsLoading = True
        ErrorMessage = Nothing

        Try
            Dim games = Await Task.Run(AddressOf GetInstalledSteamGames)
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
                If detailedResult.CurrentFolderSize > 0 Then SteamGamesData.Add(detailedResult)
            Next
        Catch ex As Exception
            ErrorMessage = $"Steam games could not be loaded: {ex.Message}"
        Finally
            _hasLoaded = True
            IsLoading = False
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

    Private Shared Async Function AnalyseGameAsync(game As SteamDetailedResult) As Task
        Dim analyser As New Core.Analyser(game.GamePath)

        Try
            Await analyser.AnalyseFolder(CancellationToken.None)
            game.UpdateAnalysis(analyser.UncompressedBytes, analyser.CompressedBytes, analyser.ContainsCompressedFiles)
        Finally
            analyser.FileCompressionDetailsList.Clear()
        End Try
    End Function

    Private Async Function CompressGameAsync(game As SteamDetailedResult) As Task
        If game Is Nothing OrElse Not game.CanCompress Then Return
        Await RunGameOperationAsync(game, False)
    End Function

    Private Async Function UncompressGameAsync(game As SteamDetailedResult) As Task
        If game Is Nothing OrElse Not game.CanUncompress Then Return
        Await RunGameOperationAsync(game, True)
    End Function

    Private Async Function RunGameOperationAsync(game As SteamDetailedResult, uncompress As Boolean) As Task

        Await _operationGate.WaitAsync()
        Try
            If (uncompress AndAlso Not game.CanUncompress) OrElse (Not uncompress AndAlso Not game.CanCompress) Then Return
            game.SetWorking(True, If(uncompress, "Uncompressing...", "Compressing..."))
            Await RunFolderOperationAsync(game, uncompress)
        Catch ex As Exception
            game.SetWorking(False, $"Operation failed: {ex.Message}")
        Finally
            _operationGate.Release()
        End Try
    End Function

    Private Async Function RunFolderOperationAsync(game As SteamDetailedResult, uncompress As Boolean) As Task
        Dim folder As New StandardFolder(game.GamePath)
        Dim backgroundingDisabled As Boolean
        Dim sleepPrevented As Boolean
        Dim operationException As Exception = Nothing

        Try
            Await _watcher.DisableBackgrounding()
            backgroundingDisabled = True
            Core.SharedMethods.PreventSleep()
            sleepPrevented = True

            folder.WikiPoorlyCompressedFiles = game.WikiPoorlyCompressedFiles
            Dim analysisResult = Await folder.AnalyseFolderAsync()
            If analysisResult = -1 Then Throw New UnauthorizedAccessException("CompactGUI does not have permission to modify this folder.")

            Dim isCurrentlyCompressed = folder.AnalysisResults.Any(Function(file) file.CompressionMode <> Core.WOFCompressionAlgorithm.NO_COMPRESSION)
            Dim succeeded As Boolean

            If uncompress Then
                If Not isCurrentlyCompressed Then Throw New InvalidOperationException("This game is not currently compressed.")
                succeeded = Await folder.UncompressFolder()
            Else
                If Not game.RecommendedCompressionMode.HasValue Then Throw New InvalidOperationException("This game does not have a compression recommendation.")
                folder.CompressionOptions.SelectedCompressionMode = game.RecommendedCompressionMode.Value
                succeeded = Await folder.CompressFolder()
                Await folder.AnalyseFolderAsync()
            End If

            game.UpdateAnalysis(folder.UncompressedBytes, folder.CompressedBytes, folder.AnalysisResults.Any(Function(file) file.CompressionMode <> Core.WOFCompressionAlgorithm.NO_COMPRESSION))

            If folder.Analyser IsNot Nothing Then
                Dim completedAnalyser = folder.Analyser
                _watcher.UpdateWatched(folder.FolderName, completedAnalyser, Not uncompress AndAlso succeeded)
            End If

            If succeeded AndAlso Not uncompress Then game.SetLastCompactGuiUpdate(DateTime.Now)
            If Not succeeded Then game.SetStatus("The operation did not complete successfully.")
        Catch ex As Exception
            operationException = ex
        Finally
            folder.AnalysisResults?.Clear()
            folder.Analyser?.FileCompressionDetailsList.Clear()
            folder.Analyser = Nothing
            folder.Compressor = Nothing
            If sleepPrevented Then Core.SharedMethods.RestoreSleep()
            game.SetWorking(False)
        End Try

        If backgroundingDisabled Then Await _watcher.EnableBackgrounding()
        If operationException IsNot Nothing Then Throw operationException
    End Function

    Private Shared Function GetInstalledSteamGames() As List(Of SteamACFResult)
        Dim steamFolder = GetSteamFolderFromRegistry()
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

    Private _uncompressedBytes As Long
    Private _currentFolderSize As Long
    Private _isCompressed As Boolean
    Private ReadOnly _lastSteamUpdate As DateTime
    Private _lastCompactGuiUpdate As DateTime?
    Private ReadOnly _hasPendingSteamUpdate As Boolean
    Private _status As SteamGameStatus
    Private _recommendedCompressionMode As Core.CompressionMode?
    Private _recommendedAction As String
    Private _expectedCompressionSavings As Long
    Private _hasCompressionEstimate As Boolean
    Private _isWorking As Boolean
    Private _operationMessage As String

    Public ReadOnly Property GameName As String
    Public ReadOnly Property GamePath As String
    Public ReadOnly Property AppID As Integer
    Public ReadOnly Property WikiCompressionResults As WikiCompressionResults
    Public ReadOnly Property WikiPoorlyCompressedFiles As List(Of String)
    Public ReadOnly Property SelectCompressionModeCommand As IRelayCommand(Of Core.CompressionMode)

    Public ReadOnly Property DisplayPath As String
        Get
            Return New DirectoryInfo(GamePath).Parent?.Parent?.Parent?.FullName
        End Get
    End Property

    Public ReadOnly Property CurrentFolderSize As Long
        Get
            Return _currentFolderSize
        End Get
    End Property

    Public ReadOnly Property IsCompressed As Boolean
        Get
            Return _isCompressed
        End Get
    End Property

    Public ReadOnly Property Status As SteamGameStatus
        Get
            Return _status
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
                    Return "Recently updated"
                Case SteamGameStatus.PendingSteamUpdate
                    Return "Pending update"
                Case Else
                    Return "Unknown"
            End Select
        End Get
    End Property

    Public ReadOnly Property RecommendedCompressionMode As Core.CompressionMode?
        Get
            Return _recommendedCompressionMode
        End Get
    End Property

    Public ReadOnly Property RecommendedAction As String
        Get
            Return _recommendedAction
        End Get
    End Property

    Public ReadOnly Property ExpectedCompressionSavings As Long
        Get
            Return _expectedCompressionSavings
        End Get
    End Property

    Public ReadOnly Property DisplayedSavings As Long
        Get
            If IsDisplayingActualSavings Then Return Math.Max(0, _uncompressedBytes - CurrentFolderSize)
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

    Public ReadOnly Property HasCompressionEstimate As Boolean
        Get
            Return _hasCompressionEstimate
        End Get
    End Property

    Public Property IsWorking As Boolean
        Get
            Return _isWorking
        End Get
        Private Set(value As Boolean)
            If _isWorking = value Then Return
            _isWorking = value
            OnPropertyChanged()
            OnPropertyChanged(NameOf(CanCompress))
            OnPropertyChanged(NameOf(CanUncompress))
        End Set
    End Property

    Public ReadOnly Property OperationMessage As String
        Get
            Return _operationMessage
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
        SelectCompressionModeCommand = New RelayCommand(Of Core.CompressionMode)(AddressOf SelectCompressionMode)
    End Sub

    Public Sub UpdateAnalysis(uncompressedBytes As Long, currentFolderSize As Long, isCompressed As Boolean)
        _uncompressedBytes = uncompressedBytes
        _currentFolderSize = currentFolderSize
        _isCompressed = isCompressed
        SetRecommendation()
        SetGameStatus()
        _operationMessage = Nothing

        OnPropertyChanged(NameOf(currentFolderSize))
        OnPropertyChanged(NameOf(isCompressed))
        OnPropertyChanged(NameOf(Status))
        OnPropertyChanged(NameOf(StatusMessage))
        OnPropertyChanged(NameOf(RecommendedCompressionMode))
        OnPropertyChanged(NameOf(RecommendedAction))
        OnPropertyChanged(NameOf(ExpectedCompressionSavings))
        OnPropertyChanged(NameOf(DisplayedSavings))
        OnPropertyChanged(NameOf(IsDisplayingActualSavings))
        OnPropertyChanged(NameOf(HasCompressionEstimate))
        OnPropertyChanged(NameOf(HasSavingsData))
        OnPropertyChanged(NameOf(OperationMessage))
        OnPropertyChanged(NameOf(HasOperationMessage))
        OnPropertyChanged(NameOf(CanCompress))
        OnPropertyChanged(NameOf(CanUncompress))
    End Sub

    Public Sub SetWorking(value As Boolean, Optional status As String = Nothing)
        IsWorking = value
        If status IsNot Nothing Then SetStatus(status)
    End Sub

    Public Sub SetStatus(status As String)
        _operationMessage = status
        OnPropertyChanged(NameOf(OperationMessage))
        OnPropertyChanged(NameOf(HasOperationMessage))
    End Sub

    Public Sub SetLastCompactGuiUpdate(updated As DateTime)
        _lastCompactGuiUpdate = updated
        SetGameStatus()
        OnPropertyChanged(NameOf(Status))
        OnPropertyChanged(NameOf(StatusMessage))
        OnPropertyChanged(NameOf(DisplayedSavings))
        OnPropertyChanged(NameOf(IsDisplayingActualSavings))
        OnPropertyChanged(NameOf(HasSavingsData))
        OnPropertyChanged(NameOf(CanCompress))
    End Sub

    Private Sub SetGameStatus()
        If _hasPendingSteamUpdate Then
            _status = SteamGameStatus.PendingSteamUpdate
        ElseIf _lastCompactGuiUpdate.HasValue AndAlso _lastSteamUpdate > _lastCompactGuiUpdate.Value Then
            _status = SteamGameStatus.RecentlyUpdated
        ElseIf IsCompressed Then
            _status = SteamGameStatus.Compressed
        Else
            _status = SteamGameStatus.Uncompressed
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
            _recommendedCompressionMode = Nothing
            _recommendedAction = "No wiki data"
            _expectedCompressionSavings = 0
            _hasCompressionEstimate = False
            Return
        End If

        _hasCompressionEstimate = True
        Dim bestSaving = validResults.Max(Function(result) result.Savings)

        If bestSaving < MinimumUsefulSaving Then
            _recommendedCompressionMode = Nothing
            _recommendedAction = "Do not compress"
            _expectedCompressionSavings = 0
            Return
        End If

        Dim recommendation = validResults.OrderBy(Function(result) CInt(result.Mode)).First(Function(result) bestSaving - result.Savings <= MaximumIncrementalSaving)
        SelectCompressionMode(recommendation.Mode)
    End Sub

    Private Sub SelectCompressionMode(mode As Core.CompressionMode)
        _recommendedCompressionMode = mode
        _recommendedAction = $"Compress with {GetCompressionModeName(mode)}"

        Dim result = GetCompressionResult(mode)
        _hasCompressionEstimate = result IsNot Nothing AndAlso result.TotalResults > 0 AndAlso result.BeforeBytes > 0 AndAlso result.AfterBytes > 0
        _expectedCompressionSavings = If(_hasCompressionEstimate, CLng(Math.Round(_uncompressedBytes * Math.Max(0, 1 - (CDbl(result.AfterBytes) / result.BeforeBytes)))), 0)

        OnPropertyChanged(NameOf(RecommendedCompressionMode))
        OnPropertyChanged(NameOf(RecommendedAction))
        OnPropertyChanged(NameOf(ExpectedCompressionSavings))
        OnPropertyChanged(NameOf(DisplayedSavings))
        OnPropertyChanged(NameOf(HasCompressionEstimate))
        OnPropertyChanged(NameOf(HasSavingsData))
        OnPropertyChanged(NameOf(CanCompress))
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
