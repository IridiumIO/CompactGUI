Imports System.Collections.ObjectModel
Imports System.Text.Json
Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports CompactGUI.Core
Imports System.Threading
Imports System.Collections.Specialized

Public Class Watcher : Inherits ObservableObject

    Public Property Watchers As New List(Of FolderWatcher)
    Public Property WatchedFolders As New ObservableCollection(Of WatchedFolder)
    Public Property LastAnalysed As DateTime
    Public Property IsActive As Boolean = False

    Private _idledetector As New IdleDetector
    Private _refreshTimer As PeriodicTimer
    Private _refreshTimerTask As Task

    Private ReadOnly _DataFolder As New IO.DirectoryInfo(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "CompactGUI"))
    Private ReadOnly Property WatcherJSONFile As IO.FileInfo = New IO.FileInfo(IO.Path.Combine(_DataFolder.FullName, "watcher.json"))

    Sub New()

        _idledetector.Start()
        AddHandler _idledetector.IsIdle, AddressOf SWTick

        WatchedFolders = If(Task.Run(Function() GetWatchedFoldersFromJson()).GetAwaiter().GetResult(), New ObservableCollection(Of WatchedFolder))
        WriteToFile()

        If WatchedFolders Is Nothing Then Return
        For Each watched In WatchedFolders
            Watchers.Add(New FolderWatcher(watched.Folder) With {.LastChangedDate = watched.LastSystemModifiedDate})
        Next

        _refreshTimer = New PeriodicTimer(TimeSpan.FromSeconds(30))
        _refreshTimerTask = RefreshTimerDoWorkAsync()

        If WatchedFolders.Count > 0 Then
            Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "CompactGUI", Process.GetCurrentProcess().MainModule.FileName & " -tray")
        Else
            Try
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True).DeleteValue("CompactGUI")
            Catch ex As Exception
            End Try
        End If

    End Sub


    Private Async Function RefreshTimerDoWorkAsync() As Task

        While Await _refreshTimer.WaitForNextTickAsync
            Debug.WriteLine("Refreshing")
            RefreshProperties()
        End While

    End Function

    Public Sub AddOrUpdateWatched(item As WatchedFolder, Optional immediateFlushToDisk As Boolean = True)

        Dim existing = WatchedFolders.FirstOrDefault(Function(f) f.Folder = item.Folder, Nothing)

        If existing Is Nothing Then
            WatchedFolders.Add(item)
            Watchers.Add(New FolderWatcher(item.Folder) With {.LastChangedDate = item.LastSystemModifiedDate})

        Else
            With WatchedFolders(WatchedFolders.IndexOf(existing))
                .Folder = item.Folder
                .DisplayName = item.DisplayName
                .IsSteamGame = item.IsSteamGame
                .LastCompressedSize = item.LastCompressedSize
                .LastUncompressedSize = item.LastUncompressedSize
                .LastCompressedDate = DateTime.Now
                .LastCheckedDate = DateTime.Now
                .LastCheckedSize = item.LastCheckedSize
                .LastSystemModifiedDate = DateTime.Now
                .CompressionLevel = item.CompressionLevel
            End With

        End If

        If immediateFlushToDisk Then WriteToFile()

    End Sub

    Public Sub RemoveWatched(item As WatchedFolder)

        Dim x = Watchers.Find(Function(f) f.Folder = item.Folder)
        If x IsNot Nothing Then
            x.Dispose()
            Watchers.Remove(x)
        End If

        WatchedFolders.Remove(item)
        WriteToFile()


    End Sub

    Public Sub RefreshProperties()
        For Each prop In Me.GetType.GetProperties
            Me.OnPropertyChanged(prop.Name)
        Next

        For Each f In WatchedFolders
            f.RefreshProperties()
        Next

    End Sub

    Private Async Function GetWatchedFoldersFromJson() As Task(Of ObservableCollection(Of WatchedFolder))

        If Not _DataFolder.Exists Then _DataFolder.Create()
        If Not WatcherJSONFile.Exists Then Await WatcherJSONFile.Create().DisposeAsync()

        Dim WatcherJSON = IO.File.ReadAllText(WatcherJSONFile.FullName)
        If WatcherJSON = "" Then WatcherJSON = "{}"

        Dim ret = JsonSerializer.Deserialize(Of (DateTime, ObservableCollection(Of WatchedFolder)))(WatcherJSON, New JsonSerializerOptions With {.IncludeFields = True})
        LastAnalysed = ret.Item1
        Dim _WatchedFolders = ret.Item2


        Return _WatchedFolders

    End Function

    Private Sub WriteToFile()

        Dim output = JsonSerializer.Serialize((LastAnalysed, WatchedFolders), New JsonSerializerOptions With {.IncludeFields = True})
        IO.File.WriteAllText(WatcherJSONFile.FullName, output)

    End Sub

    Private Async Sub SWTick()
        Await ParseWatchers()
    End Sub

    Public Async Function ParseWatchers(Optional ParseAll As Boolean = False) As Task
        If IsActive Then Return
        IsActive = True

        Dim WatchersToCheck = If(ParseAll, Watchers, Watchers.Where(Function(w) w.HasTargetChanged = True))

        For Each fsWatcher In WatchersToCheck
            Dim ret = Await Analyse(fsWatcher.Folder, ParseAll)
        Next

        LastAnalysed = DateTime.Now

        WriteToFile()
        Debug.WriteLine("")


        IsActive = False
    End Function

    Private Async Function Analyse(folder As String, checkDiskModified As Boolean) As Task(Of Boolean)
        Debug.WriteLine("analysing" & folder)
        Dim analyser As New Core.Analyser(folder)

        Dim ret = Await analyser.AnalyseFolder(Nothing)

        Dim watched = WatchedFolders.First(Function(f) f.Folder = folder)

        watched.LastCheckedDate = DateTime.Now
        watched.LastCheckedSize = analyser.CompressedBytes
        watched.LastSystemModifiedDate = Watchers.First(Function(f) f.Folder = folder).LastChangedDate
        Dim mainCompressionLVL = analyser.FileCompressionDetailsList.Select(Function(f) f.CompressionMode).Max
        watched.CompressionLevel = mainCompressionLVL
        If checkDiskModified Then
            Dim lastDiskWriteTime = analyser.FileCompressionDetailsList.Select(Function(fl)
                                                                                   Dim finfo As New IO.FileInfo(fl.FileName)
                                                                                   Return finfo.LastWriteTime
                                                                               End Function).OrderByDescending(Function(f) f).First
            Dim istrue = watched.LastSystemModifiedDate < lastDiskWriteTime
            watched.LastSystemModifiedDate = If(watched.LastSystemModifiedDate < lastDiskWriteTime, lastDiskWriteTime, watched.LastSystemModifiedDate)

        End If

        Watchers.First(Function(f) f.Folder = folder).HasTargetChanged = False

        Return True

    End Function


End Class

Public Class WatchedFolder : Inherits ObservableObject

    Public Property Folder As String
    Public Property DisplayName As String
    Public Property IsSteamGame As Boolean
    Public Property LastCompressedDate As DateTime
    Public Property LastCompressedSize As Long
    Public Property LastUncompressedSize As Long
    Public Property LastSystemModifiedDate As DateTime
    Public Property LastCheckedDate As DateTime
    Public Property LastCheckedSize As Long
    Public Property CompressionLevel As Core.CompressionAlgorithm

    Public ReadOnly Property DecayPercentage As Decimal
        Get
            If LastCompressedSize = 0 Then Return 1
            Return Math.Clamp((LastCheckedSize - LastCompressedSize) / (LastUncompressedSize - LastCompressedSize), 0, 1)
        End Get
    End Property

    Public Sub RefreshProperties()
        For Each prop In Me.GetType.GetProperties
            Me.OnPropertyChanged(prop.Name)
        Next
    End Sub

End Class


