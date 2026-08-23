Imports CommunityToolkit.Mvvm.ComponentModel

Public Class CompressionOptions : Inherits ObservableObject
    <ObservableProperty> Private _SelectedCompressionMode As Core.CompressionMode = Core.CompressionMode.XPRESS4K
    <ObservableProperty> Private _SkipPoorlyCompressedFileTypes As Boolean
    <ObservableProperty> Private _SkipUserSubmittedFiletypes As Boolean
    <ObservableProperty> Private _WatchFolderForChanges As Boolean
    ' Nothing = unconfigured (baseline flags apply). A non-null list (even empty) has priority.
    <ObservableProperty> Private _SkipList As List(Of String)
    ' Master toggle: when off, no skiplist source is applied.
    <ObservableProperty> Private _SkipListEnabled As Boolean = True


    Public Function Clone() As CompressionOptions
        Dim copy As New CompressionOptions With {
            .SelectedCompressionMode = SelectedCompressionMode,
            .SkipPoorlyCompressedFileTypes = SkipPoorlyCompressedFileTypes,
            .SkipUserSubmittedFiletypes = SkipUserSubmittedFiletypes,
            .WatchFolderForChanges = WatchFolderForChanges,
            .SkipList = If(SkipList Is Nothing, Nothing, New List(Of String)(SkipList)),
            .SkipListEnabled = SkipListEnabled
        }

        Return copy
    End Function

End Class
