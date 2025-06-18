Public Class PendingCompression
    Private Sub CompressionMode_Radio_Checked(sender As Object, e As RoutedEventArgs)
        Dim radio As RadioButton = CType(sender, RadioButton)

        Dim ret As FolderViewModel = CType(radio.DataContext, FolderViewModel)

        SettingsHandler.AppSettings.SelectedCompressionMode = ret.Folder.CompressionOptions.SelectedCompressionMode
        SettingsHandler.WriteToFile()

    End Sub

    Private Sub UiChkSkipPoorlyCompressed_Checked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.SkipNonCompressable = True
        SettingsHandler.WriteToFile()

    End Sub

    Private Sub UiChkSkipPoorlyCompressed_Unchecked(sender As Object, e As RoutedEventArgs)
        If Not IsVisible Then Return ' Prevents issues when the page is not fully loaded
        SettingsHandler.AppSettings.SkipNonCompressable = False
        SettingsHandler.WriteToFile()
    End Sub

    Private Sub UiChkSkipUserPoorlyCompressed_Checked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.SkipUserNonCompressable = True
        SettingsHandler.WriteToFile()
    End Sub

    Private Sub UiChkSkipUserPoorlyCompressed_Unchecked(sender As Object, e As RoutedEventArgs)
        If Not IsVisible Then Return
        SettingsHandler.AppSettings.SkipUserNonCompressable = False
        SettingsHandler.WriteToFile()
    End Sub

    Private Sub uiChkWatchFolderForChanges_Checked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.WatchFolderForChanges = True
        SettingsHandler.WriteToFile()
    End Sub

    Private Sub uiChkWatchFolderForChanges_Unchecked(sender As Object, e As RoutedEventArgs)
        If Not IsVisible Then Return
        SettingsHandler.AppSettings.WatchFolderForChanges = False
        SettingsHandler.WriteToFile()
    End Sub
End Class
