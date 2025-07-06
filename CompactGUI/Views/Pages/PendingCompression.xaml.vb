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

    Private Sub UiChkSkipPoorlyCompressed_Checked(sender As Object, e As RoutedEventArgs)
        _settingsService.AppSettings.SkipNonCompressable = True
        _settingsService.SaveSettings()

    End Sub

    Private Sub UiChkSkipPoorlyCompressed_Unchecked(sender As Object, e As RoutedEventArgs)
        If Not IsVisible Then Return ' Prevents issues when the page is not fully loaded
        _settingsService.AppSettings.SkipNonCompressable = False
        _settingsService.SaveSettings()
    End Sub

    Private Sub UiChkSkipUserPoorlyCompressed_Checked(sender As Object, e As RoutedEventArgs)
        _settingsService.AppSettings.SkipUserNonCompressable = True
        _settingsService.SaveSettings()
    End Sub

    Private Sub UiChkSkipUserPoorlyCompressed_Unchecked(sender As Object, e As RoutedEventArgs)
        If Not IsVisible Then Return
        _settingsService.AppSettings.SkipUserNonCompressable = False
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
End Class
