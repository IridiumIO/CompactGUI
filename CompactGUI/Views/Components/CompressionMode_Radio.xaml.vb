Public Class CompressionMode_Radio
    Inherits RadioButton


    Public Shared ReadOnly CompressionModeProperty As DependencyProperty = DependencyProperty.RegisterAttached(
        NameOf(CompressionMode),
        GetType(String),
        GetType(CompressionMode_Radio),
        New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.None))

    Public Property CompressionMode As String
        Get
            Return CType(GetValue(CompressionModeProperty), String)
        End Get
        Set(value As String)
            SetValue(CompressionModeProperty, value)
        End Set
    End Property

    Public Shared ReadOnly SavingsProperty As DependencyProperty = DependencyProperty.RegisterAttached(
        NameOf(Savings),
        GetType(Integer),
        GetType(CompressionMode_Radio),
        New FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.None))

    Public Property Savings As Integer
        Get
            Return CType(GetValue(SavingsProperty), Integer)
        End Get
        Set(value As Integer)
            SetValue(SavingsProperty, value)
        End Set
    End Property

    Public Shared ReadOnly SelectedBrushColorProperty As DependencyProperty = DependencyProperty.RegisterAttached(
        NameOf(SelectedBrushColor),
        GetType(Brush),
        GetType(CompressionMode_Radio),
        New FrameworkPropertyMetadata(Nothing, FrameworkPropertyMetadataOptions.None))

    Public Property SelectedBrushColor As Brush
        Get
            Return CType(GetValue(SelectedBrushColorProperty), Brush)
        End Get
        Set(value As Brush)
            SetValue(SelectedBrushColorProperty, value)
        End Set
    End Property


    Public Shared ReadOnly ProgressValueProperty As DependencyProperty = DependencyProperty.RegisterAttached(
        NameOf(ProgressValue),
        GetType(Integer),
        GetType(CompressionMode_Radio),
        New FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.None))

    Public Property ProgressValue As Integer
        Get
            Return CType(GetValue(ProgressValueProperty), Integer)
        End Get
        Set(value As Integer)
            SetValue(ProgressValueProperty, value)
        End Set
    End Property


    Public Shared ReadOnly EstimatedVisibilityProperty As DependencyProperty = DependencyProperty.RegisterAttached(
        NameOf(EstimatedVisibility),
        GetType(Visibility),
        GetType(CompressionMode_Radio),
        New FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.None))


    Public Property EstimatedVisibility As Visibility
        Get
            Return CType(GetValue(EstimatedVisibilityProperty), Visibility)
        End Get
        Set(value As Visibility)
            SetValue(EstimatedVisibilityProperty, value)
        End Set
    End Property

    Public Shared ReadOnly IsEstimatingProperty As DependencyProperty = DependencyProperty.RegisterAttached(
        NameOf(IsEstimating),
        GetType(Boolean),
        GetType(CompressionMode_Radio),
        New FrameworkPropertyMetadata(True, FrameworkPropertyMetadataOptions.None))

    Public Property IsEstimating As Boolean
        Get
            Return CType(GetValue(IsEstimatingProperty), Boolean)
        End Get
        Set(value As Boolean)
            SetValue(IsEstimatingProperty, value)
        End Set
    End Property


    Public Shared ReadOnly BytesSavedProperty As DependencyProperty = DependencyProperty.RegisterAttached(
        NameOf(BytesSaved),
        GetType(Long),
        GetType(CompressionMode_Radio),
        New FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.None))

    Public Property BytesSaved As Long
        Get
            Return CType(GetValue(BytesSavedProperty), Long)
        End Get
        Set(value As Long)
            SetValue(BytesSavedProperty, value)
        End Set
    End Property


    Public Shared ReadOnly BytesAfterProperty As DependencyProperty = DependencyProperty.RegisterAttached(
        NameOf(BytesAfter),
        GetType(Long),
        GetType(CompressionMode_Radio),
        New FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.None))

    Public Property BytesAfter As Long
        Get
            Return CType(GetValue(BytesAfterProperty), Long)
        End Get
        Set(value As Long)
            SetValue(BytesAfterProperty, value)
        End Set
    End Property

    ' Shared/global hover state
    Public Shared Event GlobalHoverChanged As EventHandler
    Private Shared _isAnyHovered As Boolean

    Public Shared Property IsAnyHovered As Boolean
        Get
            Return _isAnyHovered
        End Get
        Set(value As Boolean)
            If _isAnyHovered <> value Then
                _isAnyHovered = value
                RaiseEvent GlobalHoverChanged(Nothing, EventArgs.Empty)
            End If
        End Set
    End Property

    Public Shared ReadOnly IsForcedDetailedProperty As DependencyProperty =
    DependencyProperty.RegisterAttached(
        NameOf(IsForcedDetailed),
        GetType(Boolean),
        GetType(CompressionMode_Radio),
        New FrameworkPropertyMetadata(False, AddressOf OnIsForcedDetailedChanged))

    Private Shared Sub OnIsForcedDetailedChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim ctrl = TryCast(d, CompressionMode_Radio)
        If ctrl IsNot Nothing AndAlso CBool(e.NewValue) Then
            VisualStateManager.GoToState(ctrl, "MouseOver", False)
        End If
    End Sub

    Public Property IsForcedDetailed As Boolean
        Get
            Return CType(GetValue(IsForcedDetailedProperty), Boolean)
        End Get
        Set(value As Boolean)
            SetValue(IsForcedDetailedProperty, value)
        End Set
    End Property

    Public Sub New()
        InitializeComponent()
        AddHandler Me.Loaded, AddressOf OnLoaded
        AddHandler Me.Unloaded, AddressOf OnUnloaded
    End Sub

    Private Sub OnLoaded(sender As Object, e As RoutedEventArgs)
        RemoveHandler GlobalHoverChanged, AddressOf OnGlobalHoverChanged
        AddHandler GlobalHoverChanged, AddressOf OnGlobalHoverChanged
    End Sub

    Private Sub OnUnloaded(sender As Object, e As RoutedEventArgs)
        RemoveHandler GlobalHoverChanged, AddressOf OnGlobalHoverChanged
    End Sub

    Private Sub OnGlobalHoverChanged(sender As Object, e As EventArgs)
        ' Update the local dependency property to trigger visual state
        SetValue(IsGloballyHoveredProperty, IsAnyHovered)

        If IsForcedDetailed Then
            VisualStateManager.GoToState(Me, "MouseOver", False)
            Return
        End If
        VisualStateManager.GoToState(Me, If(IsAnyHovered, "MouseOver", "MouseLeave"), True)


    End Sub

    ' DependencyProperty for binding in XAML
    Public Shared ReadOnly IsGloballyHoveredProperty As DependencyProperty =
        DependencyProperty.Register("IsGloballyHovered", GetType(Boolean), GetType(CompressionMode_Radio), New PropertyMetadata(False))

    Public Property IsGloballyHovered As Boolean
        Get
            Return CType(GetValue(IsGloballyHoveredProperty), Boolean)
        End Get
        Set(value As Boolean)
            SetValue(IsGloballyHoveredProperty, value)
        End Set
    End Property

    Protected Overrides Sub OnMouseEnter(e As MouseEventArgs)
        MyBase.OnMouseEnter(e)
        IsAnyHovered = True
    End Sub

    Protected Overrides Sub OnMouseLeave(e As MouseEventArgs)
        If IsForcedDetailed Then
            VisualStateManager.GoToState(Me, "MouseOver", False)
            e.Handled = True
            Return
        End If

        MyBase.OnMouseLeave(e)

        IsAnyHovered = False
    End Sub

    Public Overrides Sub OnApplyTemplate()
        MyBase.OnApplyTemplate()
        If IsForcedDetailed Then
            VisualStateManager.GoToState(Me, "MouseOver", False)
        End If
    End Sub



End Class
