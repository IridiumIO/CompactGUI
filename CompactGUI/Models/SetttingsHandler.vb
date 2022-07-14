Imports System.Text.Json
Imports Microsoft.Toolkit.Mvvm.ComponentModel

Public Class SettingsHandler : Inherits ObservableObject

    Public Shared Property DataFolder As IO.DirectoryInfo = New IO.DirectoryInfo(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "CompactGUI"))
    Public Shared Property SettingsJSONFile As IO.FileInfo = New IO.FileInfo(IO.Path.Combine(DataFolder.FullName, "settings.json"))
    Public Shared Property AppSettings As Settings
    Private Shared Property SettingsVersion As Decimal = CDec(1.2)

    Public Shared Async Sub InitialiseSettings()

        If Not DataFolder.Exists Then DataFolder.Create()
        If Not SettingsJSONFile.Exists Then Await SettingsJSONFile.Create().DisposeAsync()

        Dim SettingsJSON = IO.File.ReadAllText(SettingsJSONFile.FullName)
        If SettingsJSON = "" Then SettingsJSON = "{}"

        AppSettings = JsonSerializer.Deserialize(Of Settings)(SettingsJSON, New JsonSerializerOptions With {.IncludeFields = True})

        If AppSettings.SettingsVersion = 0 OrElse SettingsVersion > AppSettings.SettingsVersion Then
            AppSettings = New Settings With {
                .SettingsVersion = SettingsVersion
            }

            Dim msgError As New ModernWpf.Controls.ContentDialog With {.Title = $"New Settings Version {SettingsVersion} Detected", .Content = "Your settings have been reset to their default to accommodate the update", .CloseButtonText = "OK"}
            Await msgError.ShowAsync()

        End If

        WriteToFile()

    End Sub

    Public Shared Sub WriteToFile()
        Dim output = JsonSerializer.Serialize(AppSettings)
        IO.File.WriteAllText(SettingsJSONFile.FullName, output)
    End Sub

End Class


Public Class Settings : Inherits ObservableObject

    Public Property SettingsVersion As Decimal
    Public Property ResultsDBLastUpdated As Date = Date.UnixEpoch
    Public Property SkipNonCompressable As Boolean = False
    Public Property SkipUserNonCompressable As Boolean = False
    Public Property WatchFolderForChanges As Boolean = False
    Public Property NonCompressableList As New List(Of String) From {".dl_", ".gif", ".jpg", ".jpeg", ".png", ".wmf", ".mkv", ".mp4", ".wmv", ".avi", ".bik", ".bk2", ".flv", ".ogg", ".mpg", ".m2v", ".m4v", ".vob", ".mp3", ".aac", ".wma", ".flac", ".zip", ".xap", ".rar", ".7z", ".cab", ".lzx", ".docx", ".xlsx", ".pptx", ".vssx", ".vstx", ".onepkg"}
    Public Property IsContextIntegrated As Boolean = False
    Public Property IsStartMenuEnabled As Boolean = False
    Public Property SkipUserFileTypesLevel As Integer = 0

    ' TODO: Add local saving of per-folder skip list
    Public Shared Sub Save()
        SettingsHandler.WriteToFile()
    End Sub

End Class
