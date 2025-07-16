Imports System.ComponentModel

Imports CompactGUI.Core.Settings

Imports Wpf.Ui
Imports Wpf.Ui.Abstractions
Imports Wpf.Ui.Controls

Class MainWindow : Implements INavigationWindow, INotifyPropertyChanged

    Private ReadOnly _NavigationService As INavigationService
    Private _MainWindowViewModel As MainWindowViewModel
    Private _SettingsService As ISettingsService
    Public Sub New(settingsService As ISettingsService, navigationService As INavigationService, serviceProvider As IServiceProvider, snackbarService As CustomSnackBarService, viewmodel As MainWindowViewModel)

        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        snackbarService.SetSnackbarPresenter(RootSnackbar)
        navigationService.SetNavigationControl(NavigationView)
        NavigationView.SetServiceProvider(serviceProvider)

        _NavigationService = navigationService
        _MainWindowViewModel = viewmodel
        _SettingsService = settingsService
        DataContext = viewmodel

        NotifyIconTrayMenu.DataContext = viewmodel

        AddHandler Application.GetService(Of HomeViewModel)().PropertyChanged, AddressOf HVPropertyChanged
        AddHandler navigationService.GetNavigationControl.Navigated, AddressOf OnNavigated

        If _SettingsService.AppSettings.WindowWidth > 0 Then
            Width = _SettingsService.AppSettings.WindowWidth
            Height = _SettingsService.AppSettings.WindowHeight
            Left = _SettingsService.AppSettings.WindowLeft
            Top = _SettingsService.AppSettings.WindowTop
            If _SettingsService.AppSettings.WindowState = Core.Settings.WindowState.Maximized Then
                WindowState = Core.Settings.WindowState.Maximized
            Else
                WindowState = Core.Settings.WindowState.Normal
            End If
        End If


    End Sub


    Private _isOnHomePage As Boolean

    Private Sub OnNavigated(sender As NavigationView, args As NavigatedEventArgs)
        If args.Page.GetType Is GetType(HomePage) Then
            _isOnHomePage = True
            _MainWindowViewModel.IsActive = True
            HVPropertyChanged(Application.GetService(Of HomeViewModel)(), New PropertyChangedEventArgs(NameOf(HomeViewModel.HomeViewIsFresh)))
        Else
            _isOnHomePage = False
            _MainWindowViewModel.IsActive = False
            ProgTitle.Visibility = Visibility.Visible
        End If
    End Sub

    Private Sub HVPropertyChanged(sender As Object, e As PropertyChangedEventArgs)

        Dim homeVM As HomeViewModel = CType(sender, HomeViewModel)


        If _isOnHomePage AndAlso e.PropertyName = NameOf(homeVM.HomeViewIsFresh) Then
            If homeVM.HomeViewIsFresh Then
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

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs)
        If Not IsVisible Then Return
        _SettingsService.AppSettings.WindowState = WindowState
        _SettingsService.AppSettings.WindowWidth = If(Width > 0, Width, 1300)
        _SettingsService.AppSettings.WindowHeight = If(Height > 0, Height, 700)
        _SettingsService.AppSettings.WindowLeft = Left
        _SettingsService.AppSettings.WindowTop = Top
        _SettingsService.SaveSettings()
    End Sub
End Class
