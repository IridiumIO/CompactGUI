Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports Microsoft.Toolkit.Mvvm.Input
Imports Microsoft.Win32

Public Class SettingsViewModel : Inherits ObservableObject


    Public Property AppSettings As Settings = SettingsHandler.AppSettings

    Public Sub New()

        AddHandler AppSettings.PropertyChanged, AddressOf SettingsPropertyChanged

    End Sub

    Public Shared Async Function InitializeEnvironment() As Task

        Await AddExecutableToRegistry()
        Await SetEnv()
        If SettingsHandler.AppSettings.IsContextIntegrated Then
            Await Settings.AddContextMenus
        Else
            Await Settings.RemoveContextMenus
        End If

        If SettingsHandler.AppSettings.IsStartMenuEnabled Then
            Settings.CreateStartMenuShortcut()
        Else
            Settings.DeleteStartMenuShortcut()
        End If

    End Function

    Private Shared Async Function SetEnv() As Task
        Await Task.Run(Sub() Environment.SetEnvironmentVariable("IridiumIO", IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO"), EnvironmentVariableTarget.User))
    End Function

    Private Sub SettingsPropertyChanged()
        Settings.Save()
    End Sub


    Private Shared Async Function AddExecutableToRegistry() As Task
        Await Task.Run(Sub() Registry.SetValue("HKEY_CURRENT_USER\software\IridiumIO\CompactGUI\", "Executable Path", IO.Directory.GetCurrentDirectory))
    End Function


    Public Property EditSkipListCommand As ICommand = New RelayCommand(Sub()
                                                                           Dim fl As New Settings_skiplistflyout
                                                                           fl.ShowDialog()
                                                                       End Sub)


    Public Property DisableAutoCompressionCommand As ICommand = New RelayCommand(Sub() AppSettings.EnableBackgroundAutoCompression = False)
    Public Property EnableBackgroundWatcherCommand As ICommand = New RelayCommand(Sub() AppSettings.EnableBackgroundWatcher = True)
    Public Property OpenGitHubCommand As ICommand = New RelayCommand(Sub() Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI") With {.UseShellExecute = True}))
    Public Property OpenKoFiCommand As ICommand = New RelayCommand(Sub() Process.Start(New ProcessStartInfo("https://ko-fi.com/IridiumIO") With {.UseShellExecute = True}))

End Class
