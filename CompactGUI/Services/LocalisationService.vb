Imports System.IO

Imports CompactGUI.Core.Settings

Imports LazyTranslate

Public NotInheritable Class LocalisationService

    Public Const DefaultCulture As String = "en-AU"

    Private ReadOnly _settingsService As ISettingsService

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
            .CatalogueDirectory = Path.Combine(_settingsService.DataFolder.FullName, "Localisation"),
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

End Class
