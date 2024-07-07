Imports System.Text.Json

Imports Microsoft.Toolkit.Mvvm.ComponentModel

Public Class SettingsHandler : Inherits ObservableObject

    Public Shared Property DataFolder As IO.DirectoryInfo = New IO.DirectoryInfo(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "CompactGUI"))
    Public Shared Property SettingsJSONFile As IO.FileInfo = New IO.FileInfo(IO.Path.Combine(DataFolder.FullName, "settings.json"))
    Public Shared Property AppSettings As Settings
    Public Shared Property SettingsVersion As Decimal = 1.2

    Shared Async Sub InitialiseSettings()

        If Not DataFolder.Exists Then DataFolder.Create()
        If Not SettingsJSONFile.Exists Then Await SettingsJSONFile.Create().DisposeAsync()

        AppSettings = DeserializeAndValidateJSON(SettingsJSONFile)

        If AppSettings.SettingsVersion = 0 OrElse SettingsVersion > AppSettings.SettingsVersion Then
            AppSettings = New Settings With {.SettingsVersion = SettingsVersion}

            Dim msgError As New ModernWpf.Controls.ContentDialog With {.Title = $"New Settings Version {SettingsVersion} Detected", .Content = "Your settings have been reset to their default to accommodate the update", .CloseButtonText = "OK"}
            Await msgError.ShowAsync()

        End If

        InitialiseWindowSize()

        WriteToFile()

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

            Dim msgError As New ModernWpf.Controls.ContentDialog With {.Title = $"Corrupted Settings File Detected", .Content = "Your settings have been reset to their default.", .CloseButtonText = "OK"}
            msgError.ShowAsync()


        End Try

        Return validatedSettings

    End Function


    Private Shared Sub InitialiseWindowSize()

        AppSettings.WindowWidth = AppSettings.WindowHeight * 0.625
        AppSettings.WindowScalingFactor = AppSettings.WindowHeight / 800

        If AppSettings.WindowHeight < 400 OrElse AppSettings.WindowHeight > 1600 Then
            AppSettings.WindowScalingFactor = 1
            AppSettings.WindowHeight = 800
            AppSettings.WindowWidth = 500
        End If

        Dim scHeight = SystemParameters.MaximizedPrimaryScreenHeight * 0.9

        If scHeight < AppSettings.WindowHeight Then
            AppSettings.WindowHeight = scHeight
            AppSettings.WindowWidth = scHeight * 0.625
            AppSettings.WindowScalingFactor = scHeight / 800
        End If

    End Sub


    Shared Sub WriteToFile()
        Dim output = JsonSerializer.Serialize(AppSettings, Jsonoptions)
        IO.File.WriteAllText(SettingsJSONFile.FullName, output)
    End Sub

End Class
