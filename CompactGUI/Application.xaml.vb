Imports System.IO
Imports System.IO.Pipes
Imports System.Runtime.InteropServices
Imports System.Threading

Class Application

    Public Shared ReadOnly mutex As New Mutex(False, "Global\CompactGUI")

    Private mainWindow As MainWindow

    Private Async Sub Application_Startup(sender As Object, e As StartupEventArgs)

        If Not mutex.WaitOne(0, False) Then

            If e.Args.Length <> 0 AndAlso e.Args(0) = "-tray" Then
                MessageBox.Show("An instance of CompactGUI is already running")
                Application.Current.Shutdown()

            End If

            Using client = New NamedPipeClientStream("CompactGUI")
                client.Connect()
                Using writer = New StreamWriter(client)
                    writer.WriteLine(e.Args(0))
                End Using
            End Using

            Application.Current.Shutdown()
        Else

            ProcessNextInstanceMessage()
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


    Private Async Sub ProcessNextInstanceMessage()

        Await Task.Run(Sub()
                           While True

                               Using server = New NamedPipeServerStream("CompactGUI")
                                   server.WaitForConnection()
                                   Using reader = New StreamReader(server)
                                       Dim message = reader.ReadLine()
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
                               End Using
                           End While
                       End Sub)
    End Sub

End Class
