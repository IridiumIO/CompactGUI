Public Class DatabasePage
    Public Property viewModel As DatabaseViewModel
    Sub New(VM As DatabaseViewModel)

        InitializeComponent()
        DataContext = VM
        viewModel = VM

        ScrollViewer.SetCanContentScroll(Me, False)

    End Sub

End Class
