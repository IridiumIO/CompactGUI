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
        
        ' Check configured language
        Dim currentLang = LanguageConfig.GetLanguage()
        
        ' Only proceed if language is NOT en-US (default)
        If currentLang <> "en-US" Then
            ' Try to load language file from i18n folder
            Dim langFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "i18n", $"{currentLang}.json")
            If File.Exists(langFile) Then
                LoadLanguageFile(langFile)
                
                ' Register global loaded events ONLY if translation is enabled
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

    ' Empty stub for compatibility
    Public Shared Sub TranslateWindow(obj As DependencyObject)
    End Sub

    Private Shared Sub OnElementLoaded(sender As Object, e As RoutedEventArgs)
        Dim element As DependencyObject = TryCast(sender, DependencyObject)
        If element Is Nothing Then Return

        ' Use ContextIdle priority to ensure the visual tree is populated and properties are set
        If TypeOf element Is DispatcherObject Then
            DirectCast(element, DispatcherObject).Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, Sub()
                                                                                                            Try
                                                                                                                RecursiveTranslate(element)
                                                                                                            Catch ex As Exception
                                                                                                                ' Ignore errors during recursive translation
                                                                                                            End Try
                                                                                                        End Sub)
        End If
    End Sub

    ' Text normalization: remove newlines, tabs, and merge spaces
    Private Shared Function NormalizeText(input As String) As String
        If String.IsNullOrEmpty(input) Then Return ""
        Dim normalized As String = Regex.Replace(input, "\s+", " ")
        Return normalized.Trim()
    End Function

    ' Core translation logic with Exact and Partial matching
    Private Shared Sub TryReplace(original As String, applyAction As Action(Of String))
        If String.IsNullOrWhiteSpace(original) Then Return

        Dim normalizedText As String = NormalizeText(original)
        Dim found As Boolean = False

        ' 1. Exact Match (Priority)
        If Translations.ContainsKey(normalizedText) Then
            applyAction(Translations(normalizedText))
            found = True
        Else
            ' 2. Partial / Fuzzy Match (for dynamic content like "6.52 GB saved")
            For Each kvp In Translations
                ' Only attempt match if the key is meaningful (length > 1) to avoid false positives
                If kvp.Key.Length > 1 AndAlso normalizedText.Contains(kvp.Key) Then
                    Dim translated As String = normalizedText.Replace(kvp.Key, kvp.Value)
                    
                    ' Only apply if replacement actually happened
                    If translated <> normalizedText Then
                        applyAction(translated)
                        found = True
                        Exit For ' Apply the first matching translation found
                    End If
                End If
            Next
        End If
        
        ' Debug log
        ' System.Diagnostics.Debug.WriteLine("Scanning: [" & normalizedText & "] -> " & If(found, "Translated", "Unmatched"))
    End Sub

    ' Recursive Reflection Translation
    Private Shared Sub RecursiveTranslate(element As Object)
        If element Is Nothing Then Return

        ' 1. Reflection scan for common text properties
        Dim targetProperties As String() = {"Text", "Content", "Header", "Title", "Description", "Label", "ToolTip", "PlaceholderText"}
        Dim type As Type = element.GetType()

        For Each propName In targetProperties
            Dim prop = type.GetProperty(propName)
            If prop IsNot Nothing AndAlso prop.CanRead AndAlso prop.CanWrite Then
                Try
                    Dim val = prop.GetValue(element)
                    ' If property is String -> Try translate
                    If TypeOf val Is String Then
                        TryReplace(CStr(val), Sub(newVal) prop.SetValue(element, newVal))
                    ' If property is Object (e.g. StackPanel inside Content) -> Recurse
                    ElseIf val IsNot Nothing AndAlso Not TypeOf val Is ValueType Then 
                        RecursiveTranslate(val)
                    End If
                Catch ex As Exception
                End Try
            End If
        Next

        ' 2. Special handling for TextBlock Inlines (Run elements)
        If TypeOf element Is TextBlock Then
            Dim tb = DirectCast(element, TextBlock)
            For Each inline In tb.Inlines
                If TypeOf inline Is Run Then
                    RecursiveTranslate(inline)
                End If
            Next
        End If

        ' 3. Deep traversal of Visual Tree children
        If TypeOf element Is DependencyObject Then
            Dim depObj = DirectCast(element, DependencyObject)
            ' Only traverse Visual or Visual3D to avoid issues with logical-only elements
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