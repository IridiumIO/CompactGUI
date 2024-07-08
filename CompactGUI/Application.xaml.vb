Imports System.IO
Imports System.IO.Pipes
Imports System.Runtime.InteropServices
Imports System.Threading

Class Application

    Public Shared ReadOnly mutex As New Mutex(False, "Global\CompactGUI")

    Private pipeServerCancellation As New CancellationTokenSource()
    Private pipeServerTask As Task

    Private Shadows mainWindow As MainWindow

    Private Async Sub Application_Startup(sender As Object, e As StartupEventArgs)

        SettingsHandler.InitialiseSettings()

        Dim acquiredMutex As Boolean

        Try
            acquiredMutex = mutex.WaitOne(0, False)
        Catch ex As AbandonedMutexException
            ' This means the mutex was acquired successfully,
            ' but its last owner exited abruptly, without releasing it.
            ' acquiredMutex should still be True here, but further error checking
            ' on shared program state could be added here as well.
            acquiredMutex = True
        End Try

        If Not SettingsHandler.AppSettings.AllowMultiInstance AndAlso Not acquiredMutex Then

            If e.Args.Length <> 0 AndAlso e.Args(0) = "-tray" Then
                MessageBox.Show("An instance of CompactGUI is already running")
                Application.Current.Shutdown()

            End If

            Using client = New NamedPipeClientStream(".", "CompactGUI", PipeDirection.Out)
                client.Connect()
                Using writer = New StreamWriter(client)
                    If e.Args.Length > 0 Then
                        writer.WriteLine(e.Args(0))
                    End If
                End Using
            End Using

            Application.Current.Shutdown()
        ElseIf Not SettingsHandler.AppSettings.AllowMultiInstance Then
            pipeServerTask = ProcessNextInstanceMessage()
        End If


        Dim registryTask = SettingsViewModel.InitializeEnvironment

        GC.Collect()
        mainWindow = New MainWindow()
        If e.Args.Length = 1 Then
            If e.Args(0).ToString = "-tray" Then
                mainWindow.Show()
                mainWindow.ViewModel.ClosingCommand.Execute(New ComponentModel.CancelEventArgs(True))
                Return
            End If
            mainWindow.Show()
            Await mainWindow.ViewModel.SelectFolderAsync(e.Args(0))
        End If

        If SettingsHandler.AppSettings.StartInSystemTray Then
            mainWindow.Show()
            mainWindow.ViewModel.ClosingCommand.Execute(New ComponentModel.CancelEventArgs(True))
            Return
        End If

        mainWindow.Show()
        Await registryTask
    End Sub


    Private Async Function ProcessNextInstanceMessage() As Task
        While Not pipeServerCancellation.IsCancellationRequested
            Using server = New NamedPipeServerStream("CompactGUI",
                                                 PipeDirection.In,
                                                 -1, ' Allow multiple client connections
                                                 PipeTransmissionMode.Byte,
                                                 PipeOptions.Asynchronous)
                Try
                    Await server.WaitForConnectionAsync(pipeServerCancellation.Token)

                    Using reader = New StreamReader(server)
                        Dim message = Await reader.ReadLineAsync()
                        Await mainWindow.Dispatcher.InvokeAsync(Async Function()
                                                                    mainWindow.Show()
                                                                    mainWindow.WindowState = WindowState.Normal
                                                                    mainWindow.Topmost = True
                                                                    mainWindow.Activate()
                                                                    mainWindow.Topmost = False
                                                                    If message IsNot Nothing Then
                                                                        Await mainWindow.ViewModel.SelectFolderAsync(message)
                                                                    End If
                                                                End Function).Task
                    End Using
                Catch ex As OperationCanceledException
                    ' Handle the cancellation exception if the operation was cancelled
                    Return
                Finally
                    If server.IsConnected Then
                        server.Disconnect() ' Ensure the server is ready for the next connection
                    End If
                End Try
            End Using
        End While
    End Function

    Public Async Function ShutdownPipeServer() As Task
        If pipeServerTask IsNot Nothing Then
            pipeServerCancellation.Cancel()
            Await pipeServerTask
        End If
    End Function

End Class
