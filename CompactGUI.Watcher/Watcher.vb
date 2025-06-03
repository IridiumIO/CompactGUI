Imports System.Collections.ObjectModel
Imports System.Text.Json
Imports CompactGUI.Core
Imports System.Threading
Imports CommunityToolkit.Mvvm.ComponentModel

<PropertyChanged.AddINotifyPropertyChangedInterface>
Public Class Watcher : Inherits ObservableObject
    <PropertyChanged.AlsoNotifyFor(NameOf(TotalSaved))>
    Public Property FolderMonitors As New List(Of FolderMonitor)
    <PropertyChanged.AlsoNotifyFor(NameOf(TotalSaved))>
    Public Property WatchedFolders As New ObservableCollection(Of WatchedFolder)
    <PropertyChanged.AlsoNotifyFor(NameOf(TotalSaved))>
    Public Property LastAnalysed As DateTime

    Public Shared Property IsWatchingEnabled As Boolean = True
    Public Shared Property IsBackgroundCompactingEnabled As Boolean = True

    Private ReadOnly _DataFolder As New IO.DirectoryInfo(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IridiumIO", "CompactGUI"))
    Private ReadOnly Property WatcherJSONFile As IO.FileInfo = New IO.FileInfo(IO.Path.Combine(_DataFolder.FullName, "watcher.json"))

    Public Property BGCompactor As BackgroundCompactor


    Private ReadOnly _parseWatchersSemaphore As New SemaphoreSlim(1, 1)

    Private Const LAST_SYSTEM_MODIFIED_TIME_THRESHOLD As Integer = 180 ' 3 minutes


    Private _disableCounter As Integer = 0
    Private _counterLock As New SemaphoreSlim(1, 1)

    Public Async Function DisableBackgrounding() As Task
        Await _counterLock.WaitAsync()
        Try
            _disableCounter += 1
            If _disableCounter = 1 Then
                Debug.WriteLine("Backgrounding disabled!")
                IdleDetector.Paused = True
                Await _parseWatchersSemaphore.WaitAsync()
            End If
        Finally
            _counterLock.Release()
        End Try
    End Function

    Public Async Function EnableBackgrounding() As Task
        Await _counterLock.WaitAsync()
        Try
            If _disableCounter > 0 Then
                _disableCounter -= 1
                If _disableCounter = 0 Then
                    _parseWatchersSemaphore.Release()
                    IdleDetector.Paused = False
                    Debug.WriteLine("Backgrounding enabled!")
                End If
            End If
        Finally
            _counterLock.Release()
        End Try
    End Function


    Sub New(excludedFiletypes As String())

        IdleDetector.Start()
        Debug.WriteLine("Idle Detector Started")
        AddHandler IdleDetector.IsIdle, AddressOf OnSystemIdle

        BGCompactor = New BackgroundCompactor(excludedFiletypes)

        InitializeWatchedFoldersAsync()


    End Sub

    Private Async Function InitializeWatchedFoldersAsync() As Task
        Dim initialWatchedFolders = Await GetWatchedFoldersFromJson()

        If initialWatchedFolders Is Nothing Then Return

        WatchedFolders.Clear()

        For Each folder In initialWatchedFolders.Where(Function(f) IO.Directory.Exists(f.Folder))
            WatchedFolders.Add(folder)
        Next

        FolderMonitors.AddRange(WatchedFolders.Select(Function(w) New FolderMonitor(w.Folder, w.DisplayName) With {.LastChangedDate = w.LastSystemModifiedDate}))

        UpdateRegistryBasedOnWatchedFolders()
    End Function

    Private Sub UpdateRegistryBasedOnWatchedFolders()
        Dim registryKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)

        If WatchedFolders.Count > 0 Then
            registryKey.SetValue("CompactGUI", Environment.ProcessPath & " -tray")
        Else
            registryKey.DeleteValue("CompactGUI", False)
        End If
    End Sub


    Public Sub AddOrUpdateWatched(item As WatchedFolder, Optional immediateFlushToDisk As Boolean = True)

        Dim existingItem = WatchedFolders.FirstOrDefault(Function(f) f.Folder = item.Folder)
        If existingItem Is Nothing Then
            WatchedFolders.Add(item)
            FolderMonitors.Add(New FolderMonitor(item.Folder, item.DisplayName) With {.LastChangedDate = item.LastSystemModifiedDate})
        Else
            UpdateFolderProperties(existingItem, item)
        End If
        OnPropertyChanged(NameOf(TotalSaved))
        If immediateFlushToDisk Then WriteToFile()

    End Sub

    Public Sub UpdateWatched(folder As String, ByRef analyser As Analyser, isFreshlyCompressed As Boolean, Optional immediateFlushToDisk As Boolean = True)

        Dim existingItem = WatchedFolders.FirstOrDefault(Function(f) f.Folder = folder)

        Dim existingFolderMonitor = FolderMonitors.FirstOrDefault(Function(f) f.Folder = folder)

        If existingItem IsNot Nothing AndAlso existingFolderMonitor IsNot Nothing Then

            existingItem.LastCheckedDate = DateTime.Now
            existingItem.LastCheckedSize = analyser.CompressedBytes
            existingItem.LastUncompressedSize = analyser.UncompressedBytes
            existingItem.LastSystemModifiedDate = DateTime.Now
            If analyser.FileCompressionDetailsList.Count <> 0 Then
                existingItem.CompressionLevel = analyser.FileCompressionDetailsList.Select(Function(f) f.CompressionMode).Max
            End If

            If isFreshlyCompressed Then
                existingItem.LastCompressedDate = DateTime.Now
            End If

            If isFreshlyCompressed OrElse existingItem.CompressionLevel = WOFCompressionAlgorithm.NO_COMPRESSION Then
                existingItem.LastCompressedSize = analyser.CompressedBytes
            End If

            FolderMonitors.First(Function(f) f.Folder = folder).HasTargetChanged = False
            OnPropertyChanged(NameOf(TotalSaved))
            If immediateFlushToDisk Then WriteToFile()
        End If
    End Sub

    Private Sub UpdateFolderProperties(existingItem As WatchedFolder, newItem As WatchedFolder)
        With existingItem
            .Folder = newItem.Folder
            .DisplayName = newItem.DisplayName
            .IsSteamGame = newItem.IsSteamGame
            .LastCompressedSize = newItem.LastCompressedSize
            .LastUncompressedSize = newItem.LastUncompressedSize
            .LastCompressedDate = DateTime.Now
            .LastCheckedDate = DateTime.Now
            .LastCheckedSize = newItem.LastCheckedSize
            .LastSystemModifiedDate = DateTime.Now
            .CompressionLevel = newItem.CompressionLevel
        End With
        FolderMonitors.First(Function(f) f.Folder = newItem.Folder).HasTargetChanged = False
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
        Dim retWatchedFolders = ret.Item2


        Return retWatchedFolders
    End Function


    Private Shared ReadOnly DeserializeOptions As New JsonSerializerOptions With {.IncludeFields = True}
    Private Shared ReadOnly SerializeOptions As New JsonSerializerOptions With {.IncludeFields = True, .WriteIndented = True}

    Private Shared Function DeserializeAndValidateJSON(inputjsonFile As IO.FileInfo) As (DateTime, ObservableCollection(Of WatchedFolder))
        Dim WatcherJSON = IO.File.ReadAllText(inputjsonFile.FullName)
        If WatcherJSON = "" Then WatcherJSON = "{}"

        Dim validatedResult As (DateTime, ObservableCollection(Of WatchedFolder))
        Try
            validatedResult = JsonSerializer.Deserialize(Of (DateTime, ObservableCollection(Of WatchedFolder)))(WatcherJSON, DeserializeOptions)

        Catch ex As Exception
            validatedResult = (DateTime.Now, Nothing)

        End Try

        Return validatedResult

    End Function
    Public Sub WriteToFile()

        Dim output = JsonSerializer.Serialize((LastAnalysed, WatchedFolders), SerializeOptions)
        IO.File.WriteAllText(WatcherJSONFile.FullName, output)

    End Sub



    Private _isHandlingIdle As Boolean = False

    Private Async Function OnSystemIdle() As Task
        If _isHandlingIdle Then Return
        _isHandlingIdle = True
        Try

            Debug.WriteLine("System Idle Detected")
            If Not IsWatchingEnabled Then Return

            Dim recentThresholdDate As DateTime = DateTime.Now.AddSeconds(-LAST_SYSTEM_MODIFIED_TIME_THRESHOLD)
            If FolderMonitors.Exists(Function(x) x.LastChangedDate > recentThresholdDate) Then Return

            If _parseWatchersSemaphore.CurrentCount <> 0 Then
                Await ParseWatchers()
            End If
            If _parseWatchersSemaphore.CurrentCount <> 0 AndAlso IsBackgroundCompactingEnabled Then
                Await BackgroundCompact()
            End If
        Finally

            _isHandlingIdle = False
        End Try
    End Function


    Public Async Function ParseWatchers(Optional ParseAll As Boolean = False) As Task
        Dim acquired = Await _parseWatchersSemaphore.WaitAsync(0)
        If Not acquired Then Return

        Try
            Debug.WriteLine("Background Parsing Watchers")

            Dim WatchersToCheck = If(ParseAll, FolderMonitors, FolderMonitors.Where(Function(w) w.HasTargetChanged)).ToList()

            If Not WatchersToCheck.Any() Then Return

            Dim watchersToRemove = WatchersToCheck.Where(Function(f) Not IO.Directory.Exists(f.Folder)).ToList()
            If watchersToRemove.Any() Then
                Debug.WriteLine($"Removing {watchersToRemove.Count} folders that do not exist from watcher list.")
                For Each fsWatcher In watchersToRemove
                    RemoveWatched(WatchedFolders.FirstOrDefault(Function(f) f.Folder = fsWatcher.Folder))
                Next
            End If

            For Each fsWatcher In WatchersToCheck.OrderBy(Function(f) f.DisplayName)

                Await Analyse(fsWatcher.Folder, ParseAll)
            Next

            LastAnalysed = DateTime.Now

            If WatchersToCheck.Any() Then WriteToFile()
        Finally
            _parseWatchersSemaphore.Release()
        End Try



    End Function

    Public Async Function ParseSingleWatcher(watchedFolder As WatchedFolder) As Task

        Dim acquired = Await _parseWatchersSemaphore.WaitAsync(0)
        If Not acquired Then Return

        Try
            If watchedFolder Is Nothing Then Return
            If Not IO.Directory.Exists(watchedFolder.Folder) Then
                RemoveWatched(watchedFolder)
                Return
            End If

            Await Analyse(watchedFolder.Folder, False)
            LastAnalysed = DateTime.Now
            WriteToFile()
        Finally
            _parseWatchersSemaphore.Release()
        End Try


    End Function

    Public Async Function BackgroundCompact() As Task

        Dim acquired = Await _parseWatchersSemaphore.WaitAsync(0)
        If Not acquired Then Return

        Try

            If BGCompactor.IsCompactorActive Then Return

            If Not WatchedFolders.Any(Function(f) f.DecayPercentage <> 0 AndAlso f.CompressionLevel <> WOFCompressionAlgorithm.NO_COMPRESSION) Then
                Return
            End If

            Await BGCompactor.StartCompactingAsync(WatchedFolders, FolderMonitors)
            OnPropertyChanged(NameOf(TotalSaved))
        Finally
            _parseWatchersSemaphore.Release()

        End Try

    End Function


    Public Async Function Analyse(folder As String, checkDiskModified As Boolean) As Task(Of Boolean)
        Debug.WriteLine("Background Analysing: " & folder)
        Dim analyser As New Analyser(folder)

        Dim watched = WatchedFolders.First(Function(f) f.Folder = folder)
        watched.IsWorking = True

        Dim ret = Await analyser.AnalyseFolder(Nothing)

        analyser.FileCompressionDetailsList.Clear()


        watched.LastCheckedDate = DateTime.Now
        watched.LastCheckedSize = analyser.CompressedBytes
        watched.LastUncompressedSize = analyser.UncompressedBytes

        watched.LastSystemModifiedDate = FolderMonitors.First(Function(f) f.Folder = folder).LastChangedDate

        If analyser.FileCompressionDetailsList.Count <> 0 Then
            Dim mainCompressionLVL = analyser.FileCompressionDetailsList?.Select(Function(f) f.CompressionMode).Max
            watched.CompressionLevel = If(mainCompressionLVL, WOFCompressionAlgorithm.NO_COMPRESSION)

            If checkDiskModified Then
                Dim lastDiskWriteTime = analyser.FileCompressionDetailsList.Select(Function(fl)
                                                                                       Dim finfo As New IO.FileInfo(fl.FileName)
                                                                                       Return finfo.LastWriteTime
                                                                                   End Function).OrderByDescending(Function(f) f).First

                watched.LastSystemModifiedDate = If(watched.LastSystemModifiedDate < lastDiskWriteTime, lastDiskWriteTime, watched.LastSystemModifiedDate)

            End If
        End If



        FolderMonitors.First(Function(f) f.Folder = folder).HasTargetChanged = False
        watched.IsWorking = False
        Return True

    End Function


    Public ReadOnly Property TotalSaved As Long
        Get
            Return WatchedFolders.Sum(Function(f) f.LastUncompressedSize - f.LastCheckedSize)
        End Get
    End Property



End Class


