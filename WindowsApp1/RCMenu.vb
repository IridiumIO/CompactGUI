Public Class RCMenu

    Public Shared Sub WriteLocRegistry()
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\software\Fateful Productions\CompactGUI",
                                      "Executable Path", IO.Directory.GetCurrentDirectory)




    End Sub

    Public Shared Sub WriteRCMenuRegistry()
        If My.User.IsInRole(ApplicationServices.BuiltInRole.Administrator) Then

            My.Computer.Registry.SetValue("HKEY_CLASSES_ROOT\Folder\shell\CompactGUI", "", "Compact Folder")
            My.Computer.Registry.SetValue("HKEY_CLASSES_ROOT\Folder\shell\CompactGUI\command", "", Application.ExecutablePath)

        Else

            If MsgBox("To enable this feature, CompactGUI has to be running as Administrator." _
                      & vbCrLf & vbCrLf & "Do you wish to restart the program as Administrator?", MsgBoxStyle.YesNo, "Warning") = MsgBoxResult.Yes Then

                RunAsAdmin()

            End If
        End If
    End Sub

    Public Shared Sub RunAsAdmin()
        Dim startInfo As ProcessStartInfo = New ProcessStartInfo With {
            .UseShellExecute = True,
            .WorkingDirectory = Environment.CurrentDirectory,
            .FileName = Application.ExecutablePath,
            .Verb = "runas"
        }
        Try
            Dim p As Process = Process.Start(startInfo)
        Catch ex As Exception
            Return 'If cancelled, do nothing
        End Try
        Application.Exit()

    End Sub
End Class
