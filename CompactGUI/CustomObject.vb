Public Class CustomObject

    Public Property Name As String

    Public Property Description As String

    Public Property ActionMode As ActionMode


    Public Sub New(name As String, desc As String)
        Me.Name = name
        Me.Description = desc
    End Sub

End Class


Public Enum ActionMode
    PendingCompression
    Compressing
    Results
    PendingUncompression
    Uncompressing
End Enum
