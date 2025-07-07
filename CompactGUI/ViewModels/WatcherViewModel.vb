Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging

Imports Wpf.Ui.Controls

Public NotInheritable Class WatcherViewModel : Inherits ObservableObject

    Private ReadOnly _snackbarService As CustomSnackBarService
    Public ReadOnly Property Watcher As Watcher.Watcher

    Public Sub New(watcher As Watcher.Watcher, snackbarService As CustomSnackBarService)
        Me.Watcher = watcher
        _snackbarService = snackbarService
    End Sub


    <RelayCommand>
    Private Async Function RemoveWatcher(watchedFolder As Watcher.WatchedFolder) As Task
        If watchedFolder Is Nothing Then Return
        Await Application.Current.Dispatcher.InvokeAsync(Sub() Watcher.RemoveWatched(watchedFolder))
    End Function

    <RelayCommand>
    Private Async Function RefreshWatched() As Task
        Await Watcher.DeleteWatchersWithNonExistentFolders()
        Await Task.Run(Function() Watcher.ParseWatchers(True))
    End Function

    <RelayCommand>
    Private Async Function ReAnalyseWatched(watchedfolder As Watcher.WatchedFolder) As Task
        Await Task.Run(Function() Watcher.ParseSingleWatcher(watchedfolder))
    End Function



    <RelayCommand>
    Private Sub AddWatchedFolderToQueue(folder As Watcher.WatchedFolder)

        WeakReferenceMessenger.Default.Send(New WatcherAddedFolderToQueueMessage(folder.Folder))
    End Sub

    <RelayCommand>
    Private Async Function ManuallyAddFolderToWatcher() As Task

        Dim folderSelector As New Microsoft.Win32.OpenFolderDialog
        folderSelector.ShowDialog()
        If folderSelector.FolderName = "" Then Return
        Dim path As String = folderSelector.FolderName
        Dim validFolder = Core.SharedMethods.VerifyFolder(path)
        If validFolder <> Core.SharedMethods.FolderVerificationResult.Valid Then

            _snackbarService.ShowInvalidFoldersMessage(New List(Of String) From {path}, New List(Of Core.SharedMethods.FolderVerificationResult) From {validFolder})

            Return
        End If

        Dim newFolder = Await AddFolderAsync(path)

        Dim newWatched = New Watcher.WatchedFolder(newFolder.FolderName, newFolder.DisplayName) With {
           .IsSteamGame = TypeOf (newFolder) Is SteamFolder,
           .LastCompressedSize = 0,
           .LastUncompressedSize = 0,
           .LastCompressedDate = DateTime.UnixEpoch,
           .LastCheckedDate = DateTime.UnixEpoch,
           .LastCheckedSize = 0,
           .LastSystemModifiedDate = DateTime.UnixEpoch,
           .CompressionLevel = Core.WOFCompressionAlgorithm.NO_COMPRESSION}

        Watcher.AddOrUpdateWatched(newWatched)
        Await Watcher.Analyse(path, True)

    End Function


    Public Async Function AddFolderAsync(folderPath As String) As Task(Of CompressableFolder)

        If GetInvalidFolders({folderPath}).InvalidFolders.Count > 0 Then
            Dim msgError As New ContentDialog With {.Title = "Invalid Folder", .Content = $"{folderPath}", .CloseButtonText = "OK"}
            Await msgError.ShowAsync()
            Return Nothing
        End If

        Return CompressableFolderFactory.CreateCompressableFolder(folderPath)

    End Function



End Class
