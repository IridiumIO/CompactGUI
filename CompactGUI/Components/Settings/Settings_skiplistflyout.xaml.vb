Imports System.Collections.ObjectModel

Imports CompactGUI.Core.Settings

Public Class Settings_skiplistflyout

    Private _settingsService As ISettingsService
    Private _tokens As New ObservableCollection(Of String)
    Private _editingToken As String = Nothing
    Private _suppressTextChanged As Boolean = False

    Sub New()

        InitializeComponent()
        _settingsService = Application.GetService(Of ISettingsService)()
        UiTokenList.ItemsSource = _tokens
        AddHandler UiTokenInput.TextChanged, AddressOf UiTokenInput_TextChanged
        AddHandler UiTokenInput.KeyDown, AddressOf UiTokenInput_KeyDown
        AddHandler UiTokenInput.LostFocus, AddressOf UiTokenInput_LostFocus
        PopulateTokens()
    End Sub

    Private Sub PopulateTokens()
        _tokens.Clear()
        For Each i In _settingsService.AppSettings.NonCompressableList
            _tokens.Add(i)
        Next
        UiTokenInput.Clear()
        _editingToken = Nothing
    End Sub

    Private Sub ChipDelete_Click(sender As Object, e As RoutedEventArgs)
        Dim chip = TryCast(TryCast(sender, FrameworkElement)?.DataContext, String)
        If chip IsNot Nothing Then _tokens.Remove(chip)
    End Sub

    Private Sub Chip_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        Dim chip = TryCast(sender, TextBlock)
        If chip Is Nothing Then Return

        CommitInputText()

        _editingToken = chip.Text
        _suppressTextChanged = True
        UiTokenInput.Text = chip.Text
        UiTokenInput.CaretIndex = UiTokenInput.Text.Length
        _suppressTextChanged = False
    End Sub

    Private Sub UiTokenInput_TextChanged(sender As Object, e As TextChangedEventArgs)
        If _suppressTextChanged Then Return

        If UiTokenInput.Text.IndexOfAny(New Char() {";"c, ","c}) >= 0 Then
            CommitInputText()
        End If
    End Sub

    Private Sub UiTokenInput_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Enter Then
            CommitInputText()
            e.Handled = True
        End If
    End Sub

    Private Sub UiTokenInput_LostFocus(sender As Object, e As RoutedEventArgs)
        If _editingToken IsNot Nothing Then
            CommitInputText()
        End If
    End Sub

    Private Sub CommitInputText()
        Dim parts = UiTokenInput.Text.Split(New Char() {";"c, ","c}, StringSplitOptions.RemoveEmptyEntries).
            Select(Function(p) p.Trim()).
            Where(Function(t) t.Length > 0).ToList()

        If _editingToken IsNot Nothing Then
            Dim original = _editingToken
            _editingToken = Nothing

            If parts.Count = 0 OrElse (parts.Count = 1 AndAlso parts(0) = original) Then
                UiTokenInput.Clear()
                Return
            End If

            _tokens.Remove(original)
            For Each p In parts
                _tokens.Add(p)
            Next
            UiTokenInput.Clear()
        ElseIf parts.Count > 0 Then
            For Each p In parts
                _tokens.Add(p)
            Next
            UiTokenInput.Clear()
        End If
    End Sub

    Private Sub UIReset_Click(sender As Object, e As RoutedEventArgs)
        _settingsService.AppSettings.NonCompressableList = New Settings().NonCompressableList
        _settingsService.SaveSettings()
        PopulateTokens()
    End Sub

    Private Sub UISave_Click(sender As Object, e As RoutedEventArgs)
        Dim finalList As New List(Of String)(_tokens)

        Dim pending = UiTokenInput.Text.Trim()
        If pending.Length > 0 Then finalList.Add(pending)

        _settingsService.AppSettings.NonCompressableList = finalList.Distinct().ToList
        _settingsService.SaveSettings()

        PopulateTokens()
    End Sub
End Class
