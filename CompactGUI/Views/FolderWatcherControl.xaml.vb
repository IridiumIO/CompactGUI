Imports System.Windows.Media.Animation

Public Class FolderWatcherControl
    Private currentlyExpandedBorder As Border = Nothing


    Private Sub ToggleBorderHeight(sender As Object, e As RoutedEventArgs)
        Dim border As Border = DirectCast(sender, Border)
        Dim newHeight As Double = If(border.Height = 100, 50, 100)

        If currentlyExpandedBorder Is border AndAlso border.Height = 100 AndAlso TypeOf (e) IsNot MouseButtonEventArgs Then
            ' Do nothing, keep it expanded
            Return
        End If

        If currentlyExpandedBorder IsNot Nothing AndAlso currentlyExpandedBorder IsNot border Then
            AnimateBorderHeight(currentlyExpandedBorder, 50)
        End If

        AnimateBorderHeight(border, newHeight)
        currentlyExpandedBorder = If(newHeight = 100, border, Nothing)
    End Sub

    Private Sub AnimateBorderHeight(border As Border, targetHeight As Double)
        Dim animation As New DoubleAnimation() With {
        .From = border.ActualHeight,
        .To = targetHeight,
        .Duration = TimeSpan.FromSeconds(0.2)
    }
        Dim storyboard As New Storyboard()
        Storyboard.SetTarget(animation, border)
        Storyboard.SetTargetProperty(animation, New PropertyPath(Border.HeightProperty))

        storyboard.Children.Add(animation)
        storyboard.Begin()
    End Sub

End Class
