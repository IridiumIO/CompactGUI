
Class Settings_main

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        uiIsContextEnabled.IsChecked = SettingsHandler.AppSettings.IsContextIntegrated
        uiIsStartMenuEnabled.IsChecked = SettingsHandler.AppSettings.IsStartMenuEnabled
        uiSkipFiletypes.IsChecked = SettingsHandler.AppSettings.SkipNonCompressable

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
End Class
