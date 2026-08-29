Imports System.IO

Imports CompactGUI.Core.Settings

Imports LazyTranslate

Public NotInheritable Class LocalisationService

    Public Const DefaultCulture As String = "en-AU"

    Private Shared ReadOnly LocalisationBaseUri As New Uri("https://raw.githubusercontent.com/IridiumIO/CompactGUI/refs/heads/master/CompactGUI/Resources/Localisation/")

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
        Return Await LazyTranslate.CheckForLanguageUpdateAsync(_settingsService.AppSettings.Language, LocalisationBaseUri)
    End Function

End Class
