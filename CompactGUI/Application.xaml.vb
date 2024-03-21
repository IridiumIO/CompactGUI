Imports System.IO
Imports System.IO.Pipes
Imports System.Runtime.InteropServices
Imports System.Threading

Class Application

    Public Shared ReadOnly mutex As New Mutex(False, "Global\CompactGUI")

    Private pipeServerCancellation As New CancellationTokenSource()
    Private pipeServerTask As Task

    Private mainWindow As MainWindow

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
                    writer.WriteLine(e.Args(0))
                End Using
            End Using

            Application.Current.Shutdown()
        ElseIf Not SettingsHandler.AppSettings.AllowMultiInstance Then
            pipeServerTask = ProcessNextInstanceMessage()
        End If



        GC.Collect()
        mainWindow = New MainWindow()
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


    Private Async Function ProcessNextInstanceMessage() As Task
        Using server = New NamedPipeServerStream("CompactGUI",
                                                 PipeDirection.In,
                                                 1,
                                                 PipeTransmissionMode.Byte,
                                                 PipeOptions.Asynchronous)
            While Not pipeServerCancellation.IsCancellationRequested
                Try
                    Await server.WaitForConnectionAsync(pipeServerCancellation.Token)
                Catch ex As OperationCanceledException
                    Return
                End Try
                Using reader = New StreamReader(server)
                    Dim message = Await reader.ReadLineAsync()
                    mainWindow.Dispatcher.Invoke(Sub()
                                                     mainWindow.Show()
                                                     mainWindow.WindowState = WindowState.Normal
                                                     mainWindow.Topmost = True
                                                     mainWindow.Activate()
                                                     mainWindow.Topmost = False
                                                     If message IsNot Nothing Then
                                                         mainWindow.ViewModel.SelectFolder(message)
                                                     End If
                                                 End Sub)
                End Using
            End While
        End Using
    End Function

    Public Async Function ShutdownPipeServer() As Task
        If pipeServerTask IsNot Nothing Then
            pipeServerCancellation.Cancel()
            Await pipeServerTask
        End If
    End Function

End Class
