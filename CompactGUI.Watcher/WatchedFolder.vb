Imports System.IO
Imports System.Text.Json.Serialization
Imports System.Threading

Imports CommunityToolkit.Mvvm.ComponentModel

<PropertyChanged.AddINotifyPropertyChangedInterface>
Public Class WatchedFolder
    Inherits ObservableObject
    Implements IDisposable

    ' --- Folder Metadata ---
    Public Property Folder As String
    Public Property DisplayName As String
    Public Property IsSteamGame As Boolean
    Public Property LastCompressedDate As DateTime
    Public Property LastCompressedSize As Long
    Public Property LastUncompressedSize As Long
    Public Property LastSystemModifiedDate As DateTime
    Public Property LastCheckedDate As DateTime
    Public Property LastCheckedSize As Long
    Public Property CompressionLevel As Core.WOFCompressionAlgorithm

    <JsonIgnore>
    Public Property IsWorking As Boolean
    <JsonIgnore>
    Public Property IsEditing As Boolean = False

    ' --- Monitoring State ---
    <JsonIgnore>
    Public Property HasTargetChanged As Boolean = False
    <JsonIgnore>
    Public Property LastChangedDate As DateTime

    ' --- FileSystemWatcher ---
    <JsonIgnore>
    Private WithEvents FSWatcher As FileSystemWatcher
    <JsonIgnore>
    Private debounceTimer As Timer
    <JsonIgnore>
    Private disposedValue As Boolean

    Public Sub New(_folder As String, _displayName As String)
        Folder = _folder
        DisplayName = _displayName

        InitializeMonitoring()
    End Sub

    Public Sub New()
    End Sub

    Public Sub InitializeMonitoring()
        If FSWatcher Is Nothing Then
            FSWatcher = New FileSystemWatcher(Folder) With {
            .NotifyFilter = NotifyFilters.Size Or NotifyFilters.CreationTime Or NotifyFilters.LastWrite Or NotifyFilters.FileName,
            .IncludeSubdirectories = True,
            .Filter = "",
            .EnableRaisingEvents = True
        }
            debounceTimer = New Timer(AddressOf DebounceTimerCallback, Nothing, Timeout.Infinite, Timeout.Infinite)
        End If
    End Sub


    ' --- Monitoring Events ---
    Private Sub WatcherErrorEvent(sender As Object, e As ErrorEventArgs) Handles FSWatcher.Error
        Debug.WriteLine(e.GetException.Message)
    End Sub

    Private Sub WatcherModifiedEvent(sender As Object, e As FileSystemEventArgs) Handles FSWatcher.Created, FSWatcher.Changed, FSWatcher.Renamed, FSWatcher.Deleted
        debounceTimer.Change(1000, Timeout.Infinite)
    End Sub

    Private Sub DebounceTimerCallback(state As Object)
        HasTargetChanged = True
        LastChangedDate = DateTime.Now
        OnPropertyChanged(NameOf(HasTargetChanged))
        OnPropertyChanged(NameOf(LastChangedDate))
        Debug.WriteLine("Folder " & Folder & " has changed!")
    End Sub

    ' --- Calculated Properties ---
    Public ReadOnly Property DecayPercentage As Decimal
        Get
            If LastCompressedSize = 0 Then Return 1
            Return If(LastUncompressedSize = LastCompressedSize OrElse LastCompressedSize > LastUncompressedSize, 1D, Math.Clamp((LastCheckedSize - LastCompressedSize) / (LastUncompressedSize - LastCompressedSize), 0, 1))
        End Get
    End Property

    <JsonIgnore>
    Public ReadOnly Property SavedSpace As Long
        Get
            Return LastUncompressedSize - LastCheckedSize
        End Get
    End Property

    Public Sub RefreshProperties()
        For Each prop In Me.GetType.GetProperties
            OnPropertyChanged(prop.Name)
        Next
    End Sub

    ' --- IDisposable ---
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