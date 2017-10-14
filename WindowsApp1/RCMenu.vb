Public Class RCMenu

    Public Shared Sub WriteLocRegistry()
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\software\Fateful Productions\CompactGUI",
                                      "Executable Path", IO.Directory.GetCurrentDirectory)
    End Sub


End Class
