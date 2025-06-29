Public Class ProfileManagerPage
    
    Public Property ViewModel As ProfileManagerViewModel
    
    Sub New(viewModel As ProfileManagerViewModel)
        InitializeComponent()
        Me.ViewModel = viewModel
        DataContext = viewModel
        
        ScrollViewer.SetCanContentScroll(Me, False)
    End Sub
    
End Class