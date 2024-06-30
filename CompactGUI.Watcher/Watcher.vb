Imports System.Collections.ObjectModel
Imports System.Text.Json
Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports CompactGUI.Core
Imports System.Threading
Imports System.Collections.Specialized
Imports System.Runtime
Imports System.ComponentModel

<PropertyChanged.AddINotifyPropertyChangedInterface>
Public Class Watcher : Inherits ObservableObject
    <PropertyChanged.AlsoNotifyFor(NameOf(TotalSaved))>
    Public Property FolderMonitors As New List(Of FolderMonitor)
    <PropertyChanged.AlsoNotifyFor(NameOf(TotalSaved))>
    Public Property WatchedFolders As New ObservableCollection(Of WatchedFolder)
    <PropertyChanged.AlsoNotifyFor(NameOf(TotalSaved))>
    Public Property LastAnalysed As DateTime
    Public Property IsActive As Boolean = False

    Private ReadOnly _DataFolder As New IO.DirectoryInfo(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "CompactGUI"))
    Private ReadOnly Property WatcherJSONFile As IO.FileInfo = New IO.FileInfo(IO.Path.Combine(_DataFolder.FullName, "watcher.json"))

    Public Property BGCompactor As BackgroundCompactor

    Sub New(excludedFiletypes As String())

        IdleDetector.Start()
        AddHandler IdleDetector.IsIdle, AddressOf OnSystemIdle

        BGCompactor = New BackgroundCompactor(excludedFiletypes)

        InitializeWatchedFoldersAsync()


    End Sub

    Private Async Function InitializeWatchedFoldersAsync() As Task
        Dim initialWatchedFolders = Await GetWatchedFoldersFromJson()
        'WriteToFile()

        If initialWatchedFolders Is Nothing Then Return

        WatchedFolders.Clear()

        For Each folder In initialWatchedFolders.Where(Function(f) IO.Directory.Exists(f.Folder))
            WatchedFolders.Add(folder)
        Next

        FolderMonitors.AddRange(WatchedFolders.Select(Function(w) New FolderMonitor(w.Folder) With {.LastChangedDate = w.LastSystemModifiedDate}))

        UpdateRegistryBasedOnWatchedFolders()
    End Function

    Private Sub UpdateRegistryBasedOnWatchedFolders()
        Dim registryKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)

        If WatchedFolders.Count > 0 Then
            registryKey.SetValue("CompactGUI", Process.GetCurrentProcess().MainModule.FileName & " -tray")
        Else
            registryKey.DeleteValue("CompactGUI", False)
        End If
    End Sub


    Public Sub AddOrUpdateWatched(item As WatchedFolder, Optional immediateFlushToDisk As Boolean = True)

        Dim existing = WatchedFolders.FirstOrDefault(Function(f) f.Folder = item.Folder, Nothing)

        If existing Is Nothing Then
            WatchedFolders.Add(item)
            FolderMonitors.Add(New FolderMonitor(item.Folder) With {.LastChangedDate = item.LastSystemModifiedDate})

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

        Dim x = FolderMonitors.Find(Function(f) f.Folder = item.Folder)
        If x IsNot Nothing Then
            x.Dispose()
            FolderMonitors.Remove(x)
        End If

        WatchedFolders.Remove(item)
        WriteToFile()


    End Sub


    Private Async Function GetWatchedFoldersFromJson() As Task(Of ObservableCollection(Of WatchedFolder))

        If Not _DataFolder.Exists Then _DataFolder.Create()
        If Not WatcherJSONFile.Exists Then Await WatcherJSONFile.Create().DisposeAsync()

        Dim ret = DeserializeAndValidateJSON(WatcherJSONFile)
        LastAnalysed = ret.Item1
        Dim _WatchedFolders = ret.Item2


        Return _WatchedFolders
    End Function


    Private Shared Function DeserializeAndValidateJSON(inputjsonFile As IO.FileInfo) As (DateTime, ObservableCollection(Of WatchedFolder))
        Dim WatcherJSON = IO.File.ReadAllText(inputjsonFile.FullName)
        If WatcherJSON = "" Then WatcherJSON = "{}"

        Dim validatedResult As (DateTime, ObservableCollection(Of WatchedFolder))
        Try
            validatedResult = JsonSerializer.Deserialize(Of (DateTime, ObservableCollection(Of WatchedFolder)))(WatcherJSON, New JsonSerializerOptions With {.IncludeFields = True})

        Catch ex As Exception
            validatedResult = (DateTime.Now, Nothing)

        End Try

        Return validatedResult

    End Function
    Public Sub WriteToFile()

        Dim output = JsonSerializer.Serialize((LastAnalysed, WatchedFolders), New JsonSerializerOptions With {.IncludeFields = True, .WriteIndented = True})
        IO.File.WriteAllText(WatcherJSONFile.FullName, output)

    End Sub

    Private Async Sub OnSystemIdle()
        Await ParseWatchers()
        Await BackgroundCompact()
    End Sub

    Public Async Function ParseWatchers(Optional ParseAll As Boolean = False) As Task
        If IsActive Then Return
        IsActive = True

        Dim WatchersToCheck = If(ParseAll, FolderMonitors, FolderMonitors.Where(Function(w) w.HasTargetChanged = True))

        For Each fsWatcher In WatchersToCheck
            Dim ret = Await Analyse(fsWatcher.Folder, ParseAll)
        Next

        LastAnalysed = DateTime.Now

        If WatchersToCheck.Count > 0 Then WriteToFile()

        IsActive = False
    End Function


    Public Async Function BackgroundCompact() As Task

        If IsActive Then Return
        IsActive = True

        If BGCompactor.isCompactorActive Then Return

        Dim ret = Await BGCompactor.StartCompactingAsync(WatchedFolders)
        If ret Then
            IsActive = False
        End If


    End Function


    Public Async Function Analyse(folder As String, checkDiskModified As Boolean) As Task(Of Boolean)
        Debug.WriteLine("Background Analysing: " & folder)
        Dim analyser As New Core.Analyser(folder)

        Dim ret = Await analyser.AnalyseFolder(Nothing)

        Dim watched = WatchedFolders.First(Function(f) f.Folder = folder)

        watched.LastCheckedDate = DateTime.Now
        watched.LastCheckedSize = analyser.CompressedBytes
        watched.LastSystemModifiedDate = FolderMonitors.First(Function(f) f.Folder = folder).LastChangedDate
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

        FolderMonitors.First(Function(f) f.Folder = folder).HasTargetChanged = False

        Return True

    End Function


    Public ReadOnly Property TotalSaved As Long
        Get
            Return WatchedFolders.Sum(Function(f) f.LastUncompressedSize - f.LastCheckedSize)
        End Get
    End Property



End Class


