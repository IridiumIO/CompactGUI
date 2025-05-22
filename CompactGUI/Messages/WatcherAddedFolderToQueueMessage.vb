Imports CommunityToolkit.Mvvm.Messaging.Messages

Public Class WatcherAddedFolderToQueueMessage : Inherits ValueChangedMessage(Of String)

    Public Sub New(value As String)
        MyBase.New(value)
    End Sub
End Class
