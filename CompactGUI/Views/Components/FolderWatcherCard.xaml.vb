Imports System.Windows.Media.Animation

Public Class FolderWatcherCard : Inherits UserControl
    Private currentlyExpandedBorder As Border = Nothing


    Private Sub ToggleBorderHeight(sender As Object, e As RoutedEventArgs)
        Dim border As Border = DirectCast(sender, Border)
        Dim newHeight As Double = If(border.Height = 100, 50, 100)

        Dim childSavedText = FindChild(Of TextBlock)(border, "SavedText")
        Dim childDecayedText = FindChild(Of TextBlock)(border, "DecayedText")

        Dim previousBorderChildSavedText = FindChild(Of TextBlock)(currentlyExpandedBorder, "SavedText")
        Dim previousBorderChildDecayedText = FindChild(Of TextBlock)(currentlyExpandedBorder, "DecayedText")

        If currentlyExpandedBorder Is border AndAlso border.Height = 100 AndAlso TypeOf (e) IsNot MouseButtonEventArgs Then
            ' Do nothing, keep it expanded
            Return
        End If

        If currentlyExpandedBorder IsNot Nothing AndAlso currentlyExpandedBorder IsNot border Then
            AnimateBorderHeight(currentlyExpandedBorder, 50)
            previousBorderChildSavedText.Visibility = Visibility.Collapsed
            previousBorderChildDecayedText.Visibility = Visibility.Visible

        End If
        AnimateBorderHeight(border, newHeight)
        childSavedText.Visibility = If(newHeight = 100, Visibility.Visible, Visibility.Collapsed)
        childDecayedText.Visibility = If(newHeight = 100, Visibility.Collapsed, Visibility.Visible)
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
        Storyboard.SetTargetProperty(animation, New PropertyPath(HeightProperty))

        storyboard.Children.Add(animation)
        storyboard.Begin()
    End Sub

    Public Shared Function FindChild(Of T As DependencyObject)(parent As DependencyObject, childName As String) As T
        If parent Is Nothing Then Return Nothing
        Dim foundChild As T = Nothing
        Dim childrenCount As Integer = VisualTreeHelper.GetChildrenCount(parent)
        For i As Integer = 0 To childrenCount - 1
            Dim child As DependencyObject = VisualTreeHelper.GetChild(parent, i)
            Dim childType As T = TryCast(child, T)
            If childType Is Nothing Then
                ' The child is not of the request type, so recurse down the tree
                foundChild = FindChild(Of T)(child, childName)
                If foundChild IsNot Nothing Then Exit For
            ElseIf Not String.IsNullOrEmpty(childName) Then
                Dim frameworkElement As FrameworkElement = TryCast(child, FrameworkElement)
                ' If the child has the correct name and type
                If frameworkElement IsNot Nothing AndAlso frameworkElement.Name = childName Then
                    foundChild = DirectCast(child, T)
                    Exit For
                End If
            Else
                ' Child is of the requested type but has no name, return it
                foundChild = DirectCast(child, T)
                Exit For
            End If
        Next
        Return foundChild
    End Function


End Class
