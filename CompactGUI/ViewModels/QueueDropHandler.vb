Imports GongSolutions.Wpf.DragDrop
Imports System.Windows

Public NotInheritable Class QueueDropHandler
    Implements IDropTarget

    Private ReadOnly _homeViewModel As HomeViewModel

    Public Sub New(homeViewModel As HomeViewModel)
        _homeViewModel = homeViewModel
    End Sub

    Public Sub DragOver(dropInfo As IDropInfo) Implements IDropTarget.DragOver
        Dim folder = TryCast(dropInfo.Data, CompressableFolder)
        If Not _homeViewModel.CanReorderQueuedFolder(folder) Then Return

        dropInfo.Effects = DragDropEffects.Move
        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert
    End Sub

    Public Sub DragEnter(dropInfo As IDropInfo) Implements IDropTarget.DragEnter
    End Sub

    Public Sub DragLeave(dropInfo As IDropInfo) Implements IDropTarget.DragLeave
    End Sub

    Public Sub DropHint(dropHintInfo As IDropHintInfo) Implements IDropTarget.DropHint
    End Sub

    Public Sub Drop(dropInfo As IDropInfo) Implements IDropTarget.Drop
        Dim folder = TryCast(dropInfo.Data, CompressableFolder)
        If folder Is Nothing Then Return

        _homeViewModel.MoveQueuedFolder(folder, dropInfo.InsertIndex)
        _homeViewModel.SelectedFolder = folder
    End Sub
End Class
