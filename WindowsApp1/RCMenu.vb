Public Class RCMenu

    Public Shared Sub WriteLocRegistry()

        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\software\CompactGUI",
                                      "Executable Path", IO.Directory.GetCurrentDirectory)

    End Sub




    Public Shared Sub WriteRCMenuRegistry()
        If My.User.IsInRole(ApplicationServices.BuiltInRole.Administrator) Then

            My.Computer.Registry.SetValue _
                ("HKEY_CLASSES_ROOT\Folder\shell\CompactGUI", "", "Compact Folder")

            My.Computer.Registry.SetValue _
                ("HKEY_CLASSES_ROOT\Folder\shell\CompactGUI", "Icon", Application.ExecutablePath)

            My.Computer.Registry.SetValue _
                ("HKEY_CLASSES_ROOT\Folder\shell\CompactGUI\command", "", Application.ExecutablePath + " " + """%1""")

            My.Settings.IsContextMenuEnabled = True

        Else

            If MsgBox("To enable this feature, CompactGUI has to be running as Administrator." _
                      & vbCrLf & vbCrLf & "Do you wish to restart the program as Administrator?",
                      MsgBoxStyle.YesNo, "Warning") = MsgBoxResult.Yes Then

                RunAsAdmin()

            Else
                My.Settings.IsContextMenuEnabled = False
                Info.checkEnableRCMenu.Checked = False
            End If

        End If

    End Sub




    Public Shared Sub DeleteRCMenuRegistry()
        If My.User.IsInRole(ApplicationServices.BuiltInRole.Administrator) Then

            My.Computer.Registry.ClassesRoot.DeleteSubKey("Folder\\shell\\CompactGUI\command")

            My.Computer.Registry.ClassesRoot.DeleteSubKey("Folder\\shell\\CompactGUI\")

            My.Settings.IsContextMenuEnabled = False

        Else

            If MsgBox("To enable this feature, CompactGUI has to be running as Administrator." _
                      & vbCrLf & vbCrLf & "Do you wish to restart the program as Administrator?",
                      MsgBoxStyle.YesNo, "Warning") = MsgBoxResult.Yes Then

                RunAsAdmin()

            Else
                My.Settings.IsContextMenuEnabled = True
                Info.checkEnableRCMenu.Checked = True
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
            Return
        End Try
        Application.Exit()

    End Sub

End Class
