Imports CompactGUI.Core.Settings

Public Class PendingCompression

    Private ReadOnly _settingsService As ISettingsService

    Sub New()
        InitializeComponent()
        _settingsService = Application.GetService(Of ISettingsService)
    End Sub

    Private Sub CompressionMode_Radio_Checked(sender As Object, e As RoutedEventArgs)
        Dim radio As RadioButton = CType(sender, RadioButton)

        Dim ret As FolderViewModel = CType(radio.DataContext, FolderViewModel)

        _settingsService.AppSettings.SelectedCompressionMode = ret.Folder.CompressionOptions.SelectedCompressionMode
        _settingsService.SaveSettings()

    End Sub

    Private Sub uiChkWatchFolderForChanges_Checked(sender As Object, e As RoutedEventArgs)
        _settingsService.AppSettings.WatchFolderForChanges = True
        _settingsService.SaveSettings()
    End Sub

    Private Sub uiChkWatchFolderForChanges_Unchecked(sender As Object, e As RoutedEventArgs)
        If Not IsVisible Then Return
        _settingsService.AppSettings.WatchFolderForChanges = False
        _settingsService.SaveSettings()
    End Sub

    Private Sub UiEditSkipList_Click(sender As Object, e As RoutedEventArgs)
        Dim folderViewModel = TryCast(DataContext, FolderViewModel)
        If folderViewModel Is Nothing Then Return

        Dim flyout As New Settings_skiplistflyout(folderViewModel.Folder)
        flyout.Owner = Window.GetWindow(Me)
        flyout.ShowDialog()

        folderViewModel.Folder.NotifyPropertyChanged(NameOf(CompressableFolder.SkippedFileCount))
    End Sub

    Private Sub UiChkSkipListEnabled_Checked(sender As Object, e As RoutedEventArgs)
        NotifySkippedFileCount()
    End Sub

    Private Sub UiChkSkipListEnabled_Unchecked(sender As Object, e As RoutedEventArgs)
        NotifySkippedFileCount()
    End Sub

    Private Sub NotifySkippedFileCount()
        Dim folderViewModel = TryCast(DataContext, FolderViewModel)
        If folderViewModel Is Nothing Then Return
        folderViewModel.Folder.NotifyPropertyChanged(NameOf(CompressableFolder.SkippedFileCount))
    End Sub
End Class
