﻿Imports System.ComponentModel
Imports Microsoft.Toolkit.Mvvm.Input

Partial Public Class MainViewModel

    Public Property ClosingCommand As RelayCommand(Of CancelEventArgs)
    Public Property NotifyCommand As ICommand
    Public Property NotifyIconOpenCommand As ICommand
    Public Property NotifyIconExitCommand As ICommand

    Private _windowState As WindowState
    Public Property WindowState As WindowState
        Get
            Return _windowState
        End Get
        Set(value As WindowState)
            ShowInTaskbar = True
            SetProperty(_windowState, value)
            ShowInTaskbar = value <> WindowState.Minimized
        End Set
    End Property
    Public Property ShowInTaskbar As Boolean

    Public Property NotifyRequest As NotifyIconWrapper.NotifyRequestRecord

    Private Sub InitialiseNotificationTray()
        ClosingCommand = New RelayCommand(Of CancelEventArgs)(AddressOf Closing)
        NotifyCommand = New RelayCommand(Sub() Notify("Hello", "World"))
        NotifyIconOpenCommand = New RelayCommand(Sub() WindowState = WindowState.Normal)
        NotifyIconExitCommand = New RelayCommand(AddressOf NotifyExit)
    End Sub

    Private Sub NotifyExit()
        If Watcher.WatchedFolders.Count = 0 Then Application.Current.Shutdown()
        Dim res = MessageBox.Show(
            $"You currently have {Watcher.WatchedFolders.Count} folders being watched. Closing CompactGUI will stop them from being monitored.{Environment.NewLine}{Environment.NewLine}Are you sure you want to exit?",
"CompactGUI", MessageBoxButton.YesNo)
        If res = MessageBoxResult.Yes Then Application.Current.Shutdown()

    End Sub

    Private Sub Notify(title As String, message As String)

        If Not SettingsHandler.AppSettings.ShowNotifications Then Return

        NotifyRequest = New NotifyIconWrapper.NotifyRequestRecord With {
            .Title = title,
            .Text = message,
            .Duration = 1000}
    End Sub

    Private Sub Notify_Compressed(DisplayName As String, BytesSaved As Long, CompressionRatio As Decimal)

        Dim title = $"{DisplayName}"
        Dim readableSaved = $"{New BytesToReadableConverter().Convert(ActiveFolder.UncompressedBytes - ActiveFolder.CompressedBytes, GetType(Long), Nothing, Globalization.CultureInfo.CurrentCulture)} saved"
        Dim percentCompressed = $"{100 - CInt(CompressionRatio * 100)}% smaller"
        Notify(title, $"▸ {readableSaved}{Environment.NewLine}▸ {percentCompressed}")

    End Sub

    Private Sub Closing(e As CancelEventArgs)
        If e Is Nothing Then Return
        If Watcher.WatchedFolders.Count <> 0 Then
            e.Cancel = True
            WindowState = WindowState.Minimized
        End If

    End Sub


End Class
