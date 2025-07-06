
Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging
Imports CommunityToolkit.Mvvm.Messaging.Messages


Partial Public Class MainWindowViewModel : Inherits ObservableRecipient : Implements IRecipient(Of PropertyChangedMessage(Of CompressableFolder))

    <ObservableProperty>
    Private _BackgroundImage As BitmapImage

    Private ReadOnly _watcher As Watcher.Watcher
    Private ReadOnly _windowService As IWindowService

    Public Sub New(windowService As IWindowService, watcher As Watcher.Watcher)
        _watcher = watcher
        _windowService = windowService
    End Sub

    Public ReadOnly Property IsAdmin As Boolean
        Get
            Dim principal = New Security.Principal.WindowsPrincipal(Security.Principal.WindowsIdentity.GetCurrent())
            Return principal.IsInRole(Security.Principal.WindowsBuiltInRole.Administrator)
        End Get
    End Property


    <RelayCommand>
    Private Sub NotifyIconOpen()
        _windowService.ShowMainWindow()
    End Sub


    <RelayCommand>
    Private Async Function NotifyIconExit() As Task
        If _watcher.WatchedFolders.Count = 0 Then Application.Current.Shutdown()
        Dim confirmed = Await _windowService.ShowMessageBox("CompactGUI", $"You currently have {_watcher.WatchedFolders.Count} folders being watched. Closing CompactGUI will stop them from being monitored.{Environment.NewLine}{Environment.NewLine}Are you sure you want to exit?")
        If Not confirmed Then Return
        _watcher.WriteToFile()
        Application.Current.Shutdown()
    End Function


    <RelayCommand>
    Private Sub Closing(e As ComponentModel.CancelEventArgs)
        If e Is Nothing Then Return

        If Keyboard.Modifiers = ModifierKeys.Shift Then
            e.Cancel = False
            If _watcher.WatchedFolders.Count <> 0 Then _watcher.WriteToFile()
            SettingsHandler.WriteToFile()
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


    Public Sub Receive(message As PropertyChangedMessage(Of CompressableFolder)) Implements IRecipient(Of PropertyChangedMessage(Of CompressableFolder)).Receive

        If message.Sender.GetType() IsNot GetType(HomeViewModel) Then Return
        If message.PropertyName <> NameOf(HomeViewModel.SelectedFolder) Then Return
        BackgroundImage = message.NewValue?.FolderBGImage

    End Sub
End Class
