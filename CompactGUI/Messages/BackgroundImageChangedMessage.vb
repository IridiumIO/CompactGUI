Imports CommunityToolkit.Mvvm.Messaging.Messages

Public Class BackgroundImageChangedMessage : Inherits ValueChangedMessage(Of BitmapImage)
    Public Sub New(value As BitmapImage)
        MyBase.New(value)
    End Sub
End Class
