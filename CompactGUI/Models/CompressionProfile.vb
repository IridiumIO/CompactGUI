Imports CommunityToolkit.Mvvm.ComponentModel

Public Class CompressionProfile : Inherits ObservableObject
    Public Property Id As Guid = Guid.NewGuid()
    Public Property Name As String
    Public Property Description As String
    Public Property CompressionMode As Integer = 0
    Public Property SkipNonCompressable As Boolean = False
    Public Property SkipUserNonCompressable As Boolean = False
    Public Property WatchFolderForChanges As Boolean = False
    Public Property CustomSkipList As New List(Of String)
    Public Property MaxCompressionThreads As Integer = 0
    Public Property IsDefault As Boolean = False
    Public Property CreatedAt As DateTime = DateTime.Now
    Public Property LastModifiedAt As DateTime = DateTime.Now
    
    Public Function Clone() As CompressionProfile
        Return New CompressionProfile With {
            .Id = Guid.NewGuid(),
            .Name = $"{Name} (Copy)",
            .Description = Description,
            .CompressionMode = CompressionMode,
            .SkipNonCompressable = SkipNonCompressable,
            .SkipUserNonCompressable = SkipUserNonCompressable,
            .WatchFolderForChanges = WatchFolderForChanges,
            .CustomSkipList = New List(Of String)(CustomSkipList),
            .MaxCompressionThreads = MaxCompressionThreads,
            .IsDefault = False,
            .CreatedAt = DateTime.Now,
            .LastModifiedAt = DateTime.Now
        }
    End Function
    
    Public Sub ApplyToFolder(folder As CompressableFolder)
        folder.CompressionOptions.SelectedCompressionMode = CompressionMode
        folder.CompressionOptions.SkipPoorlyCompressedFileTypes = SkipNonCompressable
        folder.CompressionOptions.SkipUserSubmittedFiletypes = SkipUserNonCompressable
        folder.CompressionOptions.WatchFolderForChanges = WatchFolderForChanges
        
        ' Apply custom skip list if needed
        ' This would require modifying the CompressableFolder class to support custom skip lists
    End Sub
End Class