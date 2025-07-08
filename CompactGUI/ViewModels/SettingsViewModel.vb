
Imports System.ComponentModel
Imports System.Xml

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Imports CompactGUI.Core.Settings
Imports CompactGUI.Logging

Imports Microsoft.Extensions.Logging

Public NotInheritable Class SettingsViewModel : Inherits ObservableObject

    Private ReadOnly _logger As ILogger(Of Settings)
    Private ReadOnly _settingsService As ISettingsService

    Public ReadOnly Property AppSettings As Settings


    Public Sub New(settingsService As ISettingsService, logger As ILogger(Of Settings))

        Me._logger = logger
        _settingsService = settingsService
        AppSettings = settingsService.AppSettings
        AddHandler AppSettings.PropertyChanged, AddressOf SettingsPropertyChanged
    End Sub

    Public Async Function InitializeEnvironment() As Task

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

        Application.GetService(Of ISettingsService).SaveSettings()
    End Sub

    Public Async Function ApplyContextIntegrationAsync() As Task
        If _settingsService.AppSettings.IsContextIntegrated Then
            Await AddContextMenus()
        Else
            Await RemoveContextMenus()
        End If
    End Function

    Public Sub ApplyStartMenuIntegration()
        If _settingsService.AppSettings.IsStartMenuEnabled Then
            CreateStartMenuShortcut()
        Else
            DeleteStartMenuShortcut()
        End If
    End Sub



    Public Shared Sub CreateStartMenuShortcut()
        SettingsLog.AddingStartMenuShortcut(Application.GetService(Of ILogger(Of Settings)))
        Dim startMenuPath As String = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)
        Dim shortcutPath As String = IO.Path.Combine(startMenuPath, "CompactGUI.lnk")
        Dim exePath As String = Environment.ProcessPath
        CreateShortcut(shortcutPath, exePath, "CompactGUI", IO.Path.GetDirectoryName(exePath), exePath)

    End Sub

    Public Shared Sub DeleteStartMenuShortcut()
        SettingsLog.RemovingStartMenuShortcut(Application.GetService(Of ILogger(Of Settings)))
        Dim startMenuPath As String = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)
        Dim shortcutPath As String = IO.Path.Combine(startMenuPath, "CompactGUI.lnk")

        If IO.File.Exists(shortcutPath) Then
            IO.File.Delete(shortcutPath)
        End If
    End Sub



    Public Shared Async Function AddContextMenus() As Task
        SettingsLog.AddingToContextMenus(Application.GetService(Of ILogger(Of Settings)))
        Await Task.Run(Sub()
                           Try
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "", "Compress Folder")
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "Icon", Environment.ProcessPath)
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI\command", "", Environment.ProcessPath & " " & """%1""")
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\Background\shell\CompactGUI", "", "Compress Folder")
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\Background\shell\CompactGUI", "Icon", Environment.ProcessPath)
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\Background\shell\CompactGUI\command", "", Environment.ProcessPath & " " & """%V""")
                               SettingsLog.AddingToContextMenusSuccess(Application.GetService(Of ILogger(Of Settings)))
                           Catch ex As Exception
                               SettingsLog.AddingToContextMenusFailed(Application.GetService(Of ILogger(Of Settings)), ex)
                           End Try
                       End Sub)
    End Function

    Public Shared Async Function RemoveContextMenus() As Task
        SettingsLog.RemovingFromContextMenus(Application.GetService(Of ILogger(Of Settings)))
        Await Task.Run(Sub()
                           Try
                               Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI\command")
                               Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI")
                               Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\Background\\shell\\CompactGUI\command")
                               Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\Background\\shell\\CompactGUI")
                               SettingsLog.RemovingFromContextMenusSuccess(Application.GetService(Of ILogger(Of Settings)))
                           Catch ex As Exception
                               SettingsLog.RemovingFromContextMenusFailed(Application.GetService(Of ILogger(Of Settings)), ex)
                           End Try
                       End Sub)
    End Function


    Public Property EditSkipListCommand As ICommand = New RelayCommand(Function() (New Settings_skiplistflyout).ShowDialog())


    Public Property DisableAutoCompressionCommand As ICommand = New RelayCommand(Sub() AppSettings.EnableBackgroundAutoCompression = False)
    Public Property EnableBackgroundWatcherCommand As ICommand = New RelayCommand(Sub() AppSettings.EnableBackgroundWatcher = True)
    Public Property OpenGitHubCommand As ICommand = New RelayCommand(Sub() Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI") With {.UseShellExecute = True}))
    Public Property OpenKoFiCommand As ICommand = New RelayCommand(Sub() Process.Start(New ProcessStartInfo("https://ko-fi.com/IridiumIO") With {.UseShellExecute = True}))

End Class
