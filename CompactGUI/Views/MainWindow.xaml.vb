﻿Imports Ookii.Dialogs.Wpf
Imports MethodTimer
Imports System.Windows.Media.Animation
Imports ModernWpf.Controls
Imports CompactGUI.Core

Public Class MainWindow
    Public Sub New()

        InitializeComponent()

        Me.DataContext = ViewModel

        ViewModel.State = "FreshLaunch"

    End Sub

    Public Property ViewModel As New MainViewModel

    Public Property activeFolder As ActiveFolder

    Private Sub SearchClicked(sender As Object, e As MouseButtonEventArgs)
        ViewModel.SelectFolder()
    End Sub

    Private Sub uiUpdateBanner_MouseUp(sender As Object, e As MouseButtonEventArgs)
        Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI/releases/") With {.UseShellExecute = True})
    End Sub

    Private Sub uiBtnOptions_Click(sender As Object, e As RoutedEventArgs) Handles uiBtnOptions.Click
        Dim settingsDialog As New ContentDialog With {
            .Content = New SettingsControl,
            .PrimaryButtonText = "save and close"
        }
        settingsDialog.ShowAsync()

    End Sub

    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs)

        If e.Key = Key.System Then e.Handled = True

    End Sub
End Class
