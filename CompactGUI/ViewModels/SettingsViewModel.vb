
Imports System.ComponentModel

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Imports CompactGUI.Logging

Imports Microsoft.Extensions.Logging

Public Class SettingsViewModel : Inherits ObservableObject

    Private ReadOnly logger As ILogger(Of Settings)

    Public Property AppSettings As Settings = SettingsHandler.AppSettings

    Public Sub New(logger As ILogger(Of Settings))

        AddHandler AppSettings.PropertyChanged, AddressOf SettingsPropertyChanged
        Me.logger = logger
    End Sub

    Public Shared Async Function InitializeEnvironment() As Task

        Await SetEnv()
        Await ApplyContextIntegrationAsync()
        ApplyStartMenuIntegration()

    End Function

    Private Shared Async Function SetEnv() As Task
        SettingsLog.SettingEnvironmentVariables(Application.GetService(Of ILogger(Of Settings)))
        Dim desiredValue = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO")
        Dim currentValue = Environment.GetEnvironmentVariable("IridiumIO", EnvironmentVariableTarget.User)
        If currentValue <> desiredValue Then Await Task.Run(Sub() Environment.SetEnvironmentVariable("IridiumIO", desiredValue, EnvironmentVariableTarget.User))

    End Function

    Private Async Sub SettingsPropertyChanged(sender As Object, e As PropertyChangedEventArgs)

        If e.PropertyName = NameOf(Settings.IsContextIntegrated) Then
            Await ApplyContextIntegrationAsync()
        End If

        If e.PropertyName = NameOf(Settings.IsStartMenuEnabled) Then
            ApplyStartMenuIntegration()
        End If

        Settings.Save()
    End Sub

    Public Shared Async Function ApplyContextIntegrationAsync() As Task
        If SettingsHandler.AppSettings.IsContextIntegrated Then
            Await Settings.AddContextMenus()
        Else
            Await Settings.RemoveContextMenus()
        End If
    End Function

    Public Shared Sub ApplyStartMenuIntegration()
        If SettingsHandler.AppSettings.IsStartMenuEnabled Then
            Settings.CreateStartMenuShortcut()
        Else
            Settings.DeleteStartMenuShortcut()
        End If
    End Sub



    Public Property EditSkipListCommand As ICommand = New RelayCommand(Function() (New Settings_skiplistflyout).ShowDialog())


    Public Property DisableAutoCompressionCommand As ICommand = New RelayCommand(Sub() AppSettings.EnableBackgroundAutoCompression = False)
    Public Property EnableBackgroundWatcherCommand As ICommand = New RelayCommand(Sub() AppSettings.EnableBackgroundWatcher = True)
    Public Property OpenGitHubCommand As ICommand = New RelayCommand(Sub() Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI") With {.UseShellExecute = True}))
    Public Property OpenKoFiCommand As ICommand = New RelayCommand(Sub() Process.Start(New ProcessStartInfo("https://ko-fi.com/IridiumIO") With {.UseShellExecute = True}))

End Class
