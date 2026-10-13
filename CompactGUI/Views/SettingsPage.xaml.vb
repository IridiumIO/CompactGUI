Imports System.Windows.Data

Imports LazyTranslate

Partial Public Class SettingsPage
    Private ReadOnly _viewModel As SettingsViewModel

    Sub New(settingsviewmodel As SettingsViewModel)

        InitializeComponent()

        _viewModel = settingsviewmodel
        DataContext = settingsviewmodel


        ScrollViewer.SetCanContentScroll(Me, False)

    End Sub


    Private Sub SettingsPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        ' Set the currently selected language
        Dim currentLanguage = _viewModel.CurrentLanguage

        For i As Integer = 0 To UiLanguageComboBox.Items.Count - 1
            Dim item As LanguageItem = CType(UiLanguageComboBox.Items(i), LanguageItem)
            If item.CultureCode = currentLanguage Then
                UiLanguageComboBox.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub

    Private Sub UiLanguageComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim comboBox As ComboBox = CType(sender, ComboBox)
        If comboBox.IsDropDownOpen AndAlso UiLanguageComboBox.SelectedItem IsNot Nothing Then
            Dim selectedLanguage As LanguageItem = CType(UiLanguageComboBox.SelectedItem, LanguageItem)
            Dim languageCode As String = CStr(selectedLanguage.CultureCode)

            If Not _viewModel.LoadLanguage(languageCode) Then
                comboBox.SelectedItem = _viewModel.LanguageItems.FirstOrDefault(Function(item) item.CultureCode = _viewModel.CurrentLanguage)
            End If
        End If
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim varx = IO.Path.Combine(Environment.GetEnvironmentVariable("IridiumIO"), "CompactGUI")
        Process.Start("explorer.exe", varx)
    End Sub
End Class
