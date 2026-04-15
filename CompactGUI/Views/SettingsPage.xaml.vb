Imports System.Windows.Data

Partial Public Class SettingsPage
    Sub New(settingsviewmodel As SettingsViewModel)

        InitializeComponent()


        DataContext = settingsviewmodel


        ScrollViewer.SetCanContentScroll(Me, False)

    End Sub


    Public Property LanguageChangedLabelContent As String
        Get
            Return CType(GetValue(LanguageChangedLabelContentProperty), String)
        End Get
        Set(value As String)
            SetValue(LanguageChangedLabelContentProperty, value)
        End Set
    End Property

    Public Shared ReadOnly LanguageChangedLabelContentProperty As DependencyProperty =
        DependencyProperty.Register("SetUi_LanguageChanged", GetType(String), GetType(SettingsPage),
                                   New PropertyMetadata("Language (Requires Restart)"))

    Private Sub SettingsPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        ' Set the currently selected language
        Dim currentLanguage As String = LanguageHelper.GetCurrentLanguage()

        For i As Integer = 0 To UiLanguageComboBox.Items.Count - 1
            Dim item As LanguageItem = CType(UiLanguageComboBox.Items(i), LanguageItem)
            If item.CultureCode = currentLanguage Then
                UiLanguageComboBox.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub

    Private Sub UpdateLocalizedText()
        ' Update language tag content
        LanguageChangedLabelContent = LanguageHelper.GetString("SetUi_LanguageChanged")

    End Sub

    Private Sub UiLanguageComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim comboBox As ComboBox = CType(sender, ComboBox)
        If comboBox.IsDropDownOpen AndAlso UiLanguageComboBox.SelectedItem IsNot Nothing Then
            Dim selectedLanguage As LanguageItem = CType(UiLanguageComboBox.SelectedItem, LanguageItem)
            Dim languageCode As String = CStr(selectedLanguage.CultureCode)

            LanguageHelper.ApplyCulture(languageCode)
            'UpdateLocalizedText()

            'Dim msgBox = New Wpf.Ui.Controls.MessageBox With {
            '   .Title = LanguageHelper.GetString("SetUi_LanguageChangedTitle"),
            '   .Content = LanguageHelper.GetString("SetUi_LanguageChangedMsg"),
            '   .IsPrimaryButtonEnabled = False,
            '   .IsCloseButtonEnabled = True,
            '   .CloseButtonText = LanguageHelper.GetString("UniOK")
            '}
            'msgBox.ShowDialogAsync()
        End If
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim varx = IO.Path.Combine(Environment.GetEnvironmentVariable("IridiumIO"), "CompactGUI")
        Process.Start("explorer.exe", varx)
    End Sub
End Class