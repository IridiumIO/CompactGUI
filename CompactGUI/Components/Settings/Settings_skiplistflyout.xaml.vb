Imports CompactGUI.Core.Settings

Public Class Settings_skiplistflyout

    Private _settingsService As ISettingsService

    Sub New()

        InitializeComponent()
        _settingsService = Application.GetService(Of ISettingsService)()
        UiTokenizedText.TokenMatcher = Function(text) If(text.EndsWith(" "c) OrElse text.EndsWith(";"c) OrElse text.EndsWith(","c), text.Substring(0, text.Length - 1).Trim(), Nothing)
        PopulateTokens()
    End Sub

    Private Sub PopulateTokens()
        UiTokenizedText.Document.Blocks.Clear()
        For Each i In _settingsService.AppSettings.NonCompressableList
            UiTokenizedText.InsertText(i)
        Next
    End Sub

    Private Sub UIReset_Click(sender As Object, e As RoutedEventArgs)
        _settingsService.AppSettings.NonCompressableList = New Settings().NonCompressableList
        _settingsService.SaveSettings()
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

        _settingsService.AppSettings.NonCompressableList = allObj.Where(Function(c) c.StartsWith("."c) AndAlso c.Length > 1).Distinct().ToList
        _settingsService.SaveSettings()

        PopulateTokens()

    End Sub
End Class
