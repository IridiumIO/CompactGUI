Public Class SteamMonitorPage

    Private ReadOnly _viewModel As SteamMonitorViewModel

    Public Sub New(viewmodel As SteamMonitorViewModel)
        InitializeComponent()
        _viewModel = viewmodel
        DataContext = viewmodel
    End Sub

    Private Async Sub OnLoaded(sender As Object, e As RoutedEventArgs)
        Await _viewModel.LoadGamesAsync()
    End Sub

    Private Sub OnCompressSplitButtonLoaded(sender As Object, e As RoutedEventArgs)
        Dim splitButton = DirectCast(sender, Wpf.Ui.Controls.SplitButton)
        splitButton.ApplyTemplate()

        Dim toggleButton = TryCast(splitButton.Template.FindName("PART_ToggleButton", splitButton), Primitives.ToggleButton)
        Dim toggleBorder = If(toggleButton Is Nothing, Nothing, TryCast(Media.VisualTreeHelper.GetParent(toggleButton), Border))
        Dim layoutGrid = If(toggleBorder Is Nothing, Nothing, TryCast(Media.VisualTreeHelper.GetParent(toggleBorder), Grid))

        If layoutGrid Is Nothing OrElse layoutGrid.ColumnDefinitions.Count <> 2 Then Return
        layoutGrid.ColumnDefinitions(0).Width = New GridLength(1, GridUnitType.Star)
    End Sub

End Class
