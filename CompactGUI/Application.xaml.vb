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
        Dim acquiredMutex As Boolean = mutex.WaitOne(0, False)

        If Not acquiredMutex Then
            If Not SettingsHandler.AppSettings.AllowMultiInstance Then
                HandleSecondInstance(e.Args)
                Return
            End If
        Else
            If Not SettingsHandler.AppSettings.AllowMultiInstance Then
                pipeServerTask = ProcessNextInstanceMessage()
            End If
        End If

        mainWindow = New MainWindow()
        Dim shouldMinimizeToTray As Boolean = (e.Args.Length = 1 AndAlso e.Args(0).ToString = "-tray") OrElse
                                          (SettingsHandler.AppSettings.StartInSystemTray AndAlso e.Args.Length = 0)

        If shouldMinimizeToTray Then
            mainWindow.Show()
            mainWindow.ViewModel.ClosingCommand.Execute(New ComponentModel.CancelEventArgs(True))
        Else
            If e.Args.Length = 1 Then
                Await mainWindow.ViewModel.SelectFolderAsync(e.Args(0))
            End If
            mainWindow.Show()
        End If

        Await SettingsViewModel.InitializeEnvironment()
    End Sub

    Private Sub HandleSecondInstance(args As String())
        If args.Length > 0 AndAlso args(0) <> "-tray" Then
            Using client = New NamedPipeClientStream(".", "CompactGUI", PipeDirection.Out)
                client.Connect()
                Using writer = New StreamWriter(client)
                    writer.WriteLine(args(0))
                End Using
            End Using
        Else
            MessageBox.Show("An instance of CompactGUI is already running")
        End If
        Application.Current.Shutdown()
    End Sub

    Private Async Function ProcessNextInstanceMessage() As Task
        While Not pipeServerCancellation.IsCancellationRequested
            Using server = New NamedPipeServerStream("CompactGUI", PipeDirection.In, -1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous)
                Try
                    Await server.WaitForConnectionAsync(pipeServerCancellation.Token)
                    Using reader = New StreamReader(server)
                        Dim message = Await reader.ReadLineAsync()
                        Await mainWindow.Dispatcher.InvokeAsync(Async Function()
                                                                    If message IsNot Nothing Then
                                                                        mainWindow.Show()
                                                                        mainWindow.WindowState = WindowState.Normal
                                                                        mainWindow.Activate()
                                                                        Await mainWindow.ViewModel.SelectFolderAsync(message)
                                                                    End If
                                                                End Function).Task
                    End Using
                Catch ex As OperationCanceledException
                    Return
                Finally
                    If server.IsConnected Then server.Disconnect()
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