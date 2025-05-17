Imports System.ComponentModel

Imports CommunityToolkit.Mvvm.ComponentModel

Imports Wpf.Ui
Imports Wpf.Ui.Abstractions
Imports Wpf.Ui.Controls

Class MainWindow : Implements INavigationWindow, INotifyPropertyChanged
    Public Sub New(navigationService As INavigationService, serviceProvider As IServiceProvider, snackbarService As CustomSnackBarService)

        ' This call is required by the designer.
        InitializeComponent()

        snackbarService.SetSnackbarPresenter(RootSnackbar)
        navigationService.SetNavigationControl(NavigationView)
        NavigationView.SetServiceProvider(serviceProvider)
        ' Add any initialization after the InitializeComponent() call.

        AddHandler Application.GetService(Of HomeViewModel)().PropertyChanged, AddressOf HVPropertyChanged

    End Sub

    Private Sub HVPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        Dim homeVM As HomeViewModel = CType(sender, HomeViewModel)
        If e.PropertyName = NameOf(homeVM.HomeViewIsFresh) Then
            If homeVM.HomeViewIsFresh Then '
                ProgTitle.Visibility = Visibility.Collapsed
            Else
                ProgTitle.Visibility = Visibility.Visible
            End If
        End If
    End Sub

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Public Sub SetServiceProvider(serviceProvider As IServiceProvider) Implements INavigationWindow.SetServiceProvider
        Throw New NotImplementedException()
    End Sub

    Public Sub SetPageService(navigationViewPageProvider As INavigationViewPageProvider) Implements INavigationWindow.SetPageService
        Throw New NotImplementedException()
    End Sub

    Public Sub ShowWindow() Implements INavigationWindow.ShowWindow
        Throw New NotImplementedException()
    End Sub

    Public Sub CloseWindow() Implements INavigationWindow.CloseWindow
        Throw New NotImplementedException()
    End Sub

    Public Function GetNavigation() As INavigationView Implements INavigationWindow.GetNavigation
        Throw New NotImplementedException()
    End Function

    Public Function Navigate(pageType As Type) As Boolean Implements INavigationWindow.Navigate
        Throw New NotImplementedException()
    End Function
End Class
