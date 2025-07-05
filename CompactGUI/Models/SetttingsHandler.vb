Imports System.Text.Json
Imports System.Threading

Imports CommunityToolkit.Mvvm.ComponentModel

Imports CompactGUI.Logging

Imports Microsoft.Extensions.Logging


Public Class SettingsHandler : Inherits ObservableObject

    Public Shared Property DataFolder As IO.DirectoryInfo = New IO.DirectoryInfo(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "CompactGUI"))
    Public Shared Property SettingsJSONFile As IO.FileInfo = New IO.FileInfo(IO.Path.Combine(DataFolder.FullName, "settings.json"))
    Public Shared Property AppSettings As Settings
    Public Shared Property SettingsVersion As Decimal = 1.2

    Shared Sub InitialiseSettings()

        If Not DataFolder.Exists Then DataFolder.Create()
        If Not SettingsJSONFile.Exists Then SettingsJSONFile.Create().Dispose()

        AppSettings = DeserializeAndValidateJSON(SettingsJSONFile)

        If AppSettings.SettingsVersion = 0 OrElse SettingsVersion > AppSettings.SettingsVersion Then
            AppSettings = New Settings With {.SettingsVersion = SettingsVersion}

            Dim msgError As New Wpf.Ui.Controls.ContentDialog With {.Title = $"New Settings Version {SettingsVersion} Detected", .Content = "Your settings have been reset to their default to accommodate the update", .CloseButtonText = "OK"}
            msgError.ShowAsync()

        End If

        Dim output = JsonSerializer.Serialize(AppSettings, Jsonoptions)
        IO.File.WriteAllText(SettingsJSONFile.FullName, output)

    End Sub

    Private Shared ReadOnly Jsonoptions As New JsonSerializerOptions With {.IncludeFields = True, .WriteIndented = True}

    Private Shared Function DeserializeAndValidateJSON(inputjsonFile As IO.FileInfo) As Settings
        Dim SettingsJSON = IO.File.ReadAllText(inputjsonFile.FullName)
        If SettingsJSON = "" Then SettingsJSON = "{}"

        Dim validatedSettings As Settings

        Try

            validatedSettings = JsonSerializer.Deserialize(Of Settings)(SettingsJSON, Jsonoptions)
        Catch ex As Exception
            validatedSettings = New Settings With {.SettingsVersion = SettingsVersion}

            Dim msgError As New Wpf.Ui.Controls.ContentDialog With {.Title = $"Corrupted Settings File Detected", .Content = "Your settings have been reset to their default.", .CloseButtonText = "OK"}
            msgError.ShowAsync()


        End Try

        Return validatedSettings

    End Function



    Shared Sub WriteToFile()
        ScheduleWriteToFile()
    End Sub

    Private Shared ReadOnly debounceDelay As TimeSpan = TimeSpan.FromMilliseconds(2000)
    Private Shared ReadOnly timerLock As New Object()
    Private Shared debounceTimer As System.Timers.Timer

    Public Shared Sub ScheduleWriteToFile()
        SyncLock timerLock
            If debounceTimer Is Nothing Then
                debounceTimer = New System.Timers.Timer(debounceDelay.TotalMilliseconds)
                debounceTimer.AutoReset = False
                AddHandler debounceTimer.Elapsed, Async Sub(__, ___)
                                                      Await WriteToFileAsync()
                                                  End Sub
            Else
                debounceTimer.Stop()
            End If
            debounceTimer.Start()
        End SyncLock
    End Sub

    Private Shared Async Function WriteToFileAsync() As Task
        Try
            Dim output = JsonSerializer.Serialize(AppSettings, Jsonoptions)
            Await IO.File.WriteAllTextAsync(SettingsJSONFile.FullName, output)
            SettingsLog.SettingsSaved(Application.GetService(Of ILogger(Of Settings)))
        Catch ex As Exception
            ' Log or handle exception
        End Try
    End Function


End Class
