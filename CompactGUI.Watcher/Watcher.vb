Imports System.Collections.ObjectModel
Imports System.Text.Json
Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports CompactGUI.Core
Imports System.Threading
Imports System.Collections.Specialized

Public Class Watcher : Inherits ObservableObject

    Public Property Watchers As New List(Of FolderWatcher)
    Public Property WatchedFolders As New ObservableCollection(Of WatchedFolder)
    Public Property LastAnalysed As Date
    Public Property IsActive As Boolean = False

    Private ReadOnly _idledetector As New IdleDetector
    Private ReadOnly _refreshTimer As PeriodicTimer
    Private ReadOnly _refreshTimerTask As Task

    Private ReadOnly _dataFolder As New IO.DirectoryInfo(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "CompactGUI"))
    Private ReadOnly Property WatcherJSONFile As IO.FileInfo = New IO.FileInfo(IO.Path.Combine(_dataFolder.FullName, "watcher.json"))

    Sub New()

        _idledetector.Start()
        ' TODO: this fails OPTION STRICT; but no easy way to fix ATM
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
            Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "CompactGUI", Environment.ProcessPath & " -tray")
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
                .LastCompressedDate = Date.Now
                .LastCheckedDate = Date.Now
                .LastCheckedSize = item.LastCheckedSize
                .LastSystemModifiedDate = Date.Now
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

        If Not _dataFolder.Exists Then _dataFolder.Create()
        If Not WatcherJSONFile.Exists Then Await WatcherJSONFile.Create().DisposeAsync()

        Dim watcherJSON = IO.File.ReadAllText(WatcherJSONFile.FullName)
        If watcherJSON = "" Then watcherJSON = "{}"

        Dim ret = JsonSerializer.Deserialize(Of (Date, ObservableCollection(Of WatchedFolder)))(watcherJSON, New JsonSerializerOptions With {.IncludeFields = True})
        LastAnalysed = ret.Item1
        Dim watchedFolders = ret.Item2


        Return watchedFolders

    End Function

    Private Sub WriteToFile()

        Dim output = JsonSerializer.Serialize((LastAnalysed, WatchedFolders), New JsonSerializerOptions With {.IncludeFields = True})
        IO.File.WriteAllText(WatcherJSONFile.FullName, output)

    End Sub

    Private Async Sub SWTick()
        Await ParseWatchers()
    End Sub

    Public Async Function ParseWatchers(Optional parseAll As Boolean = False) As Task
        If IsActive Then Return
        IsActive = True

        Dim watchersToCheck = If(parseAll, Watchers, Watchers.Where(Function(w) w.HasTargetChanged))

        For Each fsWatcher In watchersToCheck
            Await Analyse(fsWatcher.Folder, parseAll)
        Next

        LastAnalysed = Date.Now

        WriteToFile()
        Debug.WriteLine("")


        IsActive = False
    End Function

    Private Async Function Analyse(folder As String, checkDiskModified As Boolean) As Task(Of Boolean)
        Debug.WriteLine("analysing" & folder)
        Dim analyser As New Core.Analyser(folder)

        Dim ret = Await analyser.AnalyseFolder(Nothing)

        Dim watched = WatchedFolders.First(Function(f) f.Folder = folder)

        watched.LastCheckedDate = Date.Now
        watched.LastCheckedSize = analyser.CompressedBytes
        watched.LastSystemModifiedDate = Watchers.First(Function(f) f.Folder = folder).LastChangedDate
        Dim mainCompressionLVL = analyser.FileCompressionDetailsList.Select(Function(f) f.CompressionMode).Max
        watched.CompressionLevel = mainCompressionLVL
        If checkDiskModified Then
            Dim lastDiskWriteTime = analyser.FileCompressionDetailsList.Select(Function(fl)
                                                                                   Dim finfo As New IO.FileInfo(fl.FileName)
                                                                                   Return finfo.LastWriteTime
                                                                               End Function).OrderByDescending(Function(f) f).First
            ' TODO: use or discard "istrue"
            Dim istrue = watched.LastSystemModifiedDate < lastDiskWriteTime
            watched.LastSystemModifiedDate = If(watched.LastSystemModifiedDate < lastDiskWriteTime, lastDiskWriteTime, watched.LastSystemModifiedDate)

        End If

        Watchers.First(Function(f) f.Folder = folder).HasTargetChanged = False

        Return ret

    End Function


End Class

Public Class WatchedFolder : Inherits ObservableObject

    Public Property Folder As String
    Public Property DisplayName As String
    Public Property IsSteamGame As Boolean
    Public Property LastCompressedDate As Date
    Public Property LastCompressedSize As Long
    Public Property LastUncompressedSize As Long
    Public Property LastSystemModifiedDate As Date
    Public Property LastCheckedDate As Date
    Public Property LastCheckedSize As Long
    Public Property CompressionLevel As Core.CompressionAlgorithm

    Public ReadOnly Property DecayPercentage As Double
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


