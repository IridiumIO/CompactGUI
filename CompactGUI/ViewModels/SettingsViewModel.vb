
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
    
    ' New commands for advanced features
    Public Property SelectDefaultProfileCommand As ICommand = New RelayCommand(AddressOf SelectDefaultProfile)
    
    Private Sub SelectDefaultProfile()
        Dim profileService = Application.GetService(Of ProfileService)()
        Dim profiles = profileService.Profiles
        
        ' Create a simple dialog to select a profile
        Dim dialog = New Window With {
            .Title = "Select Default Profile",
            .Width = 400,
            .Height = 300,
            .WindowStartupLocation = WindowStartupLocation.CenterOwner
        }
        
        Dim panel = New StackPanel With {
            .Margin = New Thickness(20)
        }
        
        Dim label = New TextBlock With {
            .Text = "Select a default compression profile:",
            .Margin = New Thickness(0, 0, 0, 10)
        }
        
        Dim comboBox = New ComboBox With {
            .DisplayMemberPath = "Name",
            .Margin = New Thickness(0, 0, 0, 20)
        }
        
        ' Add "None" option
        comboBox.Items.Add(New CompressionProfile With {
            .Id = Guid.Empty,
            .Name = "None (Use Application Settings)"
        })
        
        ' Add all profiles
        For Each profile In profiles
            comboBox.Items.Add(profile)
        Next
        
        ' Select current default
        If AppSettings.DefaultCompressionProfileId.HasValue Then
            Dim defaultProfile = profiles.FirstOrDefault(Function(p) p.Id = AppSettings.DefaultCompressionProfileId.Value)
            If defaultProfile IsNot Nothing Then
                comboBox.SelectedItem = defaultProfile
            Else
                comboBox.SelectedIndex = 0
            End If
        Else
            comboBox.SelectedIndex = 0
        End If
        
        Dim buttonPanel = New StackPanel With {
            .Orientation = Orientation.Horizontal,
            .HorizontalAlignment = HorizontalAlignment.Right
        }
        
        Dim okButton = New Button With {
            .Content = "OK",
            .Margin = New Thickness(0, 0, 10, 0),
            .Padding = New Thickness(20, 5, 20, 5)
        }
        
        AddHandler okButton.Click, Sub(sender, e)
                                       Dim selectedProfile = TryCast(comboBox.SelectedItem, CompressionProfile)
                                       If selectedProfile IsNot Nothing Then
                                           If selectedProfile.Id = Guid.Empty Then
                                               AppSettings.DefaultCompressionProfileId = Nothing
                                           Else
                                               AppSettings.DefaultCompressionProfileId = selectedProfile.Id
                                           End If
                                           Settings.Save()
                                       End If
                                       dialog.DialogResult = True
                                   End Sub
        
        Dim cancelButton = New Button With {
            .Content = "Cancel",
            .Padding = New Thickness(20, 5, 20, 5)
        }
        
        AddHandler cancelButton.Click, Sub(sender, e)
                                           dialog.DialogResult = False
                                       End Sub
        
        buttonPanel.Children.Add(okButton)
        buttonPanel.Children.Add(cancelButton)
        
        panel.Children.Add(label)
        panel.Children.Add(comboBox)
        panel.Children.Add(buttonPanel)
        
        dialog.Content = panel
        dialog.ShowDialog()
    End Sub

End Class
