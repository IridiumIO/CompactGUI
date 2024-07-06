Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports Microsoft.Toolkit.Mvvm.Input
Imports Microsoft.Win32

Public Class SettingsViewModel : Inherits ObservableObject


    Public Property AppSettings As Settings = SettingsHandler.AppSettings

    Public Sub New()
        AddExecutableToRegistry()
        AddHandler AppSettings.PropertyChanged, AddressOf SettingsPropertyChanged
        SetEnv()
    End Sub


    Private Async Sub SetEnv()
        Await Task.Run(Sub() Environment.SetEnvironmentVariable("IridiumIO", IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO"), EnvironmentVariableTarget.User))
    End Sub

    Private Sub SettingsPropertyChanged()
        SettingsHandler.AppSettings.Save()
    End Sub


    Sub AddExecutableToRegistry()
        Registry.SetValue("HKEY_CURRENT_USER\software\IridiumIO\CompactGUI\", "Executable Path", IO.Directory.GetCurrentDirectory)
    End Sub

    Private Sub AddToContextMenu()

        Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "", "Compress Folder")
        Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "Icon", Environment.ProcessPath)
        Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI\command", "", Environment.ProcessPath + " " + """%1""")

        SettingsHandler.AppSettings.IsContextIntegrated = True
        SettingsHandler.AppSettings.Save()

    End Sub


    Private Sub RemoveFromContextMenu()
        Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI\command")

        Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI")

        SettingsHandler.AppSettings.IsContextIntegrated = False
        SettingsHandler.AppSettings.Save()
    End Sub


    Public Property EditSkipListCommand As ICommand = New RelayCommand(Sub()
                                                                           Dim fl As New Settings_skiplistflyout
                                                                           fl.ShowDialog()
                                                                       End Sub)

    Public Property AddToContextMenuCommand As ICommand = New RelayCommand(AddressOf AddToContextMenu)
    Public Property RemoveFromContextMenuCommand As ICommand = New RelayCommand(AddressOf RemoveFromContextMenu)
    Public Property UIScalingSliderCommand As ICommand = New RelayCommand(Of Double)(Sub(val) SettingsHandler.AppSettings.WindowScalingFactor = val)
    Public Property DisableAutoCompressionCommand As ICommand = New RelayCommand(Sub() AppSettings.EnableBackgroundAutoCompression = False)
    Public Property EnableBackgroundWatcherCommand As ICommand = New RelayCommand(Sub() AppSettings.EnableBackgroundWatcher = True)
    Public Property OpenGitHubCommand As ICommand = New RelayCommand(Sub() Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI") With {.UseShellExecute = True}))
    Public Property OpenKoFiCommand As ICommand = New RelayCommand(Sub() Process.Start(New ProcessStartInfo("https://ko-fi.com/IridiumIO") With {.UseShellExecute = True}))

End Class
