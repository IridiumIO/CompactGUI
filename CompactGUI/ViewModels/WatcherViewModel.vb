Imports System.ComponentModel
Imports System.Threading
Imports System.Windows.Data

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging

Imports CompactGUI.Watcher

Imports Wpf.Ui.Controls

Public NotInheritable Class WatcherViewModel : Inherits ObservableObject

    Private ReadOnly _snackbarService As CustomSnackBarService
    Public ReadOnly Property Watcher As Watcher.Watcher

    <ObservableProperty>
    Private _searchText As String

    Public ReadOnly Property FilteredWatchedFolders As ICollectionView

    Public ReadOnly Property HasFilteredResults As Boolean
        Get
            Return Not FilteredWatchedFolders.IsEmpty
        End Get
    End Property

    Public Sub New(watcher As Watcher.Watcher, snackbarService As CustomSnackBarService)
        Me.Watcher = watcher
        _snackbarService = snackbarService

        FilteredWatchedFolders = CollectionViewSource.GetDefaultView(Watcher.WatchedFolders)
        FilteredWatchedFolders.SortDescriptions.Add(New SortDescription("DisplayName", ListSortDirection.Ascending))
        FilteredWatchedFolders.Filter = AddressOf FilterWatchedFolders
        AddHandler FilteredWatchedFolders.CollectionChanged, Sub(s, e) OnPropertyChanged(NameOf(HasFilteredResults))
    End Sub

    Private Sub OnSearchTextChanged(value As String)
        FilteredWatchedFolders.Refresh()
        OnPropertyChanged(NameOf(HasFilteredResults))
    End Sub

    Private Function FilterWatchedFolders(obj As Object) As Boolean
        If String.IsNullOrWhiteSpace(SearchText) Then Return True
        Dim item = TryCast(obj, Watcher.WatchedFolder)
        If item Is Nothing Then Return False

        Return (item.DisplayName IsNot Nothing AndAlso item.DisplayName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) OrElse
               (item.Folder IsNot Nothing AndAlso item.Folder.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
    End Function



    <RelayCommand>
    Public Async Function RunWatcher(token As CancellationToken) As Task
        Await Watcher.RunWatcher(True, token)
    End Function

    <RelayCommand>
    Public Sub CancelBackgrounding()
        RunWatcherCommand.Cancel()
        Watcher.BGCompactor.CancelCompacting()
        Application.Current.Dispatcher.Invoke(Sub() CancelBackgroundingCommand.NotifyCanExecuteChanged())
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

        Return Await CompressableFolderFactory.CreateCompressableFolder(folderPath)

    End Function



End Class
