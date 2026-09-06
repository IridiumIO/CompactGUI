Class QueueSidebar

    Public Shared ReadOnly ShowCloseButtonProperty As DependencyProperty =
        DependencyProperty.Register(NameOf(ShowCloseButton), GetType(Boolean), GetType(QueueSidebar), New PropertyMetadata(True))

    Public Property ShowCloseButton As Boolean
        Get
            Return CBool(GetValue(ShowCloseButtonProperty))
        End Get
        Set(value As Boolean)
            SetValue(ShowCloseButtonProperty, value)
        End Set
    End Property

    Public Sub New()
        InitializeComponent()
        DataContext = Application.GetService(Of HomeViewModel)()
    End Sub

    Private Async Sub AddFolderButton_Click(sender As Object, e As RoutedEventArgs)
        Dim folderBrowser As New Microsoft.Win32.OpenFolderDialog With {
            .Title = "Select a folder to compress".LT(),
            .Multiselect = True,
            .ValidateNames = True
        }
        folderBrowser.ShowDialog()

        If folderBrowser.FolderNames.Length > 0 Then
            Await CType(DataContext, HomeViewModel).AddFoldersAsync(folderBrowser.FolderNames)
        End If
    End Sub
End Class
