Imports System.IO
Imports System.Reflection
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Media
Imports System.Windows.Threading

Public Class TranslationHelper

    Private Shared _isInitialized As Boolean = False
    Public Shared Translations As New Dictionary(Of String, String)

    Public Shared Sub StartAutoTranslate()
        If _isInitialized Then Return
        _isInitialized = True
        
        Dim currentLang = LanguageConfig.GetLanguage()
        
        If currentLang <> "en-US" Then
            Dim langFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "i18n", $"{currentLang}.json")
            If File.Exists(langFile) Then
                LoadLanguageFile(langFile)
                
                EventManager.RegisterClassHandler(GetType(FrameworkElement), FrameworkElement.LoadedEvent, New RoutedEventHandler(AddressOf OnElementLoaded))
                EventManager.RegisterClassHandler(GetType(FrameworkContentElement), FrameworkContentElement.LoadedEvent, New RoutedEventHandler(AddressOf OnElementLoaded))
            End If
        End If
    End Sub

    Public Shared Sub LoadLanguageFile(filePath As String)
        Try
            Dim jsonContent = File.ReadAllText(filePath)
            Dim newTranslations = JsonSerializer.Deserialize(Of Dictionary(Of String, String))(jsonContent)
            If newTranslations IsNot Nothing Then
                Translations = newTranslations
            End If
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Failed to load language file: {ex.Message}")
        End Try
    End Sub

    Public Shared Sub TranslateWindow(obj As DependencyObject)
    End Sub

    Private Shared Sub OnElementLoaded(sender As Object, e As RoutedEventArgs)
        Dim element As DependencyObject = TryCast(sender, DependencyObject)
        If element Is Nothing Then Return

        If TypeOf element Is DispatcherObject Then
            DirectCast(element, DispatcherObject).Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, Sub()
                                                                                                            Try
                                                                                                                RecursiveTranslate(element)
                                                                                                            Catch ex As Exception
                                                                                                            End Try
                                                                                                        End Sub)
        End If
    End Sub

    Private Shared Function NormalizeText(input As String) As String
        If String.IsNullOrEmpty(input) Then Return ""
        Dim normalized As String = Regex.Replace(input, "\s+", " ")
        Return normalized.Trim()
    End Function

    Public Shared Function GetString(key As String) As String
        If Translations.ContainsKey(key) Then
            Return Translations(key)
        End If
        Return key
    End Function

    Public Shared Function GetStringForLanguage(langCode As String, key As String) As String
        Dim filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "i18n", $"{langCode}.json")
        If File.Exists(filePath) Then
            Try
                Dim jsonContent = File.ReadAllText(filePath)
                Dim tempDict = JsonSerializer.Deserialize(Of Dictionary(Of String, String))(jsonContent)
                If tempDict IsNot Nothing AndAlso tempDict.ContainsKey(key) Then
                    Return tempDict(key)
                End If
            Catch
            End Try
        End If

        If langCode = "en-US" Then Return key

        Return GetString(key)
    End Function

    Private Shared Sub TryReplace(original As String, applyAction As Action(Of String))
        If String.IsNullOrWhiteSpace(original) Then Return

        Dim normalizedText As String = NormalizeText(original)
        Dim found As Boolean = False

        If Translations.ContainsKey(normalizedText) Then
            applyAction(Translations(normalizedText))
            found = True
        Else
            For Each kvp In Translations
                If kvp.Key.Length > 3 AndAlso normalizedText.Contains(kvp.Key) Then
                    Dim translated As String = normalizedText.Replace(kvp.Key, kvp.Value)
                    
                    If translated <> normalizedText Then
                        applyAction(translated)
                        found = True
                        Exit For
                    End If
                End If
            Next
        End If
    End Sub

    Private Shared Sub RecursiveTranslate(element As Object)
        If element Is Nothing Then Return

        Dim targetProperties As String() = {"Text", "Content", "Header", "Title", "Description", "Label", "ToolTip", "PlaceholderText"}
        Dim type As Type = element.GetType()

        For Each propName In targetProperties
            Dim prop = type.GetProperty(propName)
            If prop IsNot Nothing AndAlso prop.CanRead AndAlso prop.CanWrite Then
                Try
                    Dim val = prop.GetValue(element)
                    If TypeOf val Is String Then
                        TryReplace(CStr(val), Sub(newVal) prop.SetValue(element, newVal))
                    ElseIf val IsNot Nothing AndAlso Not TypeOf val Is ValueType Then 
                        RecursiveTranslate(val)
                    End If
                Catch ex As Exception
                End Try
            End If
        Next

        If TypeOf element Is TextBlock Then
            Dim tb = DirectCast(element, TextBlock)
            For Each inline In tb.Inlines
                If TypeOf inline Is Run Then
                    RecursiveTranslate(inline)
                End If
            Next
        End If

        If TypeOf element Is DependencyObject Then
            Dim depObj = DirectCast(element, DependencyObject)
            If TypeOf depObj Is Visual OrElse TypeOf depObj Is System.Windows.Media.Media3D.Visual3D Then
                Dim childCount = VisualTreeHelper.GetChildrenCount(depObj)
                For i = 0 To childCount - 1
                    Dim child = VisualTreeHelper.GetChild(depObj, i)
                    RecursiveTranslate(child)
                Next
            End If
        End If

    End Sub

End Class