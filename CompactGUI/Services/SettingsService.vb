Imports System.IO
Imports System.Text.Json

Imports CompactGUI.Core.Settings
Imports CompactGUI.Logging

Imports Microsoft.Extensions.Logging

Public Class SettingsService : Implements ISettingsService

    Public ReadOnly Property DataFolder As DirectoryInfo Implements ISettingsService.DataFolder
    Public ReadOnly Property SettingsJSONFile As FileInfo Implements ISettingsService.SettingsJSONFile
    Public ReadOnly Property SettingsVersion As Decimal Implements ISettingsService.SettingsVersion
    Public Property AppSettings As Settings Implements ISettingsService.AppSettings

    Public Sub New()

        DataFolder = New IO.DirectoryInfo(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "CompactGUI"))
        SettingsJSONFile = New IO.FileInfo(IO.Path.Combine(DataFolder.FullName, "settings.json"))

        SettingsVersion = 1.2

    End Sub

    Public Sub LoadSettings() Implements ISettingsService.LoadSettings
        If Not DataFolder.Exists Then DataFolder.Create()
        If Not SettingsJSONFile.Exists Then SettingsJSONFile.Create().Dispose()

        AppSettings = DeserializeAndValidateJSON(SettingsJSONFile)

        If AppSettings.SettingsVersion = 0 OrElse SettingsVersion > AppSettings.SettingsVersion Then

            Dim skipList = AppSettings.NonCompressableList

            AppSettings = New Settings With {.SettingsVersion = SettingsVersion, .NonCompressableList = skipList}

            Dim msgError As New Wpf.Ui.Controls.ContentDialog With {.Title = $"New Settings Version {SettingsVersion} Detected", .Content = "Your settings have been reset to their default to accommodate the update", .CloseButtonText = "OK"}
            msgError.ShowAsync()

        End If

        Dim output = JsonSerializer.Serialize(AppSettings, Jsonoptions)
        IO.File.WriteAllText(SettingsJSONFile.FullName, output)
    End Sub

    Public Sub SaveSettings() Implements ISettingsService.SaveSettings
        ScheduleSettingsSave()
    End Sub

    Public Sub ScheduleSettingsSave() Implements ISettingsService.ScheduleSettingsSave
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


    Private ReadOnly Jsonoptions As New JsonSerializerOptions With {.IncludeFields = True, .WriteIndented = True}

    Private Function DeserializeAndValidateJSON(inputjsonFile As IO.FileInfo) As Settings
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



    Private ReadOnly debounceDelay As TimeSpan = TimeSpan.FromMilliseconds(1000)
    Private ReadOnly timerLock As New Object()
    Private debounceTimer As System.Timers.Timer


    Private Async Function WriteToFileAsync() As Task
        Try
            Dim output = JsonSerializer.Serialize(AppSettings, Jsonoptions)
            Await IO.File.WriteAllTextAsync(SettingsJSONFile.FullName, output)
            SettingsLog.SettingsSaved(Application.GetService(Of ILogger(Of Settings)))
        Catch ex As Exception
            ' Log or handle exception
        End Try
    End Function

End Class
