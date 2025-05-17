Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows

Public Class ApplicationHostService
    Implements IHostedService

    Private ReadOnly _serviceProvider As IServiceProvider

    Public Sub New(serviceProvider As IServiceProvider)
        _serviceProvider = serviceProvider
    End Sub

    ''' <summary>
    ''' Triggered when the application host is ready to start the service.
    ''' </summary>
    ''' <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    Public Async Function StartAsync(cancellationToken As CancellationToken) As Task Implements IHostedService.StartAsync
        Await HandleActivationAsync()
    End Function

    ''' <summary>
    ''' Triggered when the application host is performing a graceful shutdown.
    ''' </summary>
    ''' <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    Public Async Function StopAsync(cancellationToken As CancellationToken) As Task Implements IHostedService.StopAsync
        Await Task.CompletedTask
    End Function

    ''' <summary>
    ''' Creates main window during activation.
    ''' </summary>
    Private Async Function HandleActivationAsync() As Task
        Await Task.CompletedTask


        If Not Application.Current.Windows.OfType(Of MainWindow)().Any() Then
            Dim navigationWindow = _serviceProvider.GetRequiredService(Of MainWindow)()
            AddHandler navigationWindow.Loaded, AddressOf OnNavigationWindowLoaded
            navigationWindow.Show()
        End If
    End Function

    Private Sub OnNavigationWindowLoaded(sender As Object, e As RoutedEventArgs)
        If TypeOf sender IsNot MainWindow Then
            Return
        End If

        Dim navigationWindow = DirectCast(sender, MainWindow)
        navigationWindow.NavigationView.Navigate(GetType(HomePage))
    End Sub
End Class