
Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Public Class SettingsViewModel : Inherits ObservableObject


    Public Property AppSettings As Settings = SettingsHandler.AppSettings

    Public Sub New()

        AddHandler AppSettings.PropertyChanged, AddressOf SettingsPropertyChanged

    End Sub

    Public Shared Async Function InitializeEnvironment() As Task

        ' Await AddExecutableToRegistry()
        Await SetEnv()
        Await If(SettingsHandler.AppSettings.IsContextIntegrated, Settings.AddContextMenus, Settings.RemoveContextMenus)

        If SettingsHandler.AppSettings.IsStartMenuEnabled Then
            Settings.CreateStartMenuShortcut()
        Else
            Settings.DeleteStartMenuShortcut()
        End If

    End Function

    Private Shared Async Function SetEnv() As Task
        Dim desiredValue = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO")
        Dim currentValue = Environment.GetEnvironmentVariable("IridiumIO", EnvironmentVariableTarget.User)
        If currentValue <> desiredValue Then Await Task.Run(Sub() Environment.SetEnvironmentVariable("IridiumIO", desiredValue, EnvironmentVariableTarget.User))

    End Function

    Private Sub SettingsPropertyChanged()
        Settings.Save()
    End Sub


    'Private Shared Async Function AddExecutableToRegistry() As Task
    '    Await Task.Run(Sub() Registry.SetValue("HKEY_CURRENT_USER\software\IridiumIO\CompactGUI\", "Executable Path", IO.Directory.GetCurrentDirectory))
    'End Function


    Public Property EditSkipListCommand As ICommand = New RelayCommand(Sub()
                                                                           Dim fl As New Settings_skiplistflyout
                                                                           fl.ShowDialog()
                                                                       End Sub)


    Public Property DisableAutoCompressionCommand As ICommand = New RelayCommand(Sub() AppSettings.EnableBackgroundAutoCompression = False)
    Public Property EnableBackgroundWatcherCommand As ICommand = New RelayCommand(Sub() AppSettings.EnableBackgroundWatcher = True)
    Public Property OpenGitHubCommand As ICommand = New RelayCommand(Sub() Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI") With {.UseShellExecute = True}))
    Public Property OpenKoFiCommand As ICommand = New RelayCommand(Sub() Process.Start(New ProcessStartInfo("https://ko-fi.com/IridiumIO") With {.UseShellExecute = True}))

End Class
