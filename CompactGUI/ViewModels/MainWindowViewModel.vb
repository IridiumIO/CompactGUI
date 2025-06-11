
Imports System.ComponentModel

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging


Public Class MainWindowViewModel : Inherits ObservableObject : Implements IRecipient(Of BackgroundImageChangedMessage)

    Private ReadOnly _watcher As Watcher.Watcher
    Private ReadOnly _windowService As IWindowService

    Public Sub New(windowService As IWindowService, watcher As Watcher.Watcher)
        _watcher = watcher
        _windowService = windowService
        WeakReferenceMessenger.Default.Register(Of BackgroundImageChangedMessage)(Me)
    End Sub


    Public ReadOnly Property NotifyIconOpenCommand As IRelayCommand = New RelayCommand(Sub() _windowService.ShowMainWindow())
    Public ReadOnly Property NotifyIconExitCommand As IRelayCommand = New RelayCommand(Sub() NotifyExit())


    Private Async Function NotifyExit() As Task
        If _watcher.WatchedFolders.Count = 0 Then Application.Current.Shutdown()
        Dim confirmed = Await _windowService.ShowMessageBox("CompactGUI", $"You currently have {_watcher.WatchedFolders.Count} folders being watched. Closing CompactGUI will stop them from being monitored.{Environment.NewLine}{Environment.NewLine}Are you sure you want to exit?")
        If Not confirmed Then Return
        _watcher.WriteToFile()
        Application.Current.Shutdown()
    End Function

    Public ReadOnly Property ClosingCommand As IRelayCommand = New RelayCommand(Of CancelEventArgs)(AddressOf Closing)


    Private Sub Closing(e As CancelEventArgs)
        If e Is Nothing Then Return

        If Keyboard.Modifiers = ModifierKeys.Shift Then
            e.Cancel = False
            If _watcher.WatchedFolders.Count <> 0 Then _watcher.WriteToFile()
            Application.Current.Shutdown()
            Return
        End If

        If _watcher.WatchedFolders.Count <> 0 Then
            e.Cancel = True
            _windowService.MinimizeMainWindow()
            _watcher.WriteToFile()
            _windowService.HideMainWindow()
        End If

    End Sub



    Public Property BackgroundImage As BitmapImage
    Public ReadOnly Property IsAdmin As Boolean
        Get
            Dim principal = New Security.Principal.WindowsPrincipal(Security.Principal.WindowsIdentity.GetCurrent())
            Return principal.IsInRole(Security.Principal.WindowsBuiltInRole.Administrator)
        End Get
    End Property


    Public Sub Receive(message As BackgroundImageChangedMessage) Implements IRecipient(Of BackgroundImageChangedMessage).Receive
        BackgroundImage = message.Value
    End Sub
End Class
