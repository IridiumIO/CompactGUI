Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Effects
Imports System.Windows.Shapes

Public NotInheritable Class LiveBlurBackground

    Public Shared ReadOnly EnableBlurProperty As DependencyProperty = DependencyProperty.RegisterAttached("EnableBlur", GetType(Boolean), GetType(LiveBlurBackground), New PropertyMetadata(False, AddressOf OnEnableBlurChanged))

    Public Shared ReadOnly SourceProperty As DependencyProperty = DependencyProperty.RegisterAttached("Source", GetType(Visual), GetType(LiveBlurBackground), New PropertyMetadata(Nothing, AddressOf OnAppearanceChanged))

    Public Shared ReadOnly BlurRadiusProperty As DependencyProperty = DependencyProperty.RegisterAttached("BlurRadius", GetType(Double), GetType(LiveBlurBackground), New PropertyMetadata(20.0, AddressOf OnAppearanceChanged))

    Public Shared ReadOnly BlurOpacityProperty As DependencyProperty = DependencyProperty.RegisterAttached("BlurOpacity", GetType(Double), GetType(LiveBlurBackground), New PropertyMetadata(1.0, AddressOf OnAppearanceChanged))

    Private Shared ReadOnly StateProperty As DependencyProperty = DependencyProperty.RegisterAttached("State", GetType(BlurState), GetType(LiveBlurBackground), New PropertyMetadata(Nothing))

    Public Shared Function GetEnableBlur(element As DependencyObject) As Boolean
        Return CBool(element.GetValue(EnableBlurProperty))
    End Function

    Public Shared Sub SetEnableBlur(element As DependencyObject, value As Boolean)
        element.SetValue(EnableBlurProperty, value)
    End Sub

    Public Shared Function GetSource(element As DependencyObject) As Visual
        Return TryCast(element.GetValue(SourceProperty), Visual)
    End Function

    Public Shared Sub SetSource(element As DependencyObject, value As Visual)
        element.SetValue(SourceProperty, value)
    End Sub

    Public Shared Function GetBlurRadius(element As DependencyObject) As Double
        Return CDbl(element.GetValue(BlurRadiusProperty))
    End Function

    Public Shared Sub SetBlurRadius(element As DependencyObject, value As Double)
        element.SetValue(BlurRadiusProperty, value)
    End Sub

    Public Shared Function GetBlurOpacity(element As DependencyObject) As Double
        Return CDbl(element.GetValue(BlurOpacityProperty))
    End Function

    Public Shared Sub SetBlurOpacity(element As DependencyObject, value As Double)
        element.SetValue(BlurOpacityProperty, value)
    End Sub

    Private Shared Sub OnEnableBlurChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim border = TryCast(d, Border)
        If border Is Nothing Then Return

        Dim state = TryCast(border.GetValue(StateProperty), BlurState)

        If CBool(e.NewValue) Then
            If state Is Nothing Then
                state = New BlurState(border)
                border.SetValue(StateProperty, state)
            End If

            state.Attach()
        ElseIf state IsNot Nothing Then
            state.Detach()
            border.ClearValue(StateProperty)
        End If
    End Sub

    Private Shared Sub OnAppearanceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim border = TryCast(d, Border)
        If border Is Nothing Then Return

        Dim state = TryCast(border.GetValue(StateProperty), BlurState)
        state?.Update()
    End Sub

    Private NotInheritable Class BlurState

        Private ReadOnly _border As Border

        Private _host As Grid
        Private _contentHost As Grid
        Private _blurRectangle As Rectangle
        Private _brush As VisualBrush
        Private _effect As BlurEffect
        Private _originalChild As UIElement

        Private _attached As Boolean
        Private _installed As Boolean
        Private _trackingLayout As Boolean

        Public Sub New(border As Border)
            _border = border
        End Sub

        Public Sub Attach()
            If _attached Then
                Update()
                Return
            End If

            _attached = True

            AddHandler _border.Loaded, AddressOf OnLoaded
            AddHandler _border.Unloaded, AddressOf OnUnloaded

            If _border.IsLoaded Then Install()
        End Sub

        Public Sub Detach()
            If Not _attached Then Return

            _attached = False

            RemoveHandler _border.Loaded, AddressOf OnLoaded
            RemoveHandler _border.Unloaded, AddressOf OnUnloaded

            StopTrackingLayout()

            If Not _installed Then Return

            _brush.Visual = Nothing
            _blurRectangle.Fill = Nothing
            _blurRectangle.Effect = Nothing

            If ReferenceEquals(_border.Child, _host) Then
                _border.Child = Nothing

                If _originalChild IsNot Nothing Then
                    _contentHost.Children.Remove(_originalChild)
                    _border.Child = _originalChild
                End If
            End If

            _host.Children.Clear()

            _host = Nothing
            _contentHost = Nothing
            _blurRectangle = Nothing
            _brush = Nothing
            _effect = Nothing
            _originalChild = Nothing

            _installed = False
        End Sub

        Public Sub Update()
            If Not _installed Then Return

            UpdatePaddingLayout()
            UpdateAppearance()
            UpdateClip()
            UpdateMapping()
        End Sub

        Private Sub OnLoaded(sender As Object, e As RoutedEventArgs)
            Install()
        End Sub

        Private Sub OnUnloaded(sender As Object, e As RoutedEventArgs)
            StopTrackingLayout()
        End Sub

        Private Sub Install()
            If _installed Then
                StartTrackingLayout()
                Update()
                Return
            End If

            _originalChild = _border.Child
            _border.Child = Nothing

            _brush = New VisualBrush With {
                .Stretch = Stretch.Fill,
                .AlignmentX = AlignmentX.Left,
                .AlignmentY = AlignmentY.Top,
                .ViewboxUnits = BrushMappingMode.Absolute,
                .ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                .Viewport = New Rect(0, 0, 1, 1),
                .TileMode = TileMode.None
            }

            _effect = New BlurEffect With {
                .KernelType = KernelType.Gaussian,
                .RenderingBias = RenderingBias.Performance
            }

            _blurRectangle = New Rectangle With {
                .Fill = _brush,
                .Effect = _effect,
                .IsHitTestVisible = False
            }

            _host = New Grid()
            _contentHost = New Grid()

            _host.Children.Add(_blurRectangle)
            _host.Children.Add(_contentHost)

            If _originalChild IsNot Nothing Then
                _contentHost.Children.Add(_originalChild)
            End If

            _border.Child = _host
            _installed = True

            Update()
            StartTrackingLayout()
        End Sub

        Private Sub StartTrackingLayout()
            If _trackingLayout Then Return

            AddHandler _border.LayoutUpdated, AddressOf OnLayoutUpdated
            _trackingLayout = True
        End Sub

        Private Sub StopTrackingLayout()
            If Not _trackingLayout Then Return

            RemoveHandler _border.LayoutUpdated, AddressOf OnLayoutUpdated
            _trackingLayout = False
        End Sub

        Private Sub OnLayoutUpdated(sender As Object, e As EventArgs)
            If Not _installed Then Return

            UpdatePaddingLayout()
            UpdateClip()
            UpdateMapping()
        End Sub

        Private Sub UpdatePaddingLayout()
            If _host Is Nothing OrElse _contentHost Is Nothing Then Return

            Dim padding = _border.Padding
            Dim negativePadding = New Thickness(-padding.Left, -padding.Top, -padding.Right, -padding.Bottom)

            If _host.Margin <> negativePadding Then _host.Margin = negativePadding
            If _contentHost.Margin <> padding Then _contentHost.Margin = padding

        End Sub

        Private Sub UpdateAppearance()
            If _effect Is Nothing OrElse _brush Is Nothing Then Return

            Dim radius = Math.Max(0, GetBlurRadius(_border))
            Dim blurOpacity = Math.Clamp(GetBlurOpacity(_border), 0, 1)

            If Not AreClose(_effect.Radius, radius) Then _effect.Radius = radius

            If Not AreClose(_brush.Opacity, blurOpacity) Then _brush.Opacity = blurOpacity

            'Provide buffer around the visible region so the blur does not produce transparent/ darkened edges.
            Dim overscan = Math.Ceiling(radius * 1.5)
            Dim margin = New Thickness(-overscan)

            If _blurRectangle.Margin <> margin Then _blurRectangle.Margin = margin
        End Sub

        Private Sub UpdateClip()
            If _host Is Nothing OrElse _host.ActualWidth <= 0 OrElse _host.ActualHeight <= 0 Then Return

            Dim radius = Math.Max(0, _border.CornerRadius.TopLeft)
            Dim bounds = New Rect(0, 0, _host.ActualWidth, _host.ActualHeight)
            Dim clip = TryCast(_host.Clip, RectangleGeometry)

            If clip Is Nothing Then
                _host.Clip = New RectangleGeometry(bounds, radius, radius)
            Else
                If Not RectsEqual(clip.Rect, bounds) Then clip.Rect = bounds
                If Not AreClose(clip.RadiusX, radius) Then clip.RadiusX = radius
                If Not AreClose(clip.RadiusY, radius) Then clip.RadiusY = radius
            End If
        End Sub

        Private Sub UpdateMapping()
            If _brush Is Nothing Then Return

            Dim source = GetSource(_border)

            If source Is Nothing OrElse
               _host Is Nothing OrElse
               _host.ActualWidth <= 0 OrElse
               _host.ActualHeight <= 0 Then

                _brush.Visual = Nothing
                Return
            End If

            'The source MUST NOT be an ancestor of the border or we get a circular dependency and fkn overflow
            If source.IsAncestorOf(_border) Then
                _brush.Visual = Nothing
                Return
            End If

            If Not ReferenceEquals(_brush.Visual, source) Then _brush.Visual = source

            Try
                Dim overscan = Math.Ceiling(Math.Max(0, GetBlurRadius(_border)) * 1.5)

                Dim localCaptureBounds = New Rect(-overscan, -overscan, _host.ActualWidth + overscan * 2, _host.ActualHeight + overscan * 2)

                Dim transform = _host.TransformToVisual(source)
                Dim sourceBounds = transform.TransformBounds(localCaptureBounds)

                If sourceBounds.Width <= 0 OrElse sourceBounds.Height <= 0 Then Return

                If Not RectsEqual(_brush.Viewbox, sourceBounds) Then _brush.Viewbox = sourceBounds

            Catch ex As InvalidOperationException

            End Try
        End Sub

        Private Shared Function AreClose(a As Double, b As Double) As Boolean
            Return Math.Abs(a - b) < 0.01
        End Function

        Private Shared Function RectsEqual(a As Rect, b As Rect) As Boolean
            Return AreClose(a.X, b.X) AndAlso
                   AreClose(a.Y, b.Y) AndAlso
                   AreClose(a.Width, b.Width) AndAlso
                   AreClose(a.Height, b.Height)
        End Function

    End Class

End Class