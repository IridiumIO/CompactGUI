Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging

Imports IWshRuntimeLibrary

Imports Wpf.Ui.Controls

Public Class WatcherViewModel : Inherits ObservableObject

    Public ReadOnly Property Watcher As Watcher.Watcher

    Public ReadOnly Property RemoveWatcherCommand As IRelayCommand = New RelayCommand(Of Watcher.WatchedFolder)(Sub(f) Watcher.RemoveWatched(f))

    Public ReadOnly Property ReCompressWatchedCommand As IRelayCommand = New RelayCommand(Of Watcher.WatchedFolder)(Sub(f) AddWatchedFolderToQueue(f))

    Public Property RefreshWatchedCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf RefreshWatchedAsync)
    Public Property ManuallyAddFolderToWatcherCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf ManuallyAddFolderToWatcher)

    Public Sub New(watcher As Watcher.Watcher)
        Me.Watcher = watcher
    End Sub



    Public Async Function RefreshWatchedAsync() As Task

        Await Task.Run(Function() Watcher.ParseWatchers(True))
    End Function


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
        If Not validFolder.isValid Then
            Dim msgError As New ContentDialog With {.Title = "Invalid Folder", .Content = $"{validFolder.msg}", .CloseButtonText = "OK"}
            Await msgError.ShowAsync()
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
