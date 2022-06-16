Imports System.Runtime.CompilerServices
Imports System.Text.Json

Public Class SettingsHandler

    Public Shared Property DataFolder As IO.DirectoryInfo = New IO.DirectoryInfo(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "CompactGUI"))
    Public Shared Property SettingsJSONFile As IO.FileInfo = New IO.FileInfo(IO.Path.Combine(DataFolder.FullName, "settings.json"))

    Public Shared AppSettings As Settings

    Shared Async Sub InitialiseSettings()

        If Not DataFolder.Exists Then DataFolder.Create()
        If Not SettingsJSONFile.Exists Then Await (SettingsJSONFile.Create().DisposeAsync())

        Dim SettingsJSON = IO.File.ReadAllText(SettingsJSONFile.FullName)
        If SettingsJSON = "" Then SettingsJSON = "{}"

        AppSettings = JsonSerializer.Deserialize(Of Settings)(SettingsJSON, New JsonSerializerOptions With {.IncludeFields = True})

        WriteToFile()


    End Sub


    Shared Sub WriteToFile()
        Dim output = JsonSerializer.Serialize(AppSettings)
        IO.File.WriteAllText(SettingsJSONFile.FullName, output)
    End Sub




End Class

Public Class Settings

    Public Property ResultsDBLastUpdated As DateTime
    Public Property SkipNonCompressable As Boolean
    Public Property SkipUserNonCompressable As Boolean
    Public Property NonCompressableList As New List(Of String) From {".dl_", ".gif", ".jpg", ".jpeg", ".png", ".wmf", ".mkv", ".mp4", ".wmv", ".avi", ".bik", ".bk2", ".flv", ".ogg", ".mpg", ".m2v", ".m4v", ".vob", ".mp3", ".aac", ".wma", ".flac", ".zip", ".xap", ".rar", ".7z", ".cab", ".lzx", ".docx", ".xlsx", ".pptx", ".vssx", ".vstx", ".onepkg"}

    Public Property IsContextIntegrated As Boolean
    Public Property IsStartMenuEnabled As Boolean
    Public Property SkipUserFileTypesLevel As Integer

    'TODO: Add local saving of per-folder skip list

    Public Sub Save()
        SettingsHandler.WriteToFile()
    End Sub

End Class

Module SettingsHelper


    <Extension()>
    Function InsertNodeIfNotExists(settingsJNode As Nodes.JsonNode, item As (String, Object))

        Dim exists = settingsJNode($"{item.Item1}") IsNot Nothing

        If Not exists Then
            settingsJNode($"{item.Item1}") = JsonSerializer.SerializeToNode(item.Item2)
            Return True
        End If

        Return False

    End Function

End Module