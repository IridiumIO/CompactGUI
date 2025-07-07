Imports CommunityToolkit.Mvvm.ComponentModel

Public Class CompressionOptions : Inherits ObservableObject
    <ObservableProperty> Private _SelectedCompressionMode As Core.CompressionMode = Core.CompressionMode.XPRESS4K
    <ObservableProperty> Private _SkipPoorlyCompressedFileTypes As Boolean
    <ObservableProperty> Private _SkipUserSubmittedFiletypes As Boolean
    <ObservableProperty> Private _WatchFolderForChanges As Boolean


    Public Function Clone() As CompressionOptions
        Dim copy As New CompressionOptions With {
            .SelectedCompressionMode = SelectedCompressionMode,
            .SkipPoorlyCompressedFileTypes = SkipPoorlyCompressedFileTypes,
            .SkipUserSubmittedFiletypes = SkipUserSubmittedFiletypes,
            .WatchFolderForChanges = WatchFolderForChanges
        }

        Return copy
    End Function

End Class
