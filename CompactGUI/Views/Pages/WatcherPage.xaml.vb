Class WatcherPage

    Public Property viewModel As WatcherViewModel
    Sub New(VM As WatcherViewModel)

        InitializeComponent()
        Me.DataContext = VM
        viewModel = VM

    End Sub

End Class
