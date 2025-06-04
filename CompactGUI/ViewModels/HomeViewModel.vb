Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging

Imports CompactGUI.Core.SharedMethods

Imports PropertyChanged

Partial Public Class HomeViewModel : Inherits ObservableObject : Implements IRecipient(Of WatcherAddedFolderToQueueMessage)

    Public Property Folders As New ObservableCollection(Of CompressableFolder)

    <OnChangedMethod(NameOf(OnSelectedFolderChanged))>
    <AlsoNotifyFor(NameOf(SelectedFolderViewModel))>
    Public Property SelectedFolder As CompressableFolder

    Public ReadOnly Property SelectedFolderViewModel As FolderViewModel
        Get
            If SelectedFolder Is Nothing Then Return Nothing
            Return New FolderViewModel(SelectedFolder, _watcher, _snackbarService)
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

    Sub New(watcher As Watcher.Watcher, snackbarService As CustomSnackBarService)
        WeakReferenceMessenger.Default.Register(Of WatcherAddedFolderToQueueMessage)(Me)
        AddHandler Folders.CollectionChanged, AddressOf OnFoldersCollectionChanged
        _watcher = watcher
        _snackbarService = snackbarService
    End Sub



    Public Sub OnSelectedFolderChanged()

        WeakReferenceMessenger.Default.Send(New BackgroundImageChangedMessage(SelectedFolder?.FolderBGImage))

    End Sub





    Private Sub OnAnyFolderPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        If e.PropertyName = NameOf(SelectedFolder.FolderActionState) Then
            OnPropertyChanged(NameOf(HomeViewModelState))
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


        Dim invalidFolders = GetInvalidFolders(folderPaths.ToArray)
        Dim validFolders = folderPaths.Except(invalidFolders.InvalidFolders)

        If invalidFolders.InvalidFolders.Count > 0 Then

            _snackbarService.ShowInvalidFoldersMessage(invalidFolders.InvalidFolders, invalidFolders.InvalidMessages)
        End If

        For Each folderName In validFolders

            Dim newFolder As CompressableFolder = CompressableFolderFactory.CreateCompressableFolder(folderName)
            If Not Folders.Any(Function(f) f.FolderName = newFolder.FolderName) Then
                Folders.Add(newFolder)
                SelectedFolder = newFolder
            End If

            Dim res = Await newFolder.AnalyseFolderAsync
            If TypeOf (newFolder) Is SteamFolder Then
                Await CType(newFolder, SteamFolder).GetWikiResults()
            Else
                If SettingsHandler.AppSettings.EstimateCompressionForNonSteamFolders Then
                    Await newFolder.GetEstimatedCompression()
                End If

            End If

            If _watcher.WatchedFolders.Any(Function(w) w.Folder = newFolder.FolderName) Then
                newFolder.CompressionOptions.WatchFolderForChanges = True
            End If



        Next


    End Function




    Public Property RemoveFolderCommand As IRelayCommand = New RelayCommand(Of CompressableFolder)(Sub(folder) RemoveFolder(folder))
    Public Sub RemoveFolder(folder As CompressableFolder)
        If Not CanRemoveFolder() Then
            Application.GetService(Of CustomSnackBarService)().ShowCannotRemoveFolder()
            Return
        End If

        If folder Is Nothing Then Return
        Dim index = Folders.IndexOf(folder)
        folder.CancelEstimation()
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

    Public Property CompressAllCommand As AsyncRelayCommand = New AsyncRelayCommand(execute:=AddressOf CompressAllAsync, canExecute:=Function() CanCompressAll())

    Private Function CanCompressAll() As Boolean
        Return HomeViewModelState <> ActionState.Working AndAlso Not Folders.Any(Function(f) f.FolderActionState = ActionState.Analysing)
    End Function


    Public ReadOnly Property HomeViewModelState As ActionState
        Get
            If Compressing OrElse Folders.Any(Function(f) f.FolderActionState = ActionState.Working) Then
                Return ActionState.Working
            End If
            If Folders.Any(Function(f) f.FolderActionState = ActionState.Analysing) Then
                Return ActionState.Analysing
            End If
            If Folders.All(Function(f) f.FolderActionState = ActionState.Results) Then
                Return ActionState.Results
            End If
            Return ActionState.Idle
        End Get

    End Property

    Private Property Compressing As Boolean = False
    Private Async Function CompressAllAsync() As Task

        Await _watcher.DisableBackgrounding()

        Compressing = True
        Core.PreventSleep()
        Dim tasks As New List(Of Task)()
        Dim foldersToCompress = Folders.Where(Function(f) f.FolderActionState = ActionState.Idle).ToList
        For Each folder In foldersToCompress
            If folder.FolderActionState = ActionState.Idle Then
                Await Task.Run(Async Function()
                                   Debug.WriteLine("Compressing " & folder.FolderName)
                                   Dim ret = Await folder.CompressFolder()
                                   Dim analysis = Await folder.AnalyseFolderAsync

                                   If SettingsHandler.AppSettings.ShowNotifications Then

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

        Core.RestoreSleep()

        Await _watcher.EnableBackgrounding()
    End Function


    Public Sub AddOrUpdateFolderWatcher(folder As CompressableFolder)
        Debug.WriteLine("Adding folder to watcher: " & folder.FolderName)

        Dim newWatched = New Watcher.WatchedFolder
        newWatched.Folder = folder.FolderName
        newWatched.DisplayName = folder.DisplayName
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
