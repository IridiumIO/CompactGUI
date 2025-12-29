Imports System.Windows.Data

Partial Public Class SettingsPage
    Public Property LanguageChangedLabelContent As String
        Get
            Return CType(GetValue(LanguageChangedLabelContentProperty), String)
        End Get
        Set(value As String)
            SetValue(LanguageChangedLabelContentProperty, value)
        End Set
    End Property

    Public Shared ReadOnly LanguageChangedLabelContentProperty As DependencyProperty =
        DependencyProperty.Register("SettingsUiSettingsLanguageChangedLabel", GetType(String), GetType(SettingsPage),
                                   New PropertyMetadata("Language (Requires Restart)"))

    Private Sub SettingsPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        ' 设置当前选择的语言
        Dim currentLanguage As String = LanguageHelper.GetCurrentLanguage()

        For i As Integer = 0 To UiLanguageComboBox.Items.Count - 1
            Dim item As ComboBoxItem = CType(UiLanguageComboBox.Items(i), ComboBoxItem)
            If CStr(item.Tag) = currentLanguage Then
                UiLanguageComboBox.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub

    Private Sub UpdateLocalizedText()
        ' 更新语言标签内容
        LanguageChangedLabelContent = LanguageHelper.GetString("SettingsUiSettingsLanguageChangedLabel")

    End Sub

    Private Sub UiLanguageComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim comboBox As ComboBox = CType(sender, ComboBox)
        If comboBox.IsDropDownOpen AndAlso UiLanguageComboBox.SelectedItem IsNot Nothing Then
            Dim selectedLanguage As ComboBoxItem = CType(UiLanguageComboBox.SelectedItem, ComboBoxItem)
            Dim languageCode As String = CStr(selectedLanguage.Tag)

            LanguageHelper.ApplyCulture(languageCode)
            LanguageHelper.WriteAppConfig("language", languageCode)
            UpdateLocalizedText()

            MessageBox.Show(LanguageHelper.GetString("SettingsUiSettingsLanguageChangedMsg"),
                       LanguageHelper.GetString("SettingsUiSettingsLanguageChangeTitle"),
                       MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub
End Class