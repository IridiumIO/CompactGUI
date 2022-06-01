Imports ModernWpf.Controls
Imports Ookii.Dialogs.Wpf

Public Class SearchBar


    Public ReadOnly SearchTextProperty As DependencyProperty = DependencyProperty.Register("SearchText", GetType(String), GetType(SearchBar), New PropertyMetadata(Nothing))

    Public ReadOnly IsValidPathProperty As DependencyProperty = DependencyProperty.Register("IsValidPath", GetType(Boolean), GetType(SearchBar), New PropertyMetadata(Nothing))

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

    Public Function SetDataPathAndReturn(dirPath) As Boolean

        If dirPath = "" Then Return False

        If Not verifyFolder(dirPath) Then
            Dim msgError As New ContentDialog With {
            .Title = "Invalid Folder",
            .Content = "For safety, this folder cannot be chosen.",
            .CloseButtonText = "OK"
            }

            msgError.ShowAsync()

            Return False
        End If

        _DataPath = dirPath
        SearchText = dirPath.Substring(dirPath.LastIndexOf("\") + 1)
        IsValidPath = True

        Return True


    End Function

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub


    Shared Function verifyFolder(folder As String) As Boolean

        If Not IO.Directory.Exists(folder) Then : Return False
        ElseIf folder.Contains(":\Windows") Then : Return False
        ElseIf folder.EndsWith(":\") Then : Return False
        End If

        Return True

    End Function

End Class
