Class HomePage

    Private _viewModel As HomeViewModel


    Sub New(viewmodel As HomeViewModel)

        ' This call is required by the designer.
        InitializeComponent()
        _viewModel = viewmodel
        Me.DataContext = viewmodel

        ScrollViewer.SetCanContentScroll(Me, False)

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Async Sub AddFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BTN_AddFolder_1.Click, BTN_AddFolder_2.Click
        Dim folderBrowser As New Microsoft.Win32.OpenFolderDialog With {
            .Title = "Select a folder to compress",
            .Multiselect = True,
            .ValidateNames = True
        }
        folderBrowser.ShowDialog()

        If folderBrowser.FolderNames.Length > 0 Then
            Await _viewModel.AddFoldersAsync(folderBrowser.FolderNames)
        End If
    End Sub


End Class
