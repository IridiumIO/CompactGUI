

Public Class StandardFolder : Inherits CompressableFolder

    Public Sub New(path As String)
        FolderName = path
        DisplayName = IO.Path.GetFileName(path.TrimEnd(IO.Path.DirectorySeparatorChar, IO.Path.AltDirectorySeparatorChar))

    End Sub

End Class
