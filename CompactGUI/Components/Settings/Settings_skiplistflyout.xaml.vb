Imports System.Collections.ObjectModel
Imports System.Collections.Specialized

Imports CompactGUI.Core.Settings

Public Class Settings_skiplistflyout

    Private _settingsService As ISettingsService
    Private _folder As CompressableFolder = Nothing
    Private _tokens As New ObservableCollection(Of String)
    Private _availableExtensions As New ObservableCollection(Of KeyValuePair(Of String, Integer))
    Private _folderExtensionCounts As Dictionary(Of String, Integer) = Nothing
    Private _editingToken As String = Nothing
    Private _editingChipBorder As Border = Nothing
    Private _originalChipBackground As Brush = Nothing
    Private _suppressTextChanged As Boolean = False
    Private _suppressCheckboxEvents As Boolean = False
    Private _suppressSidebarRefresh As Boolean = False

    Sub New(Optional folder As CompressableFolder = Nothing)

        InitializeComponent()
        _settingsService = Application.GetService(Of ISettingsService)()
        _folder = folder
        UiTokenList.ItemsSource = _tokens
        UiExtensionList.ItemsSource = _availableExtensions
        AddHandler UiTokenInput.TextChanged, AddressOf UiTokenInput_TextChanged
        AddHandler UiTokenInput.KeyDown, AddressOf UiTokenInput_KeyDown
        AddHandler UiTokenInput.LostFocus, AddressOf UiTokenInput_LostFocus
        AddHandler _tokens.CollectionChanged, AddressOf Tokens_CollectionChanged
        If _folder IsNot Nothing Then
            UiTitle.Text = $"Skip list for {_folder.DisplayName}"
            BuildFolderExtensionCounts()
            UiExtensionSidebar.Visibility = Visibility.Visible
            UiCheckboxPanel.Visibility = Visibility.Visible
            _suppressCheckboxEvents = True
            UiChkIncludeGlobal.IsChecked = _folder.CompressionOptions.SkipPoorlyCompressedFileTypes
            UiChkIncludeWiki.IsChecked = _folder.CompressionOptions.SkipUserSubmittedFiletypes
            _suppressCheckboxEvents = False
        End If
        PopulateTokens()
    End Sub

    Private Function BuildBaseline() As List(Of String)
        Dim result As New List(Of String)

        If _folder.CompressionOptions.SkipPoorlyCompressedFileTypes Then
            result.AddRange(GetGlobalBaselineItems())
        End If
        If _folder.CompressionOptions.SkipUserSubmittedFiletypes AndAlso _folder.WikiPoorlyCompressedFiles IsNot Nothing Then
            result.AddRange(_folder.WikiPoorlyCompressedFiles)
        End If

        Return result
    End Function

    Private Function GetGlobalBaselineItems() As List(Of String)
        Dim result As New List(Of String)
        For Each entry In _settingsService.AppSettings.NonCompressableList
            If entry.StartsWith("."c) Then
                ' Plain extensions: only those present in this folder are relevant.
                If _folderExtensionCounts IsNot Nothing AndAlso _folderExtensionCounts.ContainsKey(entry) Then
                    result.Add(entry)
                End If
            Else
                ' Globs/folder names can't be presence-checked wihtout performance hit so keep them as-is.
                result.Add(entry)
            End If
        Next
        Return result
    End Function

    Private Function GetWikiBaselineItems() As List(Of String)
        If _folder.WikiPoorlyCompressedFiles Is Nothing Then Return New List(Of String)
        Return New List(Of String)(_folder.WikiPoorlyCompressedFiles)
    End Function

    Private Sub BuildFolderExtensionCounts()
        _folderExtensionCounts = Nothing
        If _folder Is Nothing OrElse _folder.AnalysisResults Is Nothing Then Return

        _folderExtensionCounts = _folder.AnalysisResults.
            Select(Function(fl) System.IO.Path.GetExtension(fl.FileName)).
            Where(Function(ext) Not String.IsNullOrEmpty(ext)).
            GroupBy(Function(ext) ext, StringComparer.OrdinalIgnoreCase).
            ToDictionary(Function(g) g.Key, Function(g) g.Count(), StringComparer.OrdinalIgnoreCase)
    End Sub

    Private Sub RefreshAvailableExtensions()
        _availableExtensions.Clear()
        If _folderExtensionCounts Is Nothing Then Return

        For Each kvp In _folderExtensionCounts.
            Where(Function(entry) Not _tokens.Any(Function(t) String.Equals(t, entry.Key, StringComparison.OrdinalIgnoreCase))).
            OrderByDescending(Function(k) k.Value)
            _availableExtensions.Add(New KeyValuePair(Of String, Integer)(kvp.Key, kvp.Value))
        Next
    End Sub

    Private Sub Tokens_CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
        If _suppressSidebarRefresh Then Return
        RefreshAvailableExtensions()
    End Sub

    Private Sub UiChkIncludeGlobal_Checked(sender As Object, e As RoutedEventArgs)
        If _suppressCheckboxEvents OrElse _folder Is Nothing Then Return
        _folder.CompressionOptions.SkipPoorlyCompressedFileTypes = True
        For Each item In GetGlobalBaselineItems()
            AddToken(item)
        Next
    End Sub

    Private Sub UiChkIncludeGlobal_Unchecked(sender As Object, e As RoutedEventArgs)
        If _suppressCheckboxEvents OrElse _folder Is Nothing Then Return
        _folder.CompressionOptions.SkipPoorlyCompressedFileTypes = False

        Dim wikiItems = If(_folder.CompressionOptions.SkipUserSubmittedFiletypes, GetWikiBaselineItems(), New List(Of String))
        For Each item In GetGlobalBaselineItems()
            If Not wikiItems.Any(Function(w) String.Equals(w, item, StringComparison.OrdinalIgnoreCase)) Then
                RemoveToken(item)
            End If
        Next
    End Sub

    Private Sub UiChkIncludeWiki_Checked(sender As Object, e As RoutedEventArgs)
        If _suppressCheckboxEvents OrElse _folder Is Nothing Then Return
        _folder.CompressionOptions.SkipUserSubmittedFiletypes = True
        For Each item In GetWikiBaselineItems()
            AddToken(item)
        Next
    End Sub

    Private Sub UiChkIncludeWiki_Unchecked(sender As Object, e As RoutedEventArgs)
        If _suppressCheckboxEvents OrElse _folder Is Nothing Then Return
        _folder.CompressionOptions.SkipUserSubmittedFiletypes = False

        Dim globalItems = If(_folder.CompressionOptions.SkipPoorlyCompressedFileTypes, GetGlobalBaselineItems(), New List(Of String))
        For Each item In GetWikiBaselineItems()
            If Not globalItems.Any(Function(g) String.Equals(g, item, StringComparison.OrdinalIgnoreCase)) Then
                RemoveToken(item)
            End If
        Next
    End Sub

    Private Sub RemoveToken(value As String)
        Dim match = _tokens.FirstOrDefault(Function(t) String.Equals(t, value, StringComparison.OrdinalIgnoreCase))
        If match IsNot Nothing Then _tokens.Remove(match)
    End Sub

    Private Sub UiExtensionList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If UiExtensionList.SelectedItem Is Nothing Then Return

        Dim item = DirectCast(UiExtensionList.SelectedItem, KeyValuePair(Of String, Integer))
        UiExtensionList.SelectedItem = Nothing
        AddToken(item.Key)
    End Sub

    Private Function GetList() As List(Of String)
        Return _settingsService.AppSettings.NonCompressableList
    End Function

    Private Sub SaveList(values As List(Of String))
        If _folder Is Nothing Then
            _settingsService.AppSettings.NonCompressableList = values
            _settingsService.SaveSettings()
        Else
            _folder.CompressionOptions.SkipList = values
        End If
    End Sub

    Private Sub PopulateTokens()
        _suppressSidebarRefresh = True
        _tokens.Clear()
        If _folder Is Nothing Then
            For Each i In GetList()
                _tokens.Add(i)
            Next
        Else
            Dim source = _folder.CompressionOptions.SkipList
            If source Is Nothing Then
                ' Unconfigured: get from the baseline (per current flags).
                For Each i In BuildBaseline()
                    AddTokenSilent(i)
                Next
            Else
                ' Configured: the saved list has priority
                For Each i In source
                    AddTokenSilent(i)
                Next
            End If
        End If
        _suppressSidebarRefresh = False
        RefreshAvailableExtensions()
        UiTokenInput.Clear()
        _editingToken = Nothing
        _editingChipBorder = Nothing
        _originalChipBackground = Nothing
    End Sub

    Private Sub AddTokenSilent(value As String)
        If value.Length = 0 Then Return
        If _tokens.Any(Function(t) String.Equals(t, value, StringComparison.OrdinalIgnoreCase)) Then Return
        _tokens.Add(value)
    End Sub

    Private Sub ChipDelete_Click(sender As Object, e As RoutedEventArgs)
        Dim chip = TryCast(TryCast(sender, FrameworkElement)?.DataContext, String)
        If chip Is Nothing Then Return

        If _editingToken IsNot Nothing AndAlso String.Equals(chip, _editingToken, StringComparison.OrdinalIgnoreCase) Then
            EndEditChip()
            UiTokenInput.Clear()
        End If

        _tokens.Remove(chip)
    End Sub

    Private Sub Chip_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        Dim chip = TryCast(sender, TextBlock)
        If chip Is Nothing Then Return

        CommitInputText()

        _editingToken = chip.Text
        Dim chipGrid = TryCast(chip.Parent, Grid)
        _editingChipBorder = If(chipGrid IsNot Nothing, TryCast(chipGrid.Parent, Border), Nothing)
        If _editingChipBorder IsNot Nothing Then
            _originalChipBackground = _editingChipBorder.Background
            _editingChipBorder.Background = New SolidColorBrush(Color.FromRgb(122, 179, 216))
        End If

        _suppressTextChanged = True
        UiTokenInput.Text = chip.Text
        UiTokenInput.CaretIndex = UiTokenInput.Text.Length
        _suppressTextChanged = False
        UiTokenInput.Focus()
    End Sub

    Private Sub UiTokenInput_TextChanged(sender As Object, e As TextChangedEventArgs)
        If _suppressTextChanged Then Return

        If UiTokenInput.Text.IndexOfAny(New Char() {";"c, ","c}) >= 0 Then
            CommitInputText()
        End If
    End Sub

    Private Sub UiTokenInput_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Escape Then
            If _editingToken IsNot Nothing Then EndEditChip()
            UiTokenInput.Clear()
            e.Handled = True
            Return
        End If

        If e.Key = Key.Back AndAlso UiTokenInput.Text.Length = 0 Then
            If _editingToken IsNot Nothing Then
                EndEditChip()
            ElseIf _tokens.Count > 0 Then
                _tokens.RemoveAt(_tokens.Count - 1)
            End If
            e.Handled = True
            Return
        End If

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
            EndEditChip()

            If parts.Count = 0 OrElse (parts.Count = 1 AndAlso String.Equals(parts(0), original, StringComparison.OrdinalIgnoreCase)) Then
                UiTokenInput.Clear()
                Return
            End If

            _tokens.Remove(original)
            For Each p In parts
                AddToken(p)
            Next
            UiTokenInput.Clear()
        ElseIf parts.Count > 0 Then
            For Each p In parts
                AddToken(p)
            Next
            UiTokenInput.Clear()
        End If
    End Sub

    Private Sub EndEditChip()
        If _editingChipBorder IsNot Nothing Then
            _editingChipBorder.Background = _originalChipBackground
        End If
        _editingChipBorder = Nothing
        _originalChipBackground = Nothing
        _editingToken = Nothing
    End Sub

    Private Sub AddToken(value As String)
        If value.Length = 0 Then Return
        If _tokens.Any(Function(t) String.Equals(t, value, StringComparison.OrdinalIgnoreCase)) Then Return
        _tokens.Add(value)
        UiTokenScroller.ScrollToBottom()
    End Sub

    Private Sub UIReset_Click(sender As Object, e As RoutedEventArgs)
        If _folder Is Nothing Then
            SaveList(New Settings().NonCompressableList)
        Else
            ' Restore baseline: clear the override, back to flag-driven behavior.
            _folder.CompressionOptions.SkipList = Nothing
        End If
        PopulateTokens()
    End Sub

    Private Sub UISave_Click(sender As Object, e As RoutedEventArgs)
        Dim finalList As New List(Of String)(_tokens)

        Dim pending = UiTokenInput.Text.Trim()
        If pending.Length > 0 Then finalList.Add(pending)

        SaveList(finalList.Distinct(StringComparer.OrdinalIgnoreCase).ToList)

        PopulateTokens()

        Close()

    End Sub
End Class
