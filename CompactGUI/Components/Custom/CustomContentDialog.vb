Imports System.ComponentModel
Imports ModernWpf.Controls

Public Class CustomContentDialog : Inherits ContentDialog

    Sub New()
        AddHandler SettingsHandler.AppSettings.PropertyChanged, AddressOf ScaleTransformHandler
        Me.LayoutTransform = New ScaleTransform(SettingsHandler.AppSettings.WindowScalingFactor, SettingsHandler.AppSettings.WindowScalingFactor)
    End Sub

    Private Sub ScaleTransformHandler(sender As Object, e As PropertyChangedEventArgs)
        If e.PropertyName = NameOf(SettingsHandler.AppSettings.WindowScalingFactor) Then
            Me.LayoutTransform = New ScaleTransform(SettingsHandler.AppSettings.WindowScalingFactor, SettingsHandler.AppSettings.WindowScalingFactor)
        End If
    End Sub
End Class
