Imports System.IO
Imports System.Text.Json.Serialization
Imports System.Threading

Imports CommunityToolkit.Mvvm.ComponentModel

Public Class WatchedFolder
    Inherits ObservableObject
    Implements IDisposable

    ' --- Folder Metadata ---
    <ObservableProperty> Private _Folder As String
    <ObservableProperty> Private _DisplayName As String
    <ObservableProperty> Private _IsSteamGame As Boolean
    <ObservableProperty> Private _LastCompressedDate As DateTime
    <ObservableProperty> Private _LastCompressedSize As Long
    <ObservableProperty> Private _LastUncompressedSize As Long
    <ObservableProperty> Private _LastSystemModifiedDate As DateTime
    <ObservableProperty> Private _LastCheckedDate As DateTime
    <ObservableProperty> Private _LastCheckedSize As Long
    <ObservableProperty> Private _CompressionLevel As Core.WOFCompressionAlgorithm

    <AttachAttribute(GetType(JsonIgnoreAttribute))>
    <ObservableProperty> Private _IsWorking As Boolean

    <AttachAttribute(GetType(JsonIgnoreAttribute))>
    <ObservableProperty> Private _IsEditing As Boolean = False

    ' --- Monitoring State ---
    <AttachAttribute(GetType(JsonIgnoreAttribute))>
    <ObservableProperty> Private _HasTargetChanged As Boolean = False

    <AttachAttribute(GetType(JsonIgnoreAttribute))>
    <ObservableProperty> Private _LastChangedDate As DateTime

    ' --- FileSystemWatcher ---

    Private WithEvents FSWatcher As FileSystemWatcher

    Private debounceTimer As Timer

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