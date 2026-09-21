
Imports System.ComponentModel

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging
Imports CommunityToolkit.Mvvm.Messaging.Messages

Imports CompactGUI.Core.Settings


Partial Public Class MainWindowViewModel : Inherits ObservableRecipient : Implements IRecipient(Of PropertyChangedMessage(Of CompressableFolder))

    <ObservableProperty>
    Private _BackgroundImage As BitmapImage

    <ObservableProperty>
    Private _IsQueueSidebarOpen As Boolean

    Private ReadOnly _watcher As Watcher.Watcher
    Private ReadOnly _windowService As IWindowService
    Private ReadOnly _settingsService As ISettingsService

    Public ReadOnly Property HideSteamLibrary As Boolean
        Get
            Return _settingsService.AppSettings.HideSteamLibraryTab
        End Get
    End Property

    Public ReadOnly Property HideCompressionDbTab As Boolean
        Get
            Return _settingsService.AppSettings.HideCompressionDbTab
        End Get
    End Property

    Public Sub New(windowService As IWindowService, watcher As Watcher.Watcher, settingsService As ISettingsService)
        _watcher = watcher
        _windowService = windowService
        _settingsService = settingsService

        AddHandler _settingsService.AppSettings.PropertyChanged, AddressOf AppSettings_PropertyChanged

    End Sub

    Private Sub AppSettings_PropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        If e.PropertyName = NameOf(_settingsService.AppSettings.HideSteamLibraryTab) Then
            OnPropertyChanged(NameOf(HideSteamLibrary))
        ElseIf e.PropertyName = NameOf(_settingsService.AppSettings.HideCompressionDbTab) Then
            OnPropertyChanged(NameOf(HideCompressionDbTab))
        End If
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
    Private Sub ToggleQueueSidebar()
        IsQueueSidebarOpen = Not IsQueueSidebarOpen
    End Sub


    <RelayCommand>
    Private Async Function NotifyIconExit() As Task
        If _watcher.WatchedFolders.Count = 0 Then Application.Current.Shutdown()
        Dim message = "You currently have {0} folders being watched. Closing CompactGUI will stop them from being monitored.{1}{1}Are you sure you want to exit?".LTF(_watcher.WatchedFolders.Count, vbCrLf)
        Dim confirmed = Await _windowService.ShowMessageBox("CompactGUI".LT(), message)
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
            _settingsService.SaveSettings()
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
