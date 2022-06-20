Public Class ImageControl : Inherits Image

    Public Shared ReadOnly SourceChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("SourceChanged", RoutingStrategy.Direct, GetType(RoutedEventHandler), GetType(ImageControl))

    Shared Sub New()
        Image.SourceProperty.OverrideMetadata(GetType(ImageControl), New FrameworkPropertyMetadata(Nothing, AddressOf SourcePropertyChanged))
    End Sub

    Public Custom Event SourceChanged As RoutedEventHandler
        AddHandler(ByVal value As RoutedEventHandler)
            Task.Delay(1000)

            [AddHandler](SourceChangedEvent, value)
        End AddHandler
        RemoveHandler(ByVal value As RoutedEventHandler)
            [RemoveHandler](SourceChangedEvent, value)
        End RemoveHandler
        RaiseEvent(sender As Object, e As RoutedEventArgs)

            [RaiseEvent](e)
        End RaiseEvent
    End Event

    Private Shared Sub SourcePropertyChanged(ByVal obj As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim image As Image = TryCast(obj, Image)
        If image IsNot Nothing Then
            image.[RaiseEvent](New RoutedEventArgs(SourceChangedEvent))
        End If
    End Sub
End Class
