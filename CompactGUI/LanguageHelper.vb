Imports System.ComponentModel
Imports System.Globalization
Imports System.Resources
Imports System.Threading
Imports System.Windows.Markup

Imports CompactGUI.Core.Settings


Public Class LanguageItem
    Public Property Name As String
    Public Property ISOCountryCode As String
    Public Property CultureCode As String
End Class

Public Class LanguageHelper
    ' Supported language list
    ' @i18n
    Private Shared ReadOnly SupportedCultures As String() = {"en-US", "ru-RU", "zh-CN", "es-ES", "tr-TR"}
    Private Shared resourceManager As ResourceManager = i18n.i18n.ResourceManager
    Private Shared currentCulture As CultureInfo = Nothing

    Private Shared _applicationSettings As Settings

    Public Shared Event LanguageChanged As EventHandler

    Public Shared Function GetText(key As String) As String
        Return GetString(key)
    End Function

    Public Shared Sub Initialize(applicationSettings As Settings)
        _applicationSettings = applicationSettings
        Dim savedLanguage = _applicationSettings.Language

        If Not String.IsNullOrEmpty(savedLanguage) AndAlso SupportedCultures.Contains(savedLanguage) Then
            ApplyCulture(savedLanguage)
        Else
            SetDefaultLanguage()
        End If
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

            _applicationSettings.Language = cultureName
            Application.GetService(Of ISettingsService).SaveSettings()

            RaiseEvent LanguageChanged(Nothing, EventArgs.Empty)
        Catch ex As Exception
            Debug.WriteLine($"Application language failure：{cultureName}，Error：{ex.Message}")
            SetDefaultLanguage()
        End Try
    End Sub

    Private Shared Sub SetDefaultLanguage()
        ' Set the default language according to the system language.
        '@i18n
        Dim langMapping As New Dictionary(Of String, String) From {
        {"en", "en-US"},
        {"ru", "ru-RU"},
        {"zh", "zh-CN"},
        {"es", "es-ES"},
        {"tr", "tr-TR"}
    }

        Dim systemLang As String = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLower()
        Dim defaultLang As String = If(langMapping.ContainsKey(systemLang), langMapping(systemLang), "en-US")

        ApplyCulture(defaultLang)

    End Sub

    Public Shared Function GetCurrentLanguage() As String
        Return If(currentCulture, Thread.CurrentThread.CurrentUICulture).Name
    End Function


End Class

<MarkupExtensionReturnType(GetType(Object))>
Public Class LocalizeExtension
    Inherits MarkupExtension
    Implements INotifyPropertyChanged

    Private ReadOnly _key As String

    Public Sub New(key As String)
        _key = key
        AddHandler LanguageHelper.LanguageChanged, AddressOf OnLanguageChanged
    End Sub

    Public ReadOnly Property Value As String
        Get
            Return LanguageHelper.GetString(_key)
        End Get
    End Property

    Public Overrides Function ProvideValue(serviceProvider As IServiceProvider) As Object
        Dim provideValueTarget = TryCast(serviceProvider.GetService(GetType(IProvideValueTarget)), IProvideValueTarget)
        Dim targetProperty = provideValueTarget?.TargetProperty

        ' If target is not a DependencyProperty (e.g. Binding.StringFormat), return plain string.
        If Not TypeOf targetProperty Is DependencyProperty Then
            Return Value
        End If

        Dim binding As New Binding(NameOf(Value)) With {
            .Source = Me,
            .Mode = BindingMode.OneWay
        }

        Return binding.ProvideValue(serviceProvider)
    End Function

    Private Sub OnLanguageChanged(sender As Object, e As EventArgs)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Value)))
    End Sub

    'Future me: don't try to replace this with ObservableObject. It's cursed
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class
