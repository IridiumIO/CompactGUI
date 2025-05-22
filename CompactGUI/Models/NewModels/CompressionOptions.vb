Imports CommunityToolkit.Mvvm.ComponentModel

Public Class CompressionOptions : Inherits ObservableObject
    Public Property SelectedCompressionMode As Core.CompressionMode = Core.CompressionMode.XPRESS4K
    Public Property SkipPoorlyCompressedFileTypes As Boolean
    Public Property SkipUserSubmittedFiletypes As Boolean
    Public Property WatchFolderForChanges As Boolean

    Public Function GetExclusionList() As String()
        Dim exclist As String() = Array.Empty(Of String)()
        If SkipPoorlyCompressedFileTypes AndAlso SettingsHandler.AppSettings.NonCompressableList.Count <> 0 Then
            exclist = exclist.Union(SettingsHandler.AppSettings.NonCompressableList).ToArray
        End If
        'TODO: Implement Wiki stuff
        'If SkipUserSubmittedFiletypes AndAlso ActiveFolder.WikiPoorlyCompressedFiles.Count <> 0 Then
        '    exclist = exclist.Union(ActiveFolder.WikiPoorlyCompressedFiles).ToArray
        'End If

        Return exclist
    End Function


    Public Function Clone() As CompressionOptions
        Dim copy As New CompressionOptions()

        copy.SelectedCompressionMode = SelectedCompressionMode
        copy.SkipPoorlyCompressedFileTypes = SkipPoorlyCompressedFileTypes
        copy.SkipUserSubmittedFiletypes = SkipUserSubmittedFiletypes
        copy.WatchFolderForChanges = WatchFolderForChanges

        Return copy
    End Function

End Class
