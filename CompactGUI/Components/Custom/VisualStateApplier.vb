Public Class VisualStateApplier
    Public Shared Function GetVisualState(ByVal target As DependencyObject) As String
        Dim stateEnum = target.GetValue(VisualStateProperty)
        If TypeOf stateEnum Is MainViewModel.UIState Then Return CType(stateEnum, MainViewModel.UIState).ToString()
        Return Nothing
    End Function

    Public Shared Sub SetVisualState(ByVal target As DependencyObject, ByVal value As String)
        Dim stateEnum As MainViewModel.UIState
        If [Enum].TryParse(value, True, stateEnum) Then target.SetValue(VisualStateProperty, stateEnum)
    End Sub

    Public Shared ReadOnly VisualStateProperty As DependencyProperty = DependencyProperty.RegisterAttached(
        "VisualState",
        GetType(MainViewModel.UIState),
        GetType(VisualStateApplier),
        New PropertyMetadata(MainViewModel.UIState.UINothing, AddressOf VisualStatePropertyChangedCallback)
    )

    Private Shared Sub VisualStatePropertyChangedCallback(ByVal target As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        If TypeOf args.NewValue IsNot MainViewModel.UIState Then Return
        Dim newState = CType(args.NewValue, MainViewModel.UIState).ToString()
        VisualStateManager.GoToElementState(CType(target, FrameworkElement), newState, True)
    End Sub
End Class