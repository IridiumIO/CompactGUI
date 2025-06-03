Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging

Imports Wpf.Ui.Controls

Public Class WatcherViewModel : Inherits ObservableObject

    Private ReadOnly _snackbarService As CustomSnackBarService
    Public ReadOnly Property Watcher As Watcher.Watcher

    Public ReadOnly Property RemoveWatcherCommand As IRelayCommand = New RelayCommand(Of Watcher.WatchedFolder)(Sub(f) Watcher.RemoveWatched(f))

    Public ReadOnly Property ReCompressWatchedCommand As IRelayCommand = New RelayCommand(Of Watcher.WatchedFolder)(Sub(f) AddWatchedFolderToQueue(f))

    Public Property RefreshWatchedCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf RefreshWatchedAsync)

    Public ReadOnly Property ReAnalyseWatchedCommand As IRelayCommand = New AsyncRelayCommand(Of Watcher.WatchedFolder)(Function(f) ReAnalyseWatchedAsync(f))

    Public Property ManuallyAddFolderToWatcherCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf ManuallyAddFolderToWatcher)



    Public Sub New(watcher As Watcher.Watcher, snackbarService As CustomSnackBarService)
        Me.Watcher = watcher
        _snackbarService = snackbarService
    End Sub



    Public Async Function RefreshWatchedAsync() As Task
        DeleteWatchersWithNonExistentFolders()

        Await Task.Run(Function() Watcher.ParseWatchers(True))
    End Function


    Private Async Function ReAnalyseWatchedAsync(watchedfolder As Watcher.WatchedFolder) As Task
        Await Task.Run(Function() Watcher.ParseSingleWatcher(watchedfolder))
    End Function


    Private Sub DeleteWatchersWithNonExistentFolders()

        Dim watchersToRemove As New List(Of Watcher.WatchedFolder)

        For Each wx In Watcher.WatchedFolders
            If Not IO.Directory.Exists(wx.Folder) Then
                watchersToRemove.Add(wx)
            End If
        Next



        For Each wx In watchersToRemove
            Watcher.RemoveWatched(wx)
        Next
    End Sub

    Private Sub AddWatchedFolderToQueue(folder As Watcher.WatchedFolder)

        WeakReferenceMessenger.Default.Send(New WatcherAddedFolderToQueueMessage(folder.Folder))
    End Sub


    Private Async Function ManuallyAddFolderToWatcher() As Task

        Dim path As String = ""

        Dim folderSelector As New Microsoft.Win32.OpenFolderDialog
        folderSelector.ShowDialog()
        If folderSelector.FolderName = "" Then Return
        path = folderSelector.FolderName

        Dim validFolder = Core.verifyFolder(path)
        If validFolder <> Core.SharedMethods.FolderVerificationResult.Valid Then

            _snackbarService.ShowInvalidFoldersMessage(New List(Of String) From {path}, New List(Of Core.SharedMethods.FolderVerificationResult) From {validFolder})

            Return
        End If

        Dim newFolder = Await AddFolderAsync(path)

        Dim newWatched = New Watcher.WatchedFolder With {
           .Folder = newFolder.FolderName,
           .DisplayName = newFolder.DisplayName,
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
