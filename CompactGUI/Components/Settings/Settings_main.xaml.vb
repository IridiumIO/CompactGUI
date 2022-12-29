
Imports ModernWpf.Controls.Primitives

Public Class Settings_main
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        uiIsContextEnabled.IsChecked = SettingsHandler.AppSettings.IsContextIntegrated
        uiIsStartMenuEnabled.IsChecked = SettingsHandler.AppSettings.IsStartMenuEnabled
        uiShowNotifications.IsChecked = SettingsHandler.AppSettings.ShowNotifications
        comboBoxSkipUserResultsAggression.SelectedIndex = SettingsHandler.AppSettings.SkipUserFileTypesLevel

        SetEnv()

    End Sub

    Private Shared Async Sub SetEnv()
        Await Task.Run(Sub() Environment.SetEnvironmentVariable("IridiumIO", IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO"), EnvironmentVariableTarget.User))
    End Sub

    Private Sub UiIsContextEnabled_Checked(sender As Object, e As RoutedEventArgs) Handles uiIsContextEnabled.Checked

        Microsoft.Win32.Registry.SetValue _
            ("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "", "Compress Folder")

        Microsoft.Win32.Registry.SetValue _
            ("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "Icon", Environment.ProcessPath)

        Microsoft.Win32.Registry.SetValue _
            ("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI\command", "", Environment.ProcessPath & " " & """%1""")

        SettingsHandler.AppSettings.IsContextIntegrated = True
        Settings.Save()

    End Sub

    Private Sub UiIsContextEnabled_Unchecked(sender As Object, e As RoutedEventArgs) Handles uiIsContextEnabled.Unchecked

        Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI\command")

        Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI")

        SettingsHandler.AppSettings.IsContextIntegrated = False
        Settings.Save()

    End Sub


    Private Shared Sub UiEditSkipListBTN_Click()
        Dim fl As New Settings_skiplistflyout
        fl.ShowDialog()
    End Sub

    Private Sub SkipHelpIcon_MouseEnter(sender As Object, e As MouseEventArgs)
        FlyoutBase.ShowAttachedFlyout(CType(sender, FrameworkElement))
    End Sub

    Private Sub uiShowNotifications_Checked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.ShowNotifications = True
        SettingsHandler.AppSettings.Save()

    End Sub

    Private Sub uiShowNotifications_Unchecked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.ShowNotifications = False
        SettingsHandler.AppSettings.Save()

    End Sub
End Class
