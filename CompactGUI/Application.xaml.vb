Class Application
    Private Sub Application_Startup(sender As Object, e As StartupEventArgs)

        Dim mainWindow = New MainWindow()
        If e.Args.Length = 1 Then
            mainWindow.ViewModel.SelectFolder(e.Args(0))
        End If
        mainWindow.Show()

    End Sub

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

End Class
