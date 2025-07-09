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

Partial Public NotInheritable Class HomeViewModel : Inherits ObservableRecipient : Implements IRecipient(Of WatcherAddedFolderToQueueMessage)

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
    End Sub

    Private Sub OnFoldersCollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
        OnPropertyChanged(NameOf(HomeViewModelState))
        If e.Action = NotifyCollectionChangedAction.Add Then
            For Each folder As CompressableFolder In e.NewItems
                AddHandler folder.PropertyChanged, AddressOf OnAnyFolderPropertyChanged
            Next
        ElseIf e.Action = NotifyCollectionChangedAction.Remove Then
            For Each folder As CompressableFolder In e.OldItems
                RemoveHandler folder.PropertyChanged, AddressOf OnAnyFolderPropertyChanged
            Next
        End If

        OnPropertyChanged(NameOf(HomeViewIsFresh))
    End Sub



    Public Async Function AddFoldersAsync(folderPaths As IEnumerable(Of String)) As Task

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
                If watchedFolder.CompressionLevel <> Core.WOFCompressionAlgorithm.NO_COMPRESSION Then
                    newFolder.CompressionOptions.SelectedCompressionMode = Core.WOFHelper.CompressionModeFromWOFMode(watchedFolder.CompressionLevel)
                End If

            End If



        Next


    End Function




    <RelayCommand>
    Public Sub RemoveFolder(folder As CompressableFolder)
        If Not CanRemoveFolder() Then
            Application.GetService(Of CustomSnackBarService)().ShowCannotRemoveFolder()
            Return
        End If

        If folder Is Nothing Then Return
        Dim index = Folders.IndexOf(folder)
        _compressableFolderService.CancelEstimation(folder)
        folder.Dispose()

        Dim value As FolderViewModel = Nothing

        If _folderViewModels.TryGetValue(folder, value) Then
            value.Dispose()
            _folderViewModels.Remove(folder)
        End If

        Folders.Remove(folder)

        If SelectedFolder IsNot Nothing OrElse Folders.Count = 0 Then Return
        SelectedFolder = If(index < Folders.Count, Folders(index), Folders.Last())
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

            If Compressing OrElse Folders.Any(Function(f) f.FolderActionState = ActionState.Working) Then
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

        _watcher.AddOrUpdateWatched(newWatched)

    End Sub


    Public Async Sub Receive(message As WatcherAddedFolderToQueueMessage) Implements IRecipient(Of WatcherAddedFolderToQueueMessage).Receive
        Application.GetService(Of CustomSnackBarService).ShowAddedToQueue()
        Await AddFoldersAsync({message.Value})
    End Sub
End Class
