Public Class SteamMonitorPage

    Private ReadOnly _viewModel As SteamMonitorViewModel
    Private _isUpdatingSelection As Boolean
    Private _isViewChangeHandlerAttached As Boolean

    Public Sub New(viewmodel As SteamMonitorViewModel)
        InitializeComponent()
        _viewModel = viewmodel
        DataContext = viewmodel
    End Sub

    Private Async Sub OnLoaded(sender As Object, e As RoutedEventArgs)
        If Not _isViewChangeHandlerAttached Then
            AddHandler DirectCast(_viewModel.FilteredSteamGames, System.Collections.Specialized.INotifyCollectionChanged).CollectionChanged, AddressOf OnVisibleGamesChanged
            _isViewChangeHandlerAttached = True
        End If

        Await _viewModel.LoadGamesAsync()
        UpdateMasterCheckbox()
    End Sub

    Private Sub OnUnloaded(sender As Object, e As RoutedEventArgs)
        If Not _isViewChangeHandlerAttached Then Return
        RemoveHandler DirectCast(_viewModel.FilteredSteamGames, System.Collections.Specialized.INotifyCollectionChanged).CollectionChanged, AddressOf OnVisibleGamesChanged
        _isViewChangeHandlerAttached = False
    End Sub

    Private Sub OnSteamGamesSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If _isUpdatingSelection Then Return
        _viewModel.UpdateSelectedGames(DirectCast(sender, ListView).SelectedItems)
        UpdateMasterCheckbox()
    End Sub

    Private Sub OnSelectAllVisibleClick(sender As Object, e As RoutedEventArgs)
        Dim visibleGames = SteamGamesList.Items.Cast(Of Object).ToList()
        Dim allVisibleSelected = visibleGames.Count > 0 AndAlso visibleGames.All(Function(game) SteamGamesList.SelectedItems.Contains(game))

        _isUpdatingSelection = True
        If allVisibleSelected Then
            For Each game In visibleGames
                SteamGamesList.SelectedItems.Remove(game)
            Next
        Else
            For Each game In visibleGames
                If Not SteamGamesList.SelectedItems.Contains(game) Then SteamGamesList.SelectedItems.Add(game)
            Next
        End If
        _isUpdatingSelection = False

        _viewModel.UpdateSelectedGames(SteamGamesList.SelectedItems)
        UpdateMasterCheckbox()
    End Sub

    Private Sub OnVisibleGamesChanged(sender As Object, e As System.Collections.Specialized.NotifyCollectionChangedEventArgs)
        Dispatcher.BeginInvoke(New Action(AddressOf UpdateMasterCheckbox))
    End Sub

    Private Sub UpdateMasterCheckbox()
        Dim visibleGames = SteamGamesList.Items.Cast(Of Object).ToList()
        Dim selectedCount = visibleGames.Where(Function(game) SteamGamesList.SelectedItems.Contains(game)).Count()

        If selectedCount = 0 Then
            SelectAllVisibleCheckBox.IsChecked = False
        ElseIf selectedCount = visibleGames.Count Then
            SelectAllVisibleCheckBox.IsChecked = True
        Else
            SelectAllVisibleCheckBox.IsChecked = Nothing
        End If
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

    Private Sub OnDetailCardSizeChanged(sender As Object, e As SizeChangedEventArgs)
        Dim card = DirectCast(sender, Border)
        Dim clip = TryCast(card.Clip, Media.RectangleGeometry)

        If clip Is Nothing Then
            clip = New Media.RectangleGeometry With {
                .RadiusX = card.CornerRadius.TopLeft,
                .RadiusY = card.CornerRadius.TopLeft
            }
            card.Clip = clip
        End If

        clip.Rect = New Rect(0, 0, card.ActualWidth, card.ActualHeight)
    End Sub

End Class
