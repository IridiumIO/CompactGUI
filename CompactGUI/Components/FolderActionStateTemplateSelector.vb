Public Class FolderActionStateTemplateSelector
    Inherits DataTemplateSelector

    Public Property IdleTemplate As DataTemplate
    Public Property AnalysingTemplate As DataTemplate
    Public Property CompressingTemplate As DataTemplate
    Public Property ResultsTemplate As DataTemplate

    Public Overrides Function SelectTemplate(item As Object, container As DependencyObject) As DataTemplate
        If item Is Nothing Then Return MyBase.SelectTemplate(item, container)
        Dim action = DirectCast(item, ActionState)
        ' Dim folderVM = TryCast(item, CompressableFolder)
        Select Case action
            Case ActionState.Idle
                Return IdleTemplate
            Case ActionState.Analysing
                Return AnalysingTemplate
            Case ActionState.Working, ActionState.Paused
                Return CompressingTemplate
            Case ActionState.Results
                Return ResultsTemplate
            Case Else
                Return MyBase.SelectTemplate(item, container)
        End Select
    End Function
End Class


Public Class HomeViewStateTemplateSelector
    Inherits DataTemplateSelector

    Public Property IdleTemplate As DataTemplate
    Public Property AnalysingTemplate As DataTemplate
    Public Property CompressingTemplate As DataTemplate
    Public Property ResultsTemplate As DataTemplate

    Public Overrides Function SelectTemplate(item As Object, container As DependencyObject) As DataTemplate
        If item Is Nothing Then Return MyBase.SelectTemplate(item, container)
        Dim action = DirectCast(item, ActionState)
        Select Case action
            Case ActionState.Idle
                Return IdleTemplate
            Case ActionState.Analysing
                Return AnalysingTemplate
            Case ActionState.Working, ActionState.Paused
                Return CompressingTemplate
            Case ActionState.Results
                Return ResultsTemplate
            Case Else
                Return MyBase.SelectTemplate(item, container)
        End Select
    End Function
End Class