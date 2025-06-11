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

End Class
