Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports ModernWpf.Controls
Imports Ookii.Dialogs.Wpf

Public Class SearchBar


    Public Shared SearchTextProperty As DependencyProperty = DependencyProperty.Register("SearchText", GetType(Object), GetType(SearchBar), New PropertyMetadata(Nothing, New PropertyChangedCallback(AddressOf SearchTextChangedCallback)))

    Public Shared IsValidPathProperty As DependencyProperty = DependencyProperty.Register("IsValidPath", GetType(Boolean), GetType(SearchBar), New PropertyMetadata(Nothing))

    Private Shared Sub SearchTextChangedCallback(ByVal target As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)

        Dim str = TryCast(args.NewValue, String)
        Dim tg = CType(target, SearchBar)
        tg.SearchText = str.Substring(str.LastIndexOf("\") + 1)

    End Sub

    Public Property IsValidPath As Boolean
        Get
            Return GetValue(IsValidPathProperty)
        End Get
        Set(value As Boolean)
            SetValue(IsValidPathProperty, value)
        End Set
    End Property


    Public Property SearchText As String
        Get
            Return GetValue(SearchTextProperty)
        End Get
        Set(value As String)
            SetValue(SearchTextProperty, value)
        End Set
    End Property


    Private _DataPath As String
    Public ReadOnly Property DataPath As String
        Get
            Return _DataPath
        End Get

    End Property


    Sub New()
        InitializeComponent()
    End Sub


End Class
