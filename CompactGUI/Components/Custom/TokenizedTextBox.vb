'Credit: https://blog.pixelingene.com/2010/10/tokenizing-control-convert-text-to-tokens

Public Class TokenizedTextBox : Inherits RichTextBox

    Public Shared ReadOnly TokenTemplateProperty As DependencyProperty = DependencyProperty.Register("TokenTemplate", GetType(DataTemplate), GetType(TokenizedTextBox))
    Public Property TokenTemplate As DataTemplate
        Get
            Return GetValue(TokenTemplateProperty)
        End Get
        Set(value As DataTemplate)
            SetValue(TokenTemplateProperty, value)
        End Set
    End Property

    Public Property TokenMatcher As Func(Of String, Object)

    Public Sub New()

        AddHandler TextChanged, AddressOf OnTokenTextChanged

    End Sub

    Private Sub OnTokenTextChanged(sender As Object, e As TextChangedEventArgs)
        Dim text = CaretPosition.GetTextInRun(LogicalDirection.Backward)
        If TokenMatcher Is Nothing Then Return

        Dim token = TokenMatcher(text)
        If token IsNot Nothing Then
            ReplaceTextWithToken(text, token)
        End If

    End Sub

    Public Sub InsertText(text As String)
        text &= " "
        AppendText(text)
        If TokenMatcher Is Nothing Then Return
        Dim token = TokenMatcher(text)
        If token IsNot Nothing Then
            ReplaceTextWithToken(text, token)
        End If
    End Sub

    Private Sub ReplaceTextWithToken(inputText As String, token As Object)

        RemoveHandler TextChanged, AddressOf OnTokenTextChanged

        Dim para = CaretPosition.Paragraph

        Dim matchedRun As Run = para.Inlines.FirstOrDefault(Function(inline)
                                                                Dim run = TryCast(inline, Run)
                                                                Return (run IsNot Nothing AndAlso run.Text.EndsWith(inputText))
                                                            End Function)

        If matchedRun IsNot Nothing Then

            Dim tokenContainer = CreateTokenContainer(inputText, token)
            para.Inlines.InsertBefore(matchedRun, tokenContainer)

            If matchedRun.Text = inputText Then
                para.Inlines.Remove(matchedRun)
            Else
                Dim index = matchedRun.Text.IndexOf(inputText) + inputText.Length
                Dim tailEnd = New Run(matchedRun.Text.Substring(index))
                para.Inlines.InsertAfter(matchedRun, tailEnd)
                para.Inlines.Remove(matchedRun)
            End If

        End If

        AddHandler TextChanged, AddressOf OnTokenTextChanged

    End Sub

    Private Function CreateTokenContainer(inputText As String, token As Object) As InlineUIContainer
        Dim presenter = New ContentPresenter() With {
            .Content = token,
            .ContentTemplate = TokenTemplate
        }

        Return New InlineUIContainer(presenter) With {.BaselineAlignment = BaselineAlignment.Bottom}

    End Function
End Class
