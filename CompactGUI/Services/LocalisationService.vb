Imports System.Globalization
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

        Dim followSystemLanguage = String.IsNullOrWhiteSpace(configuredLanguage)

        Await LazyTranslate.InitialiseAsync(New LocalisationOptions With {
            .SourceCulture = DefaultCulture,
            .InitialCulture = If(followSystemLanguage, DefaultCulture, configuredLanguage),
            .CatalogueDirectory = LocalisationFolder,
            .ResourceAssembly = GetType(Application).Assembly,
            .ApplicationName = "CompactGUI",
            .ProgramVersion = Application.AppVersionText,
            .OutputMissingTranslationsToDebug = False
        })

        If followSystemLanguage Then ApplySystemLanguage()
    End Function

    'Catalogues can only be enumerated after InitialiseAsync has populated the directory, and the resolved
    'culture is deliberately not saved - persisting it would stop the app following later OS changes.
    Private Sub ApplySystemLanguage()
        Dim systemLanguage = ResolveSystemLanguage()
        If systemLanguage IsNot Nothing Then L.TryLoadLanguage(systemLanguage)
    End Sub

    Private Shared Function ResolveSystemLanguage() As String
        Dim uiCulture = CultureInfo.CurrentUICulture
        If String.IsNullOrEmpty(uiCulture.Name) Then Return Nothing

        Dim availableCultures = LazyTranslate.GetAvailableLanguages().Select(Function(language) language.CultureCode).ToList()

        Dim exactMatch = availableCultures.FirstOrDefault(Function(culture) String.Equals(culture, uiCulture.Name, StringComparison.OrdinalIgnoreCase))
        If exactMatch IsNot Nothing Then Return exactMatch

        Return availableCultures.FirstOrDefault(Function(culture) String.Equals(GetLanguageSubtag(culture), uiCulture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Shared Function GetLanguageSubtag(cultureCode As String) As String
        Dim separatorIndex = cultureCode.IndexOf("-"c)
        Return If(separatorIndex < 0, cultureCode, cultureCode.Substring(0, separatorIndex))
    End Function

    Public Function LoadLanguage(languageCode As String) As Boolean
        If Not L.TryLoadLanguage(languageCode) Then Return False

        _settingsService.AppSettings.Language = languageCode
        _settingsService.SaveSettings()
        Return True
    End Function

    Public Sub ReloadCurrentLanguage()
        L.TryLoadLanguage(L.CurrentCulture.Name)
    End Sub

    Public Async Function CheckForLanguageUpdate() As Task(Of Boolean)
        Return Await LazyTranslate.CheckForLanguageUpdateAsync(L.CurrentCulture.Name, LocalisationBaseUri)
    End Function

End Class
