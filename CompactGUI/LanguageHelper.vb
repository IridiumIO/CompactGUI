Imports System.Globalization
Imports System.Resources
Imports System.Threading
Imports System.Windows.Markup
Imports System.Windows.Data
Imports System.Reflection

Public Class LanguageHelper
    ' Supported language list
    ' @i18n
    Private Shared ReadOnly SupportedCultures As String() = {"en-US", "zh-CN"}
    Private Shared resourceManager As ResourceManager = i18n.i18n.ResourceManager
    Private Shared currentCulture As CultureInfo = Nothing

    Public Shared Function GetText(key As String) As String
        Return GetString(key)
    End Function

    Public Shared Sub Initialize()
        Dim savedLanguage As String = ReadAppConfig("language")
        If Not String.IsNullOrEmpty(savedLanguage) AndAlso SupportedCultures.Contains(savedLanguage) Then
            ApplyCulture(savedLanguage)
        Else
            SetDefaultLanguage()
        End If
    End Sub

    Public Shared Sub ChangeLanguage()
        If currentCulture Is Nothing Then
            currentCulture = Thread.CurrentThread.CurrentUICulture
        End If
        Dim currentLang As String = currentCulture.Name
        Dim nextLang As String = GetNextLanguage(currentLang)

        ApplyCulture(nextLang)
        WriteAppConfig("language", nextLang)
    End Sub

    Public Shared Function GetString(key As String, ParamArray args As Object()) As String
        Try
            Dim cultureToUse = If(currentCulture, Thread.CurrentThread.CurrentUICulture)
            Dim rawValue As String = resourceManager.GetString(key, cultureToUse)

            If String.IsNullOrEmpty(rawValue) Then
                Return key
            ElseIf args IsNot Nothing AndAlso args.Length > 0 Then
                Return String.Format(cultureToUse, rawValue, args)
            Else
                Return rawValue
            End If
        Catch ex As Exception
            Debug.WriteLine($"Failed to get multilingual text：{key}，Error：{ex.Message}")
            Return key
        End Try
    End Function

    Public Shared Sub ApplyCulture(cultureName As String)
        Try
            Dim culture As New CultureInfo(cultureName)
            Thread.CurrentThread.CurrentUICulture = culture
            Thread.CurrentThread.CurrentCulture = culture
            currentCulture = culture

        Catch ex As Exception
            Debug.WriteLine($"Application language failure：{cultureName}，Error：{ex.Message}")
            SetDefaultLanguage()
        End Try
    End Sub

    Private Shared Function GetNextLanguage(currentLanguage As String) As String
        Dim currentTwoLetter = New CultureInfo(currentLanguage).TwoLetterISOLanguageName
        For i As Integer = 0 To SupportedCultures.Length - 1
            Dim langTwoLetter = New CultureInfo(SupportedCultures(i)).TwoLetterISOLanguageName
            If langTwoLetter = currentTwoLetter Then
                Return SupportedCultures((i + 1) Mod SupportedCultures.Length)
            End If
        Next
        Return SupportedCultures(0)
    End Function

    Private Shared Sub SetDefaultLanguage()
        ' Set the default language according to the system language.
        '@i18n
        Dim langMapping As New Dictionary(Of String, String) From {
        {"en", "en-US"},
        {"zh", "zh-CN"}
    }

        Dim systemLang As String = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLower()
        Dim defaultLang As String = If(langMapping.ContainsKey(systemLang), langMapping(systemLang), "en-US")

        ApplyCulture(defaultLang)
        WriteAppConfig("language", defaultLang)
    End Sub

    Public Shared Function GetCurrentLanguage() As String
        Return If(currentCulture, Thread.CurrentThread.CurrentUICulture).Name
    End Function

    Public Shared Function ReadAppConfig(key As String) As String
        Try
            Dim config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None)
            Return If(config.AppSettings.Settings(key)?.Value, String.Empty)
        Catch ex As Exception
            Debug.WriteLine($"Read configuration failed：{key}，Error：{ex.Message}")
            Return String.Empty
        End Try
    End Function

    Public Shared Sub WriteAppConfig(key As String, value As String)
        Try
            Dim config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None)
            If config.AppSettings.Settings(key) IsNot Nothing Then
                config.AppSettings.Settings(key).Value = value
            Else
                config.AppSettings.Settings.Add(key, value)
            End If
            config.Save(System.Configuration.ConfigurationSaveMode.Modified)
            System.Configuration.ConfigurationManager.RefreshSection("appSettings")
        Catch ex As Exception
            Debug.WriteLine($"Write configuration failed：{key}，Error：{ex.Message}")
        End Try
    End Sub
End Class

<MarkupExtensionReturnType(GetType(String))>
Public Class LocalizeExtension
    Inherits MarkupExtension

    Private _key As String

    Public Sub New(key As String)
        _key = key
    End Sub

    Public Overrides Function ProvideValue(serviceProvider As IServiceProvider) As Object
        Return LanguageHelper.GetString(_key)
    End Function
End Class