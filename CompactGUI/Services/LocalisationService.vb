Imports System.IO
Imports System.Net.Http
Imports System.Text

Imports CompactGUI.Core.Settings

Imports LazyTranslate

Imports Tomlyn
Imports Tomlyn.Model

Public NotInheritable Class LocalisationService

    Public Const DefaultCulture As String = "en-AU"

    Private Const LocalisationBaseUrl As String = "https://raw.githubusercontent.com/IridiumIO/CompactGUI/refs/heads/master/CompactGUI/Resources/Localisation/"
    Private Const ManifestUrl As String = LocalisationBaseUrl & "manifest.toml"

    Private Shared ReadOnly HttpClient As New HttpClient()

    Private ReadOnly _settingsService As ISettingsService

    Private ReadOnly Property LocalisationFolder As String
        Get
            Return Path.Combine(_settingsService.DataFolder.FullName, "Localisation")
        End Get
    End Property

    Public Sub New(settingsService As ISettingsService)
        _settingsService = settingsService
    End Sub

    Public Async Function InitializeAsync() As Task
        Dim configuredLanguage = _settingsService.AppSettings.Language

        If String.Equals(configuredLanguage, "en-US", StringComparison.OrdinalIgnoreCase) Then
            configuredLanguage = DefaultCulture
            _settingsService.AppSettings.Language = DefaultCulture
            _settingsService.SaveSettings()
        End If

        Await LazyTranslate.InitialiseAsync(New LocalisationOptions With {
            .SourceCulture = DefaultCulture,
            .InitialCulture = configuredLanguage,
            .CatalogueDirectory = LocalisationFolder,
            .ResourceAssembly = GetType(Application).Assembly,
            .ApplicationName = "CompactGUI"
        })
    End Function

    Public Function LoadLanguage(languageCode As String) As Boolean
        If Not L.TryLoadLanguage(languageCode) Then Return False

        _settingsService.AppSettings.Language = languageCode
        _settingsService.SaveSettings()
        Return True
    End Function

    Public Async Function CheckForLanguageUpdate() As Task(Of Boolean)
        Try
            Dim languageCode = _settingsService.AppSettings.Language
            Dim localPath = Path.Combine(LocalisationFolder, languageCode & ".toml")

            Dim manifestText = Await HttpClient.GetStringAsync(ManifestUrl)
            Dim manifest = TomlSerializer.Deserialize(Of TomlTable)(manifestText)
            Dim locales = TryCast(manifest("locales"), TomlTable)

            If locales Is Nothing OrElse Not locales.ContainsKey(languageCode) Then Return False

            Dim locale = TryCast(locales(languageCode), TomlTable)
            If locale Is Nothing Then Return False

            Dim remoteVersion = Convert.ToInt32(locale("version"))
            Dim remoteFileName = CStr(locale("file"))
            Dim localVersion = If(File.Exists(localPath),
                                  LazyTranslate.ReadLocalisationVersion(Await File.ReadAllTextAsync(localPath)),
                                  0)

            If remoteVersion <= localVersion Then Return False

            Dim remoteText = Await HttpClient.GetStringAsync(LocalisationBaseUrl & remoteFileName)
            If LazyTranslate.ReadLocalisationVersion(remoteText) <> remoteVersion Then Return False

            Dim tempPath = localPath & ".tmp"
            Await File.WriteAllTextAsync(tempPath, remoteText, Encoding.UTF8)
            File.Move(tempPath, localPath, True)

            Return True
        Catch ex As Exception
            Debug.WriteLine($"[LOC] Failed to check for localisation update: {ex.Message}")
            Return False
        End Try
    End Function

End Class
