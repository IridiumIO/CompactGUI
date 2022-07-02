Imports System.Runtime.InteropServices
Imports System.Threading

Class Application

    Public Shared ReadOnly mutex As New Mutex(False, "Global\CompactGUI")
    Private Sub Application_Startup(sender As Object, e As StartupEventArgs)

        If Not mutex.WaitOne(0, False) Then
            MessageBox.Show("An instance of CompactGUI is already running")
            Application.Current.Shutdown()
        End If

        GC.Collect()
        Dim mainWindow = New MainWindow()
        If e.Args.Length = 1 Then
            If e.Args(0).ToString = "-tray" Then
                mainWindow.Show()
                mainWindow.ViewModel.ClosingCommand.Execute(New ComponentModel.CancelEventArgs(True))
                Return
            End If

            mainWindow.ViewModel.SelectFolder(e.Args(0))
            End If
            mainWindow.Show()

    End Sub

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

End Class
