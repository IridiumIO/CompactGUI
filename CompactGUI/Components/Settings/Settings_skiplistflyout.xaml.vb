Public Class Settings_skiplistflyout
    Public Sub New()

        InitializeComponent()

        uiTokenizedText.TokenMatcher = Function(text) If(text.EndsWith(" "), text.Substring(0, text.Length - 1).Trim(), Nothing)
        PopulateTokens()

    End Sub

    Private Sub PopulateTokens()
        uiTokenizedText.Document.Blocks.Clear()
        For Each i In SettingsHandler.AppSettings.NonCompressableList
            uiTokenizedText.InsertText(i)
        Next
    End Sub

    Private Sub UiReset_Click(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.NonCompressableList = New Settings().NonCompressableList
        Settings.Save()
        PopulateTokens()
    End Sub

    Private Sub UiSave_Click(sender As Object, e As RoutedEventArgs)
        Dim items = uiTokenizedText.Document.Blocks

        Dim inlineI As Paragraph = CType(items(0), Paragraph)
        Dim allObj As List(Of String) = inlineI.Inlines _
            .Where(Function(c) c.GetType = GetType(InlineUIContainer)) _
            .Select(CType(Function(f As InlineUIContainer)
                              Dim cl As ContentPresenter = CType(f.Child, ContentPresenter)
                              Return cl.Content.ToString
                          End Function, Func(Of Inline, String))).ToList

        SettingsHandler.AppSettings.NonCompressableList = allObj.Where(Function(c) c.StartsWith(".") AndAlso c.Length > 1).Distinct().ToList
        Settings.Save()

        PopulateTokens()

    End Sub
End Class
