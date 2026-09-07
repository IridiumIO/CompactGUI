Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging

Imports CompactGUI.Core.Settings

Imports CompactGUI.Core.SharedMethods
Imports CompactGUI.Logging

Imports Microsoft.Extensions.Logging

Partial Public NotInheritable Class HomeViewModel : Inherits ObservableRecipient : Implements IRecipient(Of WatcherAddedFolderToQueueMessage), IRecipient(Of SteamGamesAddedToQueueMessage)

    Private ReadOnly _folderViewModels As New Dictionary(Of CompressableFolder, FolderViewModel)

    <ObservableProperty>
    Private _Folders As ObservableCollection(Of CompressableFolder) = New ObservableCollection(Of CompressableFolder)

    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(SelectedFolderViewModel))>
    <NotifyPropertyChangedRecipients>
    Private _SelectedFolder As CompressableFolder

    Public ReadOnly Property SelectedFolderViewModel As FolderViewModel
        Get
            If SelectedFolder Is Nothing Then Return Nothing

            Dim value As FolderViewModel = Nothing
            Return If(_folderViewModels.TryGetValue(SelectedFolder, value), value, Nothing)

        End Get
    End Property

    Public ReadOnly Property HomeViewIsFresh As Boolean
        Get
            Return Not Folders.Any()
        End Get
    End Property

    Public ReadOnly Property AwaitingFolderCount As Integer
        Get
            Return Folders.Where(Function(folder) folder.FolderActionState = ActionState.Idle).Count()
        End Get
    End Property

    Public ReadOnly Property WorkingFolderCount As Integer
        Get
            Return Folders.Where(Function(folder) folder.FolderActionState = ActionState.Working OrElse folder.FolderActionState = ActionState.Paused).Count()
        End Get
    End Property

    Public ReadOnly Property IsQueueRunning As Boolean
        Get
            Return WorkingFolderCount > 0
        End Get
    End Property

    Public ReadOnly Property ActiveFolderViewModel As FolderViewModel
        Get
            Dim activeFolder = Folders.FirstOrDefault(Function(folder) folder.FolderActionState = ActionState.Working OrElse folder.FolderActionState = ActionState.Paused)
            If activeFolder Is Nothing Then Return Nothing

            Dim value As FolderViewModel = Nothing
            Return If(_folderViewModels.TryGetValue(activeFolder, value), value, Nothing)
        End Get
    End Property

    Public ReadOnly Property QueueStatusSummary As String
        Get
            Return $"{AwaitingFolderCount} awaiting · {WorkingFolderCount} working"
        End Get
    End Property

    Public ReadOnly Property TotalQueuedSize As Long
        Get
            Return Folders.Sum(Function(folder) folder.UncompressedBytes)
        End Get
    End Property

    Public ReadOnly Property HasAwaitingFolders As Boolean
        Get
            Return AwaitingFolderCount > 0
        End Get
    End Property

    Public ReadOnly Property HasCompressedFolders As Boolean
        Get
            Return Folders.Any(Function(folder) folder.FolderActionState = ActionState.Results)
        End Get
    End Property

    Public ReadOnly Property AwaitingEstimatedSavings As Long
        Get
            Return Folders.Where(Function(folder) folder.FolderActionState = ActionState.Idle).Sum(Function(folder) GetSelectedModeEstimatedSavings(folder))
        End Get
    End Property

    Public ReadOnly Property TotalSaved As Long
        Get
            Return Folders.Where(Function(folder) folder.FolderActionState = ActionState.Results).Sum(Function(folder) Math.Max(0, folder.BytesSaved))
        End Get
    End Property

    Public ReadOnly Property DisplayVersion As String
        Get
            Return Application.AppVersion.Friendly
        End Get
    End Property

    Public ReadOnly Property IsAdmin As Boolean
        Get
            Dim principal = New Security.Principal.WindowsPrincipal(Security.Principal.WindowsIdentity.GetCurrent())
            Return principal.IsInRole(Security.Principal.WindowsBuiltInRole.Administrator)
        End Get
    End Property



    Private ReadOnly _watcher As Watcher.Watcher
    Private ReadOnly _snackbarService As CustomSnackBarService
    Private ReadOnly _logger As ILogger(Of HomeViewModel)
    Private ReadOnly _settingsService As ISettingsService
    Private ReadOnly _compressableFolderService As CompressableFolderService

    Sub New(watcher As Watcher.Watcher, snackbarService As CustomSnackBarService, logger As ILogger(Of HomeViewModel), settingsService As ISettingsService, compressableFolderService As CompressableFolderService)
        WeakReferenceMessenger.Default.Register(Of WatcherAddedFolderToQueueMessage)(Me)
        WeakReferenceMessenger.Default.Register(Of SteamGamesAddedToQueueMessage)(Me)
        AddHandler Folders.CollectionChanged, AddressOf OnFoldersCollectionChanged
        _watcher = watcher
        _snackbarService = snackbarService
        _logger = logger
        _settingsService = settingsService
        _compressableFolderService = compressableFolderService
    End Sub


    'Private Sub OnSelectedFolderChanged(value As CompressableFolder)

    '    WeakReferenceMessenger.Default.Send(New BackgroundImageChangedMessage(value?.FolderBGImage))

    'End Sub




    Private Sub OnAnyFolderPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        If e.PropertyName = NameOf(CompressableFolder.FolderActionState) Then
            OnPropertyChanged(NameOf(HomeViewModelState))
            Application.Current.Dispatcher.Invoke(Sub() RemoveFolderCommand.NotifyCanExecuteChanged())
        End If

        If e.PropertyName = NameOf(CompressableFolder.FolderActionState) OrElse
           e.PropertyName = NameOf(CompressableFolder.UncompressedBytes) OrElse
           e.PropertyName = NameOf(CompressableFolder.CompressedBytes) OrElse
           e.PropertyName = NameOf(CompressableFolder.WikiCompressionResults) OrElse
           e.PropertyName = NameOf(CompressableFolder.CompressionOptions) Then
            NotifyQueueSummaryChanged()
        End If

        If e.PropertyName = NameOf(CompressableFolder.CompressionOptions) Then
            AddHandler CType(sender, CompressableFolder).CompressionOptions.PropertyChanged, AddressOf OnCompressionOptionsPropertyChanged
        End If
    End Sub

    Private Sub OnCompressionOptionsPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        If e.PropertyName = NameOf(CompressionOptions.SelectedCompressionMode) Then NotifyQueueSummaryChanged()
    End Sub

    Private Sub NotifyQueueSummaryChanged()
        OnPropertyChanged(NameOf(AwaitingFolderCount))
        OnPropertyChanged(NameOf(WorkingFolderCount))
        OnPropertyChanged(NameOf(IsQueueRunning))
        OnPropertyChanged(NameOf(ActiveFolderViewModel))
        OnPropertyChanged(NameOf(QueueStatusSummary))
        OnPropertyChanged(NameOf(TotalQueuedSize))
        OnPropertyChanged(NameOf(HasAwaitingFolders))
        OnPropertyChanged(NameOf(HasCompressedFolders))
        OnPropertyChanged(NameOf(AwaitingEstimatedSavings))
        OnPropertyChanged(NameOf(TotalSaved))
    End Sub

    Private Shared Function GetSelectedModeEstimatedSavings(folder As CompressableFolder) As Long
        If folder.WikiCompressionResults Is Nothing Then Return 0

        Dim result As CompressionResult = Nothing
        Select Case folder.CompressionOptions.SelectedCompressionMode
            Case Core.CompressionMode.XPRESS4K
                result = folder.WikiCompressionResults.XPress4K
            Case Core.CompressionMode.XPRESS8K
                result = folder.WikiCompressionResults.XPress8K
            Case Core.CompressionMode.XPRESS16K
                result = folder.WikiCompressionResults.XPress16K
            Case Core.CompressionMode.LZX
                result = folder.WikiCompressionResults.LZX
        End Select

        Return If(result Is Nothing, 0, Math.Max(0, result.BytesSaved))
    End Function

    Private Sub OnFoldersCollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
        OnPropertyChanged(NameOf(HomeViewModelState))
        If e.Action = NotifyCollectionChangedAction.Add Then
            For Each folder As CompressableFolder In e.NewItems
                AddHandler folder.PropertyChanged, AddressOf OnAnyFolderPropertyChanged
                AddHandler folder.CompressionOptions.PropertyChanged, AddressOf OnCompressionOptionsPropertyChanged
            Next
        ElseIf e.Action = NotifyCollectionChangedAction.Remove Then
            For Each folder As CompressableFolder In e.OldItems
                RemoveHandler folder.PropertyChanged, AddressOf OnAnyFolderPropertyChanged
                RemoveHandler folder.CompressionOptions.PropertyChanged, AddressOf OnCompressionOptionsPropertyChanged
            Next
        End If

        OnPropertyChanged(NameOf(HomeViewIsFresh))
        NotifyQueueSummaryChanged()
    End Sub



    Public Async Function AddFoldersAsync(folderPaths As IEnumerable(Of String), Optional queueOptions As IReadOnlyDictionary(Of String, CompressionOptions) = Nothing) As Task

        HomeViewModelLog.AddingFolders(_logger, folderPaths)

        Dim invalidFolders = GetInvalidFolders(folderPaths.ToArray)
        Dim validFolders = folderPaths.Except(invalidFolders.InvalidFolders)

        If invalidFolders.InvalidFolders.Count > 0 Then
            'TODO: Move this logger check to the snackbarService class?
            HomeViewModelLog.InvalidFolders(_logger, invalidFolders.InvalidFolders, invalidFolders.InvalidMessages.Select(Function(x) GetFolderVerificationMessage(x)))
            _snackbarService.ShowInvalidFoldersMessage(invalidFolders.InvalidFolders, invalidFolders.InvalidMessages)
        End If

        For Each folderName In validFolders

            Dim newFolder As CompressableFolder = Await CompressableFolderFactory.CreateCompressableFolder(folderName)

            newFolder.CompressionOptions.WatchFolderForChanges = _settingsService.AppSettings.WatchFolderForChanges
            newFolder.CompressionOptions.SelectedCompressionMode = _settingsService.AppSettings.SelectedCompressionMode
            newFolder.CompressionOptions.SkipPoorlyCompressedFileTypes = _settingsService.AppSettings.SkipNonCompressable
            newFolder.CompressionOptions.SkipUserSubmittedFiletypes = _settingsService.AppSettings.SkipUserNonCompressable

            Dim requestedOptions As CompressionOptions = Nothing
            Dim hasRequestedOptions = queueOptions?.TryGetValue(folderName, requestedOptions)
            If hasRequestedOptions Then newFolder.CompressionOptions = requestedOptions.Clone()

            If Not Folders.Any(Function(f) f.FolderName = newFolder.FolderName) Then
                Folders.Add(newFolder)
                Dim vm As New FolderViewModel(newFolder, _watcher, _snackbarService, _compressableFolderService)
                _folderViewModels.Add(newFolder, vm)
                SelectedFolder = newFolder
            End If

            Dim res = Await _compressableFolderService.AnalyseFolderAsync(newFolder)
            If TypeOf (newFolder) Is SteamFolder Then
                Await CType(newFolder, SteamFolder).GetWikiResults()
            Else
                If _settingsService.AppSettings.EstimateCompressionForNonSteamFolders Then
                    HomeViewModelLog.GettingEstimatedCompression(_logger, newFolder.FolderName, newFolder.UncompressedBytes)
                    Await _compressableFolderService.GetEstimatedCompression(newFolder)
                End If

            End If

            If _watcher.WatchedFolders.Any(Function(w) w.Folder = newFolder.FolderName) Then
                Dim watchedFolder = _watcher.WatchedFolders.First(Function(w) w.Folder = newFolder.FolderName)
                newFolder.CompressionOptions.WatchFolderForChanges = True
                If Not hasRequestedOptions Then
                    If watchedFolder.CompressionLevel <> Core.WOFCompressionAlgorithm.NO_COMPRESSION Then
                        newFolder.CompressionOptions.SelectedCompressionMode = Core.WOFHelper.CompressionModeFromWOFMode(watchedFolder.CompressionLevel)
                    End If
                    If watchedFolder.SkipList IsNot Nothing Then
                        newFolder.CompressionOptions.SkipList = New List(Of String)(watchedFolder.SkipList)
                    End If
                End If

            End If



        Next


    End Function




    <RelayCommand>
    Public Sub RemoveFolder(folder As CompressableFolder)
        If Not CanRemoveFolder() Then
            _snackbarService.ShowCannotRemoveFolder()
            Return
        End If

        If folder Is Nothing Then Return

        Dim index = Folders.IndexOf(folder)
        Dim wasSelected = ReferenceEquals(SelectedFolder, folder)

        _compressableFolderService.CancelEstimation(folder)

        Dim folderViewModel As FolderViewModel = Nothing
        If _folderViewModels.TryGetValue(folder, folderViewModel) Then
            folderViewModel.Dispose()
            _folderViewModels.Remove(folder)
        End If

        Folders.Remove(folder)

        If wasSelected Then
            If Folders.Count = 0 Then
                SelectedFolder = Nothing
            Else
                SelectedFolder = If(index < Folders.Count, Folders(index), Folders.Last())
            End If
        End If

        folder.Dispose()
    End Sub

    Public Function CanRemoveFolder() As Boolean
        Return HomeViewModelState = ActionState.Results OrElse HomeViewModelState = ActionState.Idle
    End Function


    Public Sub NotifyPropertyChanged(propertyName As String)
        OnPropertyChanged(propertyName)
    End Sub

    Public ReadOnly Property HomeViewModelState As ActionState
        Get

            Dim retState As ActionState

            If Compressing OrElse Folders.Any(Function(f) f.FolderActionState = ActionState.Working OrElse f.FolderActionState = ActionState.Paused) Then
                retState = ActionState.Working
            ElseIf Folders.Any(Function(f) f.FolderActionState = ActionState.Analysing) Then
                retState = ActionState.Analysing
            ElseIf Folders.All(Function(f) f.FolderActionState = ActionState.Results) Then
                retState = ActionState.Results
            Else
                retState = ActionState.Idle
            End If

            Return retState

        End Get

    End Property


    <ObservableProperty>
    <NotifyPropertyChangedFor(NameOf(HomeViewModelState))>
    Private _Compressing As Boolean = False




    <RelayCommand>
    Private Async Function CompressAll() As Task

        Await _watcher.DisableBackgrounding()

        Compressing = True
        Core.SharedMethods.PreventSleep()
        Dim tasks As New List(Of Task)()
        Dim foldersToCompress = Folders.Where(Function(f) f.FolderActionState = ActionState.Idle).ToList
        HomeViewModelLog.StartingBatchCompression(_logger, foldersToCompress.Count)
        For Each folder In foldersToCompress
            If folder.FolderActionState = ActionState.Idle Then
                Await Task.Run(Async Function()
                                   HomeViewModelLog.CompressingFolder(_logger, folder.FolderName)
                                   Dim ret = Await _compressableFolderService.CompressFolder(folder)
                                   Dim analysis = Await _compressableFolderService.AnalyseFolderAsync(folder)

                                   If _settingsService.AppSettings.ShowNotifications Then

                                       Application.GetService(Of TrayNotifierService).Notify_Compressed(folder.DisplayName, folder.UncompressedBytes - folder.CompressedBytes, folder.CompressionRatio)

                                   End If

                                   _watcher.UpdateWatched(folder.FolderName, folder.Analyser, True)

                                   'For Each poorext In folder.PoorlyCompressedFiles
                                   '    Debug.WriteLine($"{poorext.extension} : {poorext.totalFiles} with ratio of {poorext.cRatio}")
                                   'Next

                                   Return True
                               End Function)
            End If
        Next
        Compressing = False

        For Each folder In Folders.Where(Function(f) f.CompressionOptions.WatchFolderForChanges)
            AddOrUpdateFolderWatcher(folder)
        Next

        RemoveFolderCommand.NotifyCanExecuteChanged()
        Core.SharedMethods.RestoreSleep()
        Await _watcher.EnableBackgrounding()
    End Function


    Private Function CanCompressAll() As Boolean
        Return HomeViewModelState <> ActionState.Working AndAlso Not Folders.Any(Function(f) f.FolderActionState = ActionState.Analysing)
    End Function


    Public Sub AddOrUpdateFolderWatcher(folder As CompressableFolder)
        HomeViewModelLog.AddingFolderToWatcher(_logger, folder.FolderName)

        Dim newWatched = New Watcher.WatchedFolder(folder.FolderName, folder.DisplayName)
        newWatched.IsSteamGame = TypeOf (folder) Is SteamFolder
        newWatched.LastCompressedSize = folder.CompressedBytes
        newWatched.LastUncompressedSize = folder.UncompressedBytes
        newWatched.LastCompressedDate = DateTime.Now
        newWatched.LastCheckedDate = DateTime.Now
        newWatched.LastCheckedSize = folder.CompressedBytes
        newWatched.LastSystemModifiedDate = DateTime.Now
        newWatched.CompressionLevel = If(folder.AnalysisResults.Any(), folder.AnalysisResults.Max(Function(f) f.CompressionMode), Core.WOFCompressionAlgorithm.NO_COMPRESSION)

        Dim skipList As New List(Of String)
        If folder.CompressionOptions.SkipListEnabled Then
            If folder.CompressionOptions.SkipList?.Count > 0 Then skipList.AddRange(folder.CompressionOptions.SkipList)
            If folder.CompressionOptions.SkipUserSubmittedFiletypes AndAlso folder.WikiPoorlyCompressedFiles?.Count > 0 Then
                skipList.AddRange(folder.WikiPoorlyCompressedFiles)
            End If
        End If
        newWatched.SkipList = skipList.Distinct(StringComparer.OrdinalIgnoreCase).ToList

        _watcher.AddOrUpdateWatched(newWatched)

    End Sub


    Public Async Sub Receive(message As WatcherAddedFolderToQueueMessage) Implements IRecipient(Of WatcherAddedFolderToQueueMessage).Receive
        Application.GetService(Of CustomSnackBarService).ShowAddedToQueue()
        Await AddFoldersAsync({message.Value})
    End Sub

    Public Async Sub Receive(message As SteamGamesAddedToQueueMessage) Implements IRecipient(Of SteamGamesAddedToQueueMessage).Receive
        Application.GetService(Of CustomSnackBarService).ShowAddedToQueue()
        Dim options = message.Value.GroupBy(Function(item) item.FolderPath, StringComparer.OrdinalIgnoreCase).ToDictionary(Function(group) group.Key, Function(group) group.Last().CompressionOptions, StringComparer.OrdinalIgnoreCase)
        Await AddFoldersAsync(options.Keys, options)
    End Sub
End Class
