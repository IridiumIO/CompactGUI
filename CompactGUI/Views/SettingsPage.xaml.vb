Public Class SettingsPage
    Private _isInitialized As Boolean = False

    Sub New(settingsviewmodel As SettingsViewModel)

        InitializeComponent()


        DataContext = settingsviewmodel


        ScrollViewer.SetCanContentScroll(Me, False)

        ' Initialize Language Selection
        Dim currentLang = LanguageConfig.GetLanguage()
        
        For i As Integer = 0 To UiLanguageComboBox.Items.Count - 1
            Dim item = CType(UiLanguageComboBox.Items(i), ComboBoxItem)
            If item.Tag.ToString() = currentLang Then
                UiLanguageComboBox.SelectedIndex = i
                Exit For
            End If
        Next
        
        ' Fallback to English if no match
        If UiLanguageComboBox.SelectedIndex = -1 Then
             UiLanguageComboBox.SelectedIndex = 0
        End If

        _isInitialized = True

    End Sub

    Private Sub UiLanguageComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not _isInitialized Then Return
        If UiLanguageComboBox.SelectedItem Is Nothing Then Return

        Dim selectedItem = CType(UiLanguageComboBox.SelectedItem, ComboBoxItem)
        Dim langCode = selectedItem.Tag.ToString()

        ' Avoid triggering on initial load if it matches
        Dim currentSaved = LanguageConfig.GetLanguage()
        If langCode <> currentSaved Then
            LanguageConfig.SaveLanguage(langCode)
            MessageBox.Show("Language settings saved. Please restart CompactGUI to apply changes." & vbCrLf & "语言设置已保存，请重启软件以生效。", "CompactGUI", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

End Class