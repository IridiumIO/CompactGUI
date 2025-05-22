Class WatcherPage

    Public Property viewModel As WatcherViewModel
    Sub New(VM As WatcherViewModel)

        InitializeComponent()
        DataContext = VM
        viewModel = VM

        ScrollViewer.SetCanContentScroll(Me, False)

    End Sub

End Class
