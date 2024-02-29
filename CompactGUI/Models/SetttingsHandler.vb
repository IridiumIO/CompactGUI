Imports System.Text.Json
Imports Microsoft.Toolkit.Mvvm.ComponentModel

Public Class SettingsHandler : Inherits ObservableObject

    Public Shared Property DataFolder As IO.DirectoryInfo = New IO.DirectoryInfo(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "CompactGUI"))
    Public Shared Property SettingsJSONFile As IO.FileInfo = New IO.FileInfo(IO.Path.Combine(DataFolder.FullName, "settings.json"))
    Public Shared Property AppSettings As Settings
    Private Shared Property SettingsVersion As Decimal = 1.2

    Shared Async Sub InitialiseSettings()

        If Not DataFolder.Exists Then DataFolder.Create()
        If Not SettingsJSONFile.Exists Then Await SettingsJSONFile.Create().DisposeAsync()

        AppSettings = DeserializeAndValidateJSON(SettingsJSONFile)

        If AppSettings.SettingsVersion = 0 OrElse SettingsVersion > AppSettings.SettingsVersion Then
            AppSettings = New Settings
            AppSettings.SettingsVersion = SettingsVersion

            Dim msgError As New ModernWpf.Controls.ContentDialog With {.Title = $"New Settings Version {SettingsVersion} Detected", .Content = "Your settings have been reset to their default to accommodate the update", .CloseButtonText = "OK"}
            Await msgError.ShowAsync()

        End If

        InitialiseWindowSize()

        WriteToFile()

    End Sub

    Private Shared Function DeserializeAndValidateJSON(inputjsonFile As IO.FileInfo) As Settings
        Dim SettingsJSON = IO.File.ReadAllText(inputjsonFile.FullName)
        If SettingsJSON = "" Then SettingsJSON = "{}"

        Dim validatedSettings As Settings

        Try
            validatedSettings = JsonSerializer.Deserialize(Of Settings)(SettingsJSON, New JsonSerializerOptions With {.IncludeFields = True})
        Catch ex As Exception
            validatedSettings = New Settings
            validatedSettings.SettingsVersion = SettingsVersion

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

    End Sub


    Shared Sub WriteToFile()
        Dim output = JsonSerializer.Serialize(AppSettings, New JsonSerializerOptions With {.IncludeFields = True, .WriteIndented = True})
        IO.File.WriteAllText(SettingsJSONFile.FullName, output)
    End Sub

End Class


Public Class Settings : Inherits ObservableObject

    Public Property SettingsVersion As Decimal
    Public Property ResultsDBLastUpdated As DateTime = DateTime.UnixEpoch
    Public Property SelectedCompressionMode As Integer = 0
    Public Property SkipNonCompressable As Boolean = False
    Public Property SkipUserNonCompressable As Boolean = False
    Public Property WatchFolderForChanges As Boolean = False
    Public Property NonCompressableList As New List(Of String) From {".dl_", ".gif", ".jpg", ".jpeg", ".png", ".wmf", ".mkv", ".mp4", ".wmv", ".avi", ".bik", ".bk2", ".flv", ".ogg", ".mpg", ".m2v", ".m4v", ".vob", ".mp3", ".aac", ".wma", ".flac", ".zip", ".xap", ".rar", ".7z", ".cab", ".lzx", ".docx", ".xlsx", ".pptx", ".vssx", ".vstx", ".onepkg", ".tar", ".gz", ".dmg", ".bz2", ".tgz", ".lz", ".xz", ".txz"}
    Public Property IsContextIntegrated As Boolean = False
    Public Property IsStartMenuEnabled As Boolean = False
    Public Property SkipUserFileTypesLevel As Integer = 0
    Public Property ShowNotifications As Boolean = False
    Private _WindowScalingFactor = 1
    Public Property WindowScalingFactor As Double
        Get
            Return _WindowScalingFactor
        End Get
        Set(value As Double)
            _WindowScalingFactor = value
            WindowWidth = 500 * value
            WindowHeight = 800 * value

            OnPropertyChanged()
        End Set
    End Property

    Public Property WindowWidth As Decimal = 500
    Public Property WindowHeight As Decimal = 800

    Public Property AllowMultiInstance As Boolean = False

    'TODO: Add local saving of per-folder skip list
    Public Sub Save()
        SettingsHandler.WriteToFile()
    End Sub

End Class
