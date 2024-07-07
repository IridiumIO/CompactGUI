Imports System.IO
Imports System.Threading

Public Class FolderMonitor : Implements IDisposable

    Private WithEvents FSWatcher As FileSystemWatcher
    Private disposedValue As Boolean
    Public Property Folder As String
    Public Property DisplayName As String
    Public Property HasTargetChanged As Boolean = False
    Public Property LastChangedDate As DateTime

    Private debounceTimer As Timer

    Sub New(_folder As String, _displayName As String)
        Folder = _folder
        DisplayName = _displayName
        FSWatcher = New FileSystemWatcher(Folder) With {
            .NotifyFilter = NotifyFilters.Size Or NotifyFilters.CreationTime Or NotifyFilters.LastWrite Or NotifyFilters.FileName,
            .IncludeSubdirectories = True,
            .Filter = "",
            .EnableRaisingEvents = True
        }

        debounceTimer = New Timer(AddressOf DebounceTimerCallback, Nothing, Timeout.Infinite, Timeout.Infinite)
    End Sub


    Private Sub WatcherErrorEvent(sender As Object, e As ErrorEventArgs) Handles FSWatcher.Error
        Debug.WriteLine(e.GetException.Message)
    End Sub


    Private Sub WatcherModifiedEvent(sender As Object, e As FileSystemEventArgs) Handles FSWatcher.Created, FSWatcher.Changed, FSWatcher.Renamed, FSWatcher.Deleted
        debounceTimer.Change(1000, Timeout.Infinite)
    End Sub

    Private Sub DebounceTimerCallback(state As Object)
        ' This method is called after the specified debounce time has passed without any further events
        HasTargetChanged = True
        LastChangedDate = DateTime.Now
        Debug.WriteLine("Folder " & Folder & " has changed!")
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                FSWatcher?.Dispose()
                debounceTimer?.Dispose()
            End If
            disposedValue = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class

