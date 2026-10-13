Imports CommunityToolkit.Mvvm.Messaging.Messages

Public Class SteamGamesAddedToQueueMessage : Inherits ValueChangedMessage(Of IReadOnlyList(Of SteamQueueItem))
    Public Sub New(value As IReadOnlyList(Of SteamQueueItem))
        MyBase.New(value)
    End Sub
End Class

Public Class SteamQueueItem
    Public ReadOnly Property FolderPath As String
    Public ReadOnly Property CompressionOptions As CompressionOptions

    Public Sub New(folderPath As String, compressionOptions As CompressionOptions)
        Me.FolderPath = folderPath
        Me.CompressionOptions = compressionOptions
    End Sub
End Class
