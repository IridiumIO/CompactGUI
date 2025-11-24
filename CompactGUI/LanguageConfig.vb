Imports System.IO
Imports System.Text.Json

Public Class LanguageConfig
    Private Shared ConfigPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language_config.json")

    Public Class ConfigData
        Public Property LanguageCode As String = "en-US"
    End Class

    Public Shared Sub SaveLanguage(langCode As String)
        Try
            Dim data As New ConfigData With {.LanguageCode = langCode}
            Dim jsonString = JsonSerializer.Serialize(data)
            File.WriteAllText(ConfigPath, jsonString)
        Catch ex As Exception
        End Try
    End Sub

    Public Shared Function GetLanguage() As String
        Try
            If File.Exists(ConfigPath) Then
                Dim jsonString = File.ReadAllText(ConfigPath)
                Dim data = JsonSerializer.Deserialize(Of ConfigData)(jsonString)
                Return If(data IsNot Nothing AndAlso Not String.IsNullOrEmpty(data.LanguageCode), data.LanguageCode, "en-US")
            Else
                Dim sysLang = System.Globalization.CultureInfo.CurrentUICulture.Name
                Dim i18nPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "i18n", $"{sysLang}.json")
                If File.Exists(i18nPath) Then
                    Return sysLang
                End If
            End If
        Catch ex As Exception
        End Try
        Return "en-US"
    End Function
End Class