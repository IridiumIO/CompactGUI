Imports System.IO
Imports System.IO.Pipes
Imports System.Threading
Imports System.Windows.Threading
Imports Wpf.Ui
Imports Wpf.Ui.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Configuration
Imports System.Drawing
Imports CompactGUI.Core.Settings

Partial Public Class Application

    Public Shared ReadOnly AppVersion As New SemVersion(4, 0, 0, "beta", 5)

    Private Shared _host As IHost

    Private Shared ReadOnly SettingsService As ISettingsService

    Shared Sub New()
        SettingsService = New SettingsService()
        SettingsService.LoadSettings()
        InitializeHost()

        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnDomainUnhandledException

    End Sub


    Private Shared Sub InitializeHost()

        _host = Host.CreateDefaultBuilder() _
        .ConfigureAppConfiguration(Sub(context, configBuilder)
                                       ' Set base path using IConfigurationBuilder
                                       configBuilder.SetBasePath(AppContext.BaseDirectory)
                                   End Sub) _
        .ConfigureServices(Sub(context, services)

                               services.AddHostedService(Of ApplicationHostService)()

                               'Settings handler
                               services.AddSingleton(Of ISettingsService)(SettingsService)

                               services.AddLogging(Sub(logging)
                                                       logging.SetMinimumLevel(SettingsService.AppSettings.LogLevel)
                                                       logging.AddConsole()
                                                       logging.AddDebug()
                                                       logging.AddFile(
                                                        Path.Combine(SettingsService.DataFolder.FullName, "log.log"),
                                                        SettingsService.AppSettings.LogLevel,
                                                        retainedFileCountLimit:=2,
                                                        fileSizeLimitBytes:=1000000,
                                                        outputTemplate:="{Timestamp:o} {RequestId,13} [{Level:u3}] {Message}{NewLine}{Exception}"
                                                        )
                                                   End Sub)

                               ' Theme manipulation
                               services.AddSingleton(Of IThemeService, ThemeService)()

                               ' TaskBar manipulation
                               services.AddSingleton(Of ITaskBarService, TaskBarService)()
                               ' Service containing navigation, same as INavigationWindow... but without window
                               services.AddNavigationViewPageProvider()
                               services.AddSingleton(Of INavigationService, NavigationService)()
                               services.AddSingleton(Of CustomSnackBarService)()
                               services.AddSingleton(Of IWindowService, WindowService)()
                               services.AddSingleton(Of IUpdaterService, UpdaterService)()
                               services.AddSingleton(Of IWikiService, WikiService)()

                               services.AddSingleton(Of INavigationWindow, MainWindow)()
                               services.AddSingleton(Of MainWindow)()
                               services.AddSingleton(Of MainWindowViewModel)()


                               ' Views and ViewModels
                               services.AddTransient(Of HomePage)()
                               services.AddSingleton(Of HomeViewModel)()

                               services.AddTransient(Of WatcherPage)()
                               services.AddTransient(Of WatcherViewModel)()

                               services.AddTransient(Of SettingsPage)()
                               services.AddSingleton(Of SettingsViewModel)()

                               services.AddTransient(Of DatabasePage)()
                               services.AddTransient(Of DatabaseViewModel)()

                               'Other services
                               services.AddSingleton(Of Watcher.Watcher)(Function(s)
                                                                             Return New Watcher.Watcher(Array.Empty(Of String)(), s.GetRequiredService(Of ILogger(Of Watcher.Watcher)), SettingsService)
                                                                         End Function)
                               services.AddSingleton(Of TrayNotifierService)(Function(sp)
                                                                                 Return New TrayNotifierService(sp.GetRequiredService(Of MainWindow)(), Icon.ExtractAssociatedIcon(Environment.ProcessPath), "CompactGUI")
                                                                             End Function)

                               services.AddSingleton(Of CompressableFolderService)

                           End Sub) _
            .Build()







    End Sub


    Public Shared Function GetService(Of T As Class)() As T
        Return TryCast(_host?.Services.GetService(GetType(T)), T)
    End Function


    Public Shared ReadOnly mutex As New Mutex(False, "Global\CompactGUI")
    Private pipeServerCancellation As New CancellationTokenSource()
    Private pipeServerTask As Task



    Private Shadows Async Sub OnStartup(sender As Object, e As StartupEventArgs)

        AddHandler Dispatcher.CurrentDispatcher.UnhandledException, AddressOf OnDispatcherUnhandledException
        Dim acquiredMutex As Boolean = mutex.WaitOne(0, False)

        If Not acquiredMutex Then
            If Not SettingsService.AppSettings.AllowMultiInstance Then
                HandleSecondInstance(e.Args)
                Return
            End If
        Else
            If Not SettingsService.AppSettings.AllowMultiInstance Then
                pipeServerTask = ProcessNextInstanceMessage()
            End If
        End If

        GetService(Of Watcher.Watcher)()

        Await _host.StartAsync()
        Await GetService(Of SettingsViewModel).InitializeEnvironment()

        Dim UpdateTask = GetService(Of IUpdaterService).CheckForUpdate(SettingsService.AppSettings.EnablePreReleaseUpdates)
        Dim WikiTask = GetService(Of IWikiService).GetUpdatedJSONAsync()
        Await Task.WhenAll(UpdateTask, WikiTask)

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
        Current.Shutdown()
    End Sub

    Private Async Function ProcessNextInstanceMessage() As Task
        While Not pipeServerCancellation.IsCancellationRequested
            Using server = New NamedPipeServerStream("CompactGUI", PipeDirection.In, -1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous)
                Try
                    Await server.WaitForConnectionAsync(pipeServerCancellation.Token)
                    Using reader = New StreamReader(server)
                        Dim message = Await reader.ReadLineAsync()
                        Await MainWindow.Dispatcher.InvokeAsync(Async Function()
                                                                    If message IsNot Nothing Then
                                                                        MainWindow.Show()
                                                                        MainWindow.WindowState = WindowState.Normal
                                                                        MainWindow.Activate()
                                                                        Await GetService(Of HomeViewModel).AddFoldersAsync({message})
                                                                        'Await MainWindow.ViewModel.SelectFolderAsync(message)
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


    Private Shadows Async Sub OnExit(sender As Object, e As ExitEventArgs)
        Await _host.StopAsync()
        _host.Dispose()
    End Sub

    Private Sub OnDispatcherUnhandledException(sender As Object, e As DispatcherUnhandledExceptionEventArgs)
        GetService(Of ILogger(Of Application))().LogCritical(e.Exception, "Unhandled exception in application: {Message}", e.Exception.Message)
    End Sub

    Private Shared Sub OnDomainUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex = TryCast(e.ExceptionObject, Exception)
        Dim logger = GetService(Of ILogger(Of Application))()
        If logger IsNot Nothing AndAlso ex IsNot Nothing Then
            logger.LogCritical(ex, "Unhandled domain exception: {Message}", ex.Message)
        End If
    End Sub

























    'Public Shared ReadOnly mutex As New Mutex(False, "Global\CompactGUI")
    'Private pipeServerCancellation As New CancellationTokenSource()
    'Private pipeServerTask As Task
    'Private Shadows mainWindow As MainWindow

    'Private Async Sub Application_Startup(sender As Object, e As StartupEventArgs)
    '    SettingsHandler.InitialiseSettings()
    '    Dim acquiredMutex As Boolean = mutex.WaitOne(0, False)

    '    If Not acquiredMutex Then
    '        If Not SettingsHandler.AppSettings.AllowMultiInstance Then
    '            HandleSecondInstance(e.Args)
    '            Return
    '        End If
    '    Else
    '        If Not SettingsHandler.AppSettings.AllowMultiInstance Then
    '            pipeServerTask = ProcessNextInstanceMessage()
    '        End If
    '    End If

    '    mainWindow = New MainWindow()
    '    Dim shouldMinimizeToTray As Boolean = (e.Args.Length = 1 AndAlso e.Args(0).ToString = "-tray") OrElse
    '                                      (SettingsHandler.AppSettings.StartInSystemTray AndAlso e.Args.Length = 0)

    '    If shouldMinimizeToTray Then
    '        mainWindow.Show()
    '        mainWindow.ViewModel.ClosingCommand.Execute(New ComponentModel.CancelEventArgs(True))
    '    Else
    '        If e.Args.Length = 1 Then
    '            Await mainWindow.ViewModel.SelectFolderAsync(e.Args(0))
    '        End If
    '        mainWindow.Show()
    '    End If

    '    Await SettingsViewModel.InitializeEnvironment()
    'End Sub

    'Private Sub HandleSecondInstance(args As String())
    '    If args.Length > 0 AndAlso args(0) <> "-tray" Then
    '        Using client = New NamedPipeClientStream(".", "CompactGUI", PipeDirection.Out)
    '            client.Connect()
    '            Using writer = New StreamWriter(client)
    '                writer.WriteLine(args(0))
    '            End Using
    '        End Using
    '    Else
    '        MessageBox.Show("An instance of CompactGUI is already running")
    '    End If
    '    Application.Current.Shutdown()
    'End Sub

    'Private Async Function ProcessNextInstanceMessage() As Task
    '    While Not pipeServerCancellation.IsCancellationRequested
    '        Using server = New NamedPipeServerStream("CompactGUI", PipeDirection.In, -1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous)
    '            Try
    '                Await server.WaitForConnectionAsync(pipeServerCancellation.Token)
    '                Using reader = New StreamReader(server)
    '                    Dim message = Await reader.ReadLineAsync()
    '                    Await mainWindow.Dispatcher.InvokeAsync(Async Function()
    '                                                                If message IsNot Nothing Then
    '                                                                    mainWindow.Show()
    '                                                                    mainWindow.WindowState = WindowState.Normal
    '                                                                    mainWindow.Activate()
    '                                                                    Await mainWindow.ViewModel.SelectFolderAsync(message)
    '                                                                End If
    '                                                            End Function).Task
    '                End Using
    '            Catch ex As OperationCanceledException
    '                Return
    '            Finally
    '                If server.IsConnected Then server.Disconnect()
    '            End Try
    '        End Using
    '    End While
    'End Function

    'Public Async Function ShutdownPipeServer() As Task
    '    If pipeServerTask IsNot Nothing Then
    '        pipeServerCancellation.Cancel()
    '        Await pipeServerTask
    '    End If
    'End Function

End Class