Imports System.IO

Public Class FolderWatcher : Implements IDisposable

    Private WithEvents FSWatcher As FileSystemWatcher
    Private disposedValue As Boolean
    Public Property Folder As String
    Public Property HasTargetChanged As Boolean = False
    Public Property LastChangedDate As DateTime

    Sub New(_folder As String)
        Folder = _folder
        FSWatcher = New FileSystemWatcher(Folder)
        FSWatcher.NotifyFilter = NotifyFilters.Size Or NotifyFilters.CreationTime Or NotifyFilters.LastWrite Or NotifyFilters.FileName
        FSWatcher.IncludeSubdirectories = True
        FSWatcher.Filter = ""
        FSWatcher.EnableRaisingEvents = True

    End Sub


    Private Sub WatcherErrorEvent(sender As Object, e As ErrorEventArgs) Handles FSWatcher.Error
        Debug.WriteLine(e.GetException.Message)
    End Sub


    Private Sub WatcherModifiedEvent(sender As Object, e As FileSystemEventArgs) Handles FSWatcher.Created, FSWatcher.Changed, FSWatcher.Renamed, FSWatcher.Deleted
        If Not HasTargetChanged Then Debug.WriteLine($"{Folder} has been modified")
        HasTargetChanged = True
        LastChangedDate = DateTime.Now
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                FSWatcher.Dispose()

            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ' TODO: set large fields to null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
    ' Protected Overrides Sub Finalize()
    '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class

