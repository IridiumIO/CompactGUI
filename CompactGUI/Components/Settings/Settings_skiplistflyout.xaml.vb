Public Class Settings_skiplistflyout

    Sub New()

        InitializeComponent()
        'Me.MainGrid.LayoutTransform = New ScaleTransform(SettingsHandler.AppSettings.WindowScalingFactor, SettingsHandler.AppSettings.WindowScalingFactor)
        'Me.MainGrid.Margin = New Thickness(20 * SettingsHandler.AppSettings.WindowScalingFactor)
        'Me.Width = 500 * SettingsHandler.AppSettings.WindowScalingFactor
        'Me.Height = 400 * SettingsHandler.AppSettings.WindowScalingFactor
        UiTokenizedText.TokenMatcher = Function(text) If(text.EndsWith(" "c) OrElse text.EndsWith(";"c) OrElse text.EndsWith(","c), text.Substring(0, text.Length - 1).Trim(), Nothing)
        PopulateTokens()

    End Sub

    Private Sub PopulateTokens()
        UiTokenizedText.Document.Blocks.Clear()
        For Each i In SettingsHandler.AppSettings.NonCompressableList
            UiTokenizedText.InsertText(i)
        Next
    End Sub

    Private Sub UIReset_Click(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.NonCompressableList = New Settings().NonCompressableList
        Settings.Save()
        PopulateTokens()
    End Sub

    Private Sub UISave_Click(sender As Object, e As RoutedEventArgs)
        Dim items = UiTokenizedText.Document.Blocks

        Dim inlineI As Paragraph = items(0)
        Dim allObj = inlineI.Inlines _
            .Where(Function(c) c.GetType = GetType(InlineUIContainer)) _
            .Select(Function(f As InlineUIContainer)
                        Dim cl As ContentPresenter = f.Child
                        Return cl.Content.ToString
                    End Function).ToList

        SettingsHandler.AppSettings.NonCompressableList = allObj.Where(Function(c) c.StartsWith("."c) AndAlso c.Length > 1).Distinct().ToList
        Settings.Save()

        PopulateTokens()

    End Sub
End Class
