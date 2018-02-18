Class RCMenu

    Shared Sub WriteLocRegistry()
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\software\CompactGUI", "Executable Path", IO.Directory.GetCurrentDirectory)
    End Sub


    Shared Sub WriteRCMenuRegistry()

        My.Computer.Registry.SetValue _
            ("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "", "Compact Folder")

        My.Computer.Registry.SetValue _
            ("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "Icon", Application.ExecutablePath)

        My.Computer.Registry.SetValue _
            ("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI\command", "", Application.ExecutablePath + " " + """%1""")

        My.Settings.IsContextMenuEnabled = True

    End Sub


    Public Shared Sub DeleteRCMenuRegistry()

        My.Computer.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI\command")

        My.Computer.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI")

        My.Settings.IsContextMenuEnabled = False

    End Sub


    Public Shared Sub RunAsAdmin()
        Dim startInfo As ProcessStartInfo = New ProcessStartInfo With {
            .UseShellExecute = True,
            .WorkingDirectory = Environment.CurrentDirectory,
            .FileName = Application.ExecutablePath,
            .Verb = "runas"
        }

        Process.Start(startInfo)

        Application.Exit()

    End Sub

End Class
