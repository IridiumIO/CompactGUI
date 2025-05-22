Imports System.Windows.Media.Animation

Public Class ImageControl : Inherits Image

    Public Shared ReadOnly SourceChangingEvent As RoutedEvent =
        EventManager.RegisterRoutedEvent("SourceChanging", RoutingStrategy.Direct, GetType(RoutedEventHandler), GetType(ImageControl))

    Public Shared ReadOnly SourceChangedEvent As RoutedEvent =
        EventManager.RegisterRoutedEvent("SourceChanged", RoutingStrategy.Direct, GetType(RoutedEventHandler), GetType(ImageControl))


    Public Shared ReadOnly NewSourceProperty As DependencyProperty =
        DependencyProperty.Register("NewSource", GetType(ImageSource), GetType(ImageControl),
                                    New PropertyMetadata(Nothing, AddressOf OnNewSourceChanged))

    Public Property NewSource As ImageSource
        Get
            Return CType(GetValue(NewSourceProperty), ImageSource)
        End Get
        Set(value As ImageSource)
            SetValue(NewSourceProperty, value)
        End Set
    End Property


    Private Shared Async Sub OnNewSourceChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim control = TryCast(d, ImageControl)
        If control Is Nothing Then Return

        control.RaiseEvent(New RoutedEventArgs(SourceChangingEvent))

        ' Animate fade out
        Dim fadeOut As New DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
        Await control.BeginAnimationAsync(OpacityProperty, fadeOut)

        ' Now update the actual image source
        control.Source = CType(e.NewValue, ImageSource)

        control.RaiseEvent(New RoutedEventArgs(SourceChangedEvent))
    End Sub

    Shared Sub New()
        SourceProperty.OverrideMetadata(GetType(ImageControl), New FrameworkPropertyMetadata(Nothing, AddressOf OnSourcePropertyChanged, AddressOf OnSourceCoerceValue))
    End Sub

    Public Custom Event SourceChanging As RoutedEventHandler
        AddHandler(value As RoutedEventHandler)
            [AddHandler](SourceChangingEvent, value)
        End AddHandler
        RemoveHandler(value As RoutedEventHandler)
            [RemoveHandler](SourceChangingEvent, value)
        End RemoveHandler
        RaiseEvent(sender As Object, e As RoutedEventArgs)
            [RaiseEvent](e)
        End RaiseEvent
    End Event

    Public Custom Event SourceChanged As RoutedEventHandler
        AddHandler(value As RoutedEventHandler)
            [AddHandler](SourceChangedEvent, value)
        End AddHandler
        RemoveHandler(value As RoutedEventHandler)
            [RemoveHandler](SourceChangedEvent, value)
        End RemoveHandler
        RaiseEvent(sender As Object, e As RoutedEventArgs)
            [RaiseEvent](e)
        End RaiseEvent
    End Event

    Private Shared Function OnSourceCoerceValue(d As DependencyObject, baseValue As Object) As Object
        Dim image = TryCast(d, ImageControl)
        If image IsNot Nothing Then
            image.RaiseEvent(New RoutedEventArgs(SourceChangingEvent))
        End If
        Return baseValue
    End Function

    Private Shared Sub OnSourcePropertyChanged(obj As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim image = TryCast(obj, ImageControl)
        If image IsNot Nothing Then
            image.RaiseEvent(New RoutedEventArgs(SourceChangedEvent))
        End If
    End Sub
End Class

Public Module AnimationHelper
    <Runtime.CompilerServices.Extension()>
    Public Async Function BeginAnimationAsync(target As UIElement, dp As DependencyProperty, animation As AnimationTimeline) As Task
        Dim tcs As New TaskCompletionSource(Of Boolean)()

        If animation Is Nothing Then
            tcs.SetResult(True)
            Return
        End If

        animation.FillBehavior = FillBehavior.Stop

        AddHandler animation.Completed, Sub(s, e)
                                            tcs.TrySetResult(True)
                                        End Sub

        target.BeginAnimation(dp, animation)
        Await tcs.Task
    End Function
End Module