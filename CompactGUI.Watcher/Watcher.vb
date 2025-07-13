Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.Runtime
Imports System.Text.Json
Imports System.Threading

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports CommunityToolkit.Mvvm.Messaging
Imports CommunityToolkit.Mvvm.Messaging.Messages

Imports CompactGUI.Core
Imports CompactGUI.Core.Settings
Imports CompactGUI.Logging.Watcher

Imports Microsoft.Extensions.Logging

Imports Microsoft.Extensions.Logging.Abstractions
Imports Microsoft.Win32
Imports Microsoft.Win32.Registry


Partial Public Class Watcher : Inherits ObservableRecipient : Implements IRecipient(Of PropertyChangedMessage(Of Boolean))

    Private ReadOnly _DataFolder As IO.DirectoryInfo
    Private ReadOnly _parseWatchersSemaphore As New SemaphoreSlim(1, 1)

    Private ReadOnly _logger As ILogger(Of Watcher)
    Private ReadOnly _settingsService As ISettingsService
    Private ReadOnly _idleDetector As IdleDetector

    <NotifyPropertyChangedFor(NameOf(TotalSaved))>
    <ObservableProperty> Private _LastAnalysed As DateTime
    <ObservableProperty> Private _WatchedFolders As New ObservableCollection(Of WatchedFolder)
    <ObservableProperty> Private _IsWatchingEnabled As Boolean = True
    <ObservableProperty> Private _IsBackgroundCompactingEnabled As Boolean = True
    <ObservableProperty> Private _BGCompactor As BackgroundCompactor

    Private ReadOnly Property WatcherJSONFile As IO.FileInfo
    Private ReadOnly IdleSettings As IdleSettings

    Public ReadOnly Property TotalSaved As Long
        Get
            Return WatchedFolders.Sum(Function(f) f.LastUncompressedSize - f.LastCheckedSize)
        End Get
    End Property


    Sub New(excludedFiletypes As String(), logger As ILogger(Of Watcher), settingsService As ISettingsService)
        _logger = logger
        _settingsService = settingsService
        _DataFolder = settingsService.DataFolder
        WatcherJSONFile = New IO.FileInfo(IO.Path.Combine(_DataFolder.FullName, "watcher.json"))

        IdleSettings = New IdleSettings
        _idleDetector = New IdleDetector(IdleSettings)

        WatcherLog.WatcherStarted(logger)
        IsActive = True


        _idleDetector.Start()
        AddHandler _idleDetector.IsIdle, _idleHandler
        AddHandler _idleDetector.IsNotIdle, AddressOf OnSystemNotIdle
        AddHandler WatchedFolders.CollectionChanged, AddressOf WatchedFolders_CollectionChanged


        BGCompactor = New BackgroundCompactor(excludedFiletypes, _logger)


        AddHandler BGCompactor.IsCompactingEvent, Sub(sender, isCompacting)
                                                      CancelBackgroundingCommand.NotifyCanExecuteChanged()
                                                  End Sub

        InitializeWatchedFoldersAsync()


    End Sub

    Private _idleHandler As EventHandler = AddressOf OnSystemIdle
    Private _isSystemIdle As Boolean = False

    Private Async Sub OnSystemIdle()
        _isSystemIdle = True
        WatcherLog.SystemIdleDetected(_logger)
        BGCompactor.ResumeCompacting()

        Await RunWatcher(False)

    End Sub

    <RelayCommand>
    Public Async Function RunWatcher() As Task(Of Boolean)
        Return Await RunWatcher(True)
    End Function

    Public Async Function RunWatcher(Optional runAll As Boolean = True) As Task(Of Boolean)
        RemoveHandler _idleDetector.IsIdle, _idleHandler

        For Each watcher In WatchedFolders
            watcher.PauseMonitoring()
        Next

        Try
            If Not IsWatchingEnabled Then Return False
            Dim recentThresholdDate As DateTime = DateTime.Now.AddSeconds(-IdleSettings.LastSystemModifiedTimeThresholdSeconds)
            If Not runAll AndAlso WatchedFolders.Any(Function(x) x.LastChangedDate > recentThresholdDate) Then Return False

            If _parseWatchersSemaphore.CurrentCount <> 0 Then
                Await ParseWatchers(runAll)
            End If
            If _parseWatchersSemaphore.CurrentCount <> 0 AndAlso (IsBackgroundCompactingEnabled OrElse runAll) Then
                Await BackgroundCompact(runAll)
            End If
            Return True
        Finally

            AddHandler _idleDetector.IsIdle, _idleHandler
            For Each watcher In WatchedFolders
                watcher.ResumeMonitoring()
            Next
        End Try
        Return False
    End Function



    Private Sub OnSystemNotIdle(sender As Object, e As EventArgs)
        _isSystemIdle = False
        WatcherLog.SystemNotIdle(_logger)

        BGCompactor.PauseCompacting()
    End Sub


    Private _disableCounter As Integer = 0
    Private _counterLock As New SemaphoreSlim(1, 1)

    Public Async Function DisableBackgrounding() As Task
        Await _counterLock.WaitAsync()
        Try
            _disableCounter += 1
            If _disableCounter = 1 Then
                WatcherLog.BackgroundingDisabled(_logger)
                Await _idleDetector.StopAsync()
                BGCompactor.CancelCompacting()
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
                    _idleDetector.Start()
                    WatcherLog.BackgroundingEnabled(_logger)
                End If
            End If
        Finally
            _counterLock.Release()
        End Try
    End Function



    Private Sub WatchedFolders_CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
        OnPropertyChanged(NameOf(TotalSaved))
    End Sub

    Private Async Function InitializeWatchedFoldersAsync() As Task
        Dim initialWatchedFolders = Await GetWatchedFoldersFromJson()

        If initialWatchedFolders Is Nothing Then Return

        WatchedFolders.Clear()

        For Each folder In initialWatchedFolders.Where(Function(f) IO.Directory.Exists(f.Folder))
            WatchedFolders.Add(folder)
            folder.LastChangedDate = folder.LastSystemModifiedDate
        Next

        UpdateRegistryBasedOnWatchedFolders()
    End Function

    Private Sub UpdateRegistryBasedOnWatchedFolders()
        Dim registryKey As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)

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
            item.LastChangedDate = item.LastSystemModifiedDate
        Else
            UpdateFolderProperties(existingItem, item)
        End If
        OnPropertyChanged(NameOf(TotalSaved))
        If immediateFlushToDisk Then WriteToFile()

    End Sub

    Public Async Sub UpdateWatched(folder As String, analyser As Analyser, isFreshlyCompressed As Boolean, Optional immediateFlushToDisk As Boolean = True)

        Dim existingItem = WatchedFolders.FirstOrDefault(Function(f) f.Folder = folder)

        If existingItem IsNot Nothing Then

            Dim analysedFiles = Await analyser.GetAnalysedFilesAsync(CancellationToken.None)

            existingItem.LastCheckedDate = DateTime.Now
            existingItem.LastCheckedSize = analyser.CompressedBytes
            existingItem.LastUncompressedSize = analyser.UncompressedBytes
            existingItem.LastSystemModifiedDate = DateTime.Now
            If analysedFiles?.Count <> 0 Then
                existingItem.CompressionLevel = analysedFiles.Select(Function(f) f.CompressionMode).Max
            End If

            If isFreshlyCompressed Then
                existingItem.LastCompressedDate = DateTime.Now
            End If

            If isFreshlyCompressed OrElse existingItem.CompressionLevel = WOFCompressionAlgorithm.NO_COMPRESSION Then
                existingItem.LastCompressedSize = analyser.CompressedBytes
            End If

            existingItem.HasTargetChanged = False
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
            .CompressionLevel = If(newItem.CompressionLevel <> WOFCompressionAlgorithm.NO_COMPRESSION, newItem.CompressionLevel, existingItem.CompressionLevel)
        End With
        existingItem.HasTargetChanged = False
    End Sub

    Public Async Function RemoveWatched(item As WatchedFolder, Optional writeToFile As Boolean = True) As Task

        item.Dispose()
        WatchedFolders.Remove(item)
        If writeToFile Then Await WriteToFileAsync()

    End Function


    Public Async Function DeleteWatchersWithNonExistentFolders() As Task

        For i As Integer = WatchedFolders.Count - 1 To 0 Step -1
            If Not IO.Directory.Exists(WatchedFolders(i).Folder) Then
                WatcherLog.RemovingNonexistentFolders(_logger, 1)
                Await RemoveWatched(WatchedFolders(i), False)
            End If
        Next

        Await WriteToFileAsync()

    End Function


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

    Private Function DeserializeAndValidateJSON(inputjsonFile As IO.FileInfo) As (DateTime, ObservableCollection(Of WatchedFolder))
        Dim WatcherJSON = IO.File.ReadAllText(inputjsonFile.FullName)
        If WatcherJSON = "" Then WatcherJSON = "{}"

        Dim validatedResult As (DateTime, ObservableCollection(Of WatchedFolder))
        Try
            validatedResult = JsonSerializer.Deserialize(Of (DateTime, ObservableCollection(Of WatchedFolder)))(WatcherJSON, DeserializeOptions)

            If validatedResult.Item2 IsNot Nothing Then
                For Each folder In validatedResult.Item2.Where(Function(f) IO.Directory.Exists(f.Folder))
                    folder.InitializeMonitoring()
                Next
            End If

        Catch ex As Exception
            validatedResult = (DateTime.Now, Nothing)
            WatcherLog.DeserializeWatcherJsonFailed(_logger, ex.Message)
        End Try

        Return validatedResult

    End Function
    Public Sub WriteToFile()

        Dim output = JsonSerializer.Serialize((LastAnalysed, WatchedFolders), SerializeOptions)
        IO.File.WriteAllText(WatcherJSONFile.FullName, output)

    End Sub

    Public Async Function WriteToFileAsync() As Task
        Using stream = IO.File.Open(WatcherJSONFile.FullName, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.None)
            Await JsonSerializer.SerializeAsync(stream, (LastAnalysed, WatchedFolders), SerializeOptions)
        End Using
    End Function




    Public Async Function ParseWatchers(Optional ParseAll As Boolean = False) As Task
        Dim acquired = Await _parseWatchersSemaphore.WaitAsync(0)
        If Not acquired Then Return

        Try
            WatcherLog.ParsingWatchers(_logger, ParseAll)
            Await DeleteWatchersWithNonExistentFolders()

            Dim WatchersQuery = If(ParseAll,
                    WatchedFolders,
                    WatchedFolders.Where(Function(w) w.HasTargetChanged)
                    ).OrderBy(Function(f) f.DisplayName)

            If Not WatchersQuery.Any() Then Return

            For Each fsWatcher In WatchersQuery
                WatcherLog.FolderChanged(_logger, fsWatcher.DisplayName)
                Await Analyse(fsWatcher.Folder, ParseAll)
            Next

            Await WriteToFileAsync()
            LastAnalysed = DateTime.Now
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
                Await RemoveWatched(watchedFolder)
                Return
            End If

            Await Analyse(watchedFolder.Folder, False)
            LastAnalysed = DateTime.Now
            Await WriteToFileAsync()
        Finally
            _parseWatchersSemaphore.Release()
        End Try


    End Function

    Public Async Function BackgroundCompact(Optional runAll As Boolean = False) As Task

        Dim acquired = Await _parseWatchersSemaphore.WaitAsync(0)
        If Not acquired Then Return

        Try

            If BGCompactor.IsCompactorActive Then Return

            Dim recentThresholdDate As DateTime = DateTime.Now.AddSeconds(-IdleSettings.LastSystemModifiedTimeThresholdSeconds)

            Dim foldersToCompress = WatchedFolders.
                Where(Function(folder)
                          Dim eligible = folder.DecayPercentage <> 0 AndAlso folder.CompressionLevel <> WOFCompressionAlgorithm.NO_COMPRESSION
                          Dim recentlyModified = folder.LastSystemModifiedDate > recentThresholdDate AndAlso Not runAll
                          If eligible AndAlso recentlyModified Then
                              WatcherLog.SkippingRecentlyModifiedFolder(_logger, folder.DisplayName)
                          End If
                          Return eligible AndAlso Not recentlyModified
                      End Function)

            If foldersToCompress.Any = 0 Then Return

            Await BGCompactor.StartCompactingAsync(foldersToCompress)

            OnPropertyChanged(NameOf(TotalSaved))
        Finally
            _parseWatchersSemaphore.Release()

        End Try

    End Function


    Public Async Function Analyse(folder As String, checkDiskModified As Boolean) As Task(Of Boolean)

        Using analyser As New Analyser(folder, NullLogger(Of Analyser).Instance)
            Dim watched = WatchedFolders.First(Function(f) f.Folder = folder)
            watched.IsWorking = True

            Dim analysedFiles = Await analyser.GetAnalysedFilesAsync(CancellationToken.None)

            watched.LastCheckedDate = DateTime.Now
            watched.LastCheckedSize = analyser.CompressedBytes
            watched.LastUncompressedSize = analyser.UncompressedBytes

            watched.LastSystemModifiedDate = watched.LastChangedDate

            If analysedFiles.Count <> 0 Then
                Dim mainCompressionLVL = analysedFiles?.Select(Function(f) f.CompressionMode).Max
                watched.CompressionLevel = If(mainCompressionLVL <> WOFCompressionAlgorithm.NO_COMPRESSION, mainCompressionLVL, watched.CompressionLevel)

                If checkDiskModified Then
                    Dim lastDiskWriteTime = analysedFiles.Select(Function(fl)
                                                                     Dim finfo As New IO.FileInfo(fl.FileName)
                                                                     Return finfo.LastWriteTime
                                                                 End Function).OrderByDescending(Function(f) f).First

                    watched.LastSystemModifiedDate = If(watched.LastSystemModifiedDate < lastDiskWriteTime, lastDiskWriteTime, watched.LastSystemModifiedDate)

                End If
            End If

            watched.HasTargetChanged = False
            watched.IsWorking = False
            Return True

        End Using

    End Function

    Public Sub Receive(message As PropertyChangedMessage(Of Boolean)) Implements IRecipient(Of PropertyChangedMessage(Of Boolean)).Receive
        If (message.Sender.GetType() IsNot GetType(Settings)) Then Return

        If message.PropertyName = NameOf(Settings.EnableBackgroundWatcher) Then : IsWatchingEnabled = message.NewValue
        ElseIf message.PropertyName = NameOf(Settings.EnableBackgroundAutoCompression) Then : IsBackgroundCompactingEnabled = message.NewValue
        End If


    End Sub

    <RelayCommand>
    Public Sub CancelBackgrounding()
        BGCompactor.CancelCompacting()
        CancelBackgroundingCommand.NotifyCanExecuteChanged()
    End Sub

    Public Function CanCancelBackgrounding() As Boolean
        Return BGCompactor.IsCompactorActive
    End Function

End Class


