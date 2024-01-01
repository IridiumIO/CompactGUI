Public Class Settings_skiplistflyout

    Sub New()

        InitializeComponent()

        uiTokenizedText.TokenMatcher = Function(text) If(text.EndsWith(" ") OrElse text.EndsWith(";") OrElse text.EndsWith(","), text.Substring(0, text.Length - 1).Trim(), Nothing)
        PopulateTokens()

    End Sub

    Private Sub PopulateTokens()
        uiTokenizedText.Document.Blocks.Clear()
        For Each i In SettingsHandler.AppSettings.NonCompressableList
            uiTokenizedText.InsertText(i)
        Next
    End Sub

    Private Sub uiReset_Click(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.NonCompressableList = New Settings().NonCompressableList
        SettingsHandler.AppSettings.Save()
        PopulateTokens()
    End Sub

    Private Sub uiSave_Click(sender As Object, e As RoutedEventArgs)
        Dim items = uiTokenizedText.Document.Blocks

        Dim inlineI As Paragraph = items(0)
        Dim allObj = inlineI.Inlines _
            .Where(Function(c) c.GetType = GetType(InlineUIContainer)) _
            .Select(Function(f As InlineUIContainer)
                        Dim cl As ContentPresenter = f.Child
                        Return cl.Content.ToString
                    End Function).ToList

        SettingsHandler.AppSettings.NonCompressableList = allObj.Where(Function(c) c.StartsWith(".") AndAlso c.Length > 1).Distinct().ToList
        SettingsHandler.AppSettings.Save()

        PopulateTokens()

    End Sub
End Class
