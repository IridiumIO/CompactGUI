Imports Microsoft.Xaml.Behaviors

Public Class FocusOnMouseOverBehavior : Inherits Behavior(Of ComboBox)

    Protected Overrides Sub OnAttached()
        MyBase.OnAttached()
        AddHandler AssociatedObject.MouseEnter, AddressOf OnMouseEnter
    End Sub
    Protected Overrides Sub OnDetaching()
        MyBase.OnDetaching()
        RemoveHandler AssociatedObject.MouseEnter, AddressOf OnMouseEnter
    End Sub
    Private Sub OnMouseEnter(sender As Object, e As MouseEventArgs)
        AssociatedObject.Focus()
    End Sub


End Class
