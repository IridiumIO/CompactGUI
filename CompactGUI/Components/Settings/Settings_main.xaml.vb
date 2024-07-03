
Imports ModernWpf.Controls.Primitives

Class Settings_main

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        uiIsContextEnabled.IsChecked = SettingsHandler.AppSettings.IsContextIntegrated
        uiIsStartMenuEnabled.IsChecked = SettingsHandler.AppSettings.IsStartMenuEnabled
        uiShowNotifications.IsChecked = SettingsHandler.AppSettings.ShowNotifications
        uiEnableBackgroundWatcher.IsChecked = SettingsHandler.AppSettings.EnableBackgroundWatcher
        uiEnableBackgroundAutoCompression.IsChecked = SettingsHandler.AppSettings.EnableBackgroundAutoCompression
        comboBoxSkipUserResultsAggression.SelectedIndex = SettingsHandler.AppSettings.SkipUserFileTypesLevel
        uiScalingFactor.Value = SettingsHandler.AppSettings.WindowScalingFactor
        SetEnv()

    End Sub


    Private Async Sub SetEnv()
        Await Task.Run(Sub() Environment.SetEnvironmentVariable("IridiumIO", IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO"), EnvironmentVariableTarget.User))
    End Sub

    Private Sub uiIsContextEnabled_Checked(sender As Object, e As RoutedEventArgs) Handles uiIsContextEnabled.Checked

        Microsoft.Win32.Registry.SetValue _
            ("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "", "Compress Folder")

        Microsoft.Win32.Registry.SetValue _
            ("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "Icon", Process.GetCurrentProcess().MainModule.FileName)

        Microsoft.Win32.Registry.SetValue _
            ("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI\command", "", Process.GetCurrentProcess().MainModule.FileName + " " + """%1""")

        SettingsHandler.AppSettings.IsContextIntegrated = True
        SettingsHandler.AppSettings.Save()

    End Sub

    Private Sub uiIsContextEnabled_Unchecked(sender As Object, e As RoutedEventArgs) Handles uiIsContextEnabled.Unchecked

        Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI\command")

        Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI")

        SettingsHandler.AppSettings.IsContextIntegrated = False
        SettingsHandler.AppSettings.Save()

    End Sub


    Private Sub uiEditSkipListBTN_Click()
        Dim fl As New Settings_skiplistflyout
        fl.ShowDialog()
    End Sub

    Private Sub skipHelpIcon_MouseEnter(sender As Object, e As MouseEventArgs)
        FlyoutBase.ShowAttachedFlyout(sender)
    End Sub

    Private Sub uiShowNotifications_Checked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.ShowNotifications = True
        SettingsHandler.AppSettings.Save()

    End Sub

    Private Sub uiShowNotifications_Unchecked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.ShowNotifications = False
        SettingsHandler.AppSettings.Save()

    End Sub

    Private Sub uiScalingFactor_PreviewMouseUp(sender As Object, e As MouseButtonEventArgs)
        SettingsHandler.AppSettings.WindowScalingFactor = uiScalingFactor.Value
        SettingsHandler.AppSettings.Save()

    End Sub

    Private Sub uiScalingFactor_PreviewKeyUp(sender As Object, e As KeyEventArgs)
        SettingsHandler.AppSettings.WindowScalingFactor = uiScalingFactor.Value
        SettingsHandler.AppSettings.Save()
    End Sub

    Private Sub Canvas_MouseUp(sender As Object, e As MouseButtonEventArgs)
        Process.Start(New ProcessStartInfo("https://ko-fi.com/IridiumIO") With {.UseShellExecute = True})
    End Sub

    Private Sub Canvas_MouseUp_1(sender As Object, e As MouseButtonEventArgs)
        Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI") With {.UseShellExecute = True})

    End Sub

    Private Sub uiEnableBackgroundWatcher_Checked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.EnableBackgroundWatcher = True
        SettingsHandler.AppSettings.Save()
    End Sub

    Private Sub uiEnableBackgroundWatcher_Unchecked(sender As Object, e As RoutedEventArgs)
        uiEnableBackgroundAutoCompression.IsChecked = False
        SettingsHandler.AppSettings.EnableBackgroundWatcher = False
        SettingsHandler.AppSettings.Save()
    End Sub

    Private Sub uiEnableBackgroundAutoCompression_Checked(sender As Object, e As RoutedEventArgs)
        uiEnableBackgroundWatcher.IsChecked = True
        SettingsHandler.AppSettings.EnableBackgroundAutoCompression = True
        SettingsHandler.AppSettings.Save()
    End Sub

    Private Sub uiEnableBackgroundAutoCompression_Unchecked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.EnableBackgroundAutoCompression = False
        SettingsHandler.AppSettings.Save()
    End Sub
End Class
