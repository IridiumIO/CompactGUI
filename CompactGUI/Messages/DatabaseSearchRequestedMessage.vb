Imports CommunityToolkit.Mvvm.Messaging.Messages

Public NotInheritable Class DatabaseSearchRequestedMessage : Inherits ValueChangedMessage(Of Integer)

    Public Sub New(appId As Integer)
        MyBase.New(appId)
    End Sub

End Class
