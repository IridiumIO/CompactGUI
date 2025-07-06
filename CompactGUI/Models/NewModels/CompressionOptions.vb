Imports CommunityToolkit.Mvvm.ComponentModel

Public Class CompressionOptions : Inherits ObservableObject
    Public Property SelectedCompressionMode As Core.CompressionMode = Core.CompressionMode.XPRESS4K
    Public Property SkipPoorlyCompressedFileTypes As Boolean
    Public Property SkipUserSubmittedFiletypes As Boolean
    Public Property WatchFolderForChanges As Boolean


    Public Function Clone() As CompressionOptions
        Dim copy As New CompressionOptions()

        copy.SelectedCompressionMode = SelectedCompressionMode
        copy.SkipPoorlyCompressedFileTypes = SkipPoorlyCompressedFileTypes
        copy.SkipUserSubmittedFiletypes = SkipUserSubmittedFiletypes
        copy.WatchFolderForChanges = WatchFolderForChanges

        Return copy
    End Function

End Class
