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


Partial Public Class Watcher : Inherits ObservableRecipient : Implements IRecipient(Of PropertyChangedMessage(Of Boolean)), IRecipient(Of PropertyChangedMessage(Of BackgroundMode))

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


    Sub New(logger As ILogger(Of Watcher), settingsService As ISettingsService, idleDetector As IdleDetector)
        _logger = logger
        _settingsService = settingsService
        _DataFolder = settingsService.DataFolder

        WatcherJSONFile = New IO.FileInfo(IO.Path.Combine(_DataFolder.FullName, "watcher.json"))

        IdleSettings = New IdleSettings
        _idleDetector = idleDetector
        WatcherLog.WatcherStarted(logger)
        IsActive = True

        AddHandler _idleDetector.IsIdle, _idleHandler
        AddHandler _idleDetector.IsNotIdle, AddressOf OnSystemNotIdle
        AddHandler WatchedFolders.CollectionChanged, AddressOf WatchedFolders_CollectionChanged


        BGCompactor = New BackgroundCompactor(Array.Empty(Of String), _logger)


        InitializeWatchedFoldersAsync()


    End Sub

    Private _idleHandler As EventHandler = AddressOf OnSystemIdle
    Private _isSystemIdle As Boolean = False

    Private Async Sub OnSystemIdle()
        If Not _isSystemIdle Then WatcherLog.SystemIdleDetected(_logger)
        _isSystemIdle = True

        'Skip idle analysis if the background mode is not set to IdleOnly
        Dim bgMode = _settingsService.AppSettings.BackgroundModeSelection
        If bgMode <> BackgroundMode.IdleOnly Then Return

        BGCompactor.ResumeCompacting()
        If IsRunning Then Return

        Await RunWatcher(False)

    End Sub



    <ObservableProperty> Private _isRunning As Boolean = False

    Public Async Function RunWatcher(Optional runAll As Boolean = True, Optional cToken As CancellationToken = Nothing) As Task(Of Boolean)
        IsRunning = True
        Try
            For Each watcher In WatchedFolders
                watcher.PauseMonitoring()
            Next

            _settingsService.AppSettings.ScheduledBackgroundLastRan = DateTime.Now
            If Not IsWatchingEnabled Then Return False
            Dim recentThresholdDate As DateTime = DateTime.Now.AddSeconds(-IdleSettings.LastSystemModifiedTimeThresholdSeconds)
            If Not runAll AndAlso WatchedFolders.Any(Function(x) x.LastChangedDate > recentThresholdDate) Then Return False

            If _parseWatchersSemaphore.CurrentCount <> 0 Then
                Await ParseWatchers(runAll, cToken)
            End If
            If cToken <> Nothing AndAlso cToken.IsCancellationRequested Then
                _logger.LogInformation("Watcher run cancelled by user.")
                Return False
            End If
            If _parseWatchersSemaphore.CurrentCount <> 0 AndAlso (IsBackgroundCompactingEnabled OrElse runAll) Then
                Await BackgroundCompact(runAll) 'Don't need to pass the cancellation token here, as the background compactor handles it internally.
            End If
            If cToken <> Nothing AndAlso cToken.IsCancellationRequested Then
                _logger.LogInformation("Watcher run cancelled by user.")
                Return False
            End If
            Return True

        Catch ex As TaskCanceledException
            Return False
        Finally
            For Each watcher In WatchedFolders
                watcher.ResumeMonitoring()
            Next
            IsRunning = False
        End Try
        Return False
    End Function



    Private Sub OnSystemNotIdle(sender As Object, e As EventArgs)
        _isSystemIdle = False
        WatcherLog.SystemNotIdle(_logger)

        'Skip idle analysis if the background mode is not set to IdleOnly
        Dim bgMode = _settingsService.AppSettings.BackgroundModeSelection
        If bgMode <> BackgroundMode.IdleOnly Then Return

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

        For Each folder In initialWatchedFolders
            If IO.Directory.Exists(folder.Folder) Then
                folder.IsDriveUnavailable = False
                folder.InitializeMonitoring()
            ElseIf IsRootUnavailable(folder.Folder) Then
                folder.IsDriveUnavailable = True
            Else
                Continue For
            End If

            folder.LastChangedDate = folder.LastSystemModifiedDate
            WatchedFolders.Add(folder)
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
            .SkipList = newItem.SkipList
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
            Dim watchedFolder = WatchedFolders(i)

            If IO.Directory.Exists(watchedFolder.Folder) Then
                If watchedFolder.IsDriveUnavailable Then watchedFolder.InitializeMonitoring()
                watchedFolder.IsDriveUnavailable = False
                Continue For
            End If

            watchedFolder.IsDriveUnavailable = IsRootUnavailable(watchedFolder.Folder)
            If watchedFolder.IsDriveUnavailable Then Continue For

            WatcherLog.RemovingNonexistentFolders(_logger, 1)
            Await RemoveWatched(watchedFolder, False)
        Next

        Await WriteToFileAsync()

    End Function

    Private Shared Function IsRootUnavailable(folderPath As String) As Boolean
        Try
            Dim rootPath = IO.Path.GetPathRoot(folderPath)
            Return Not String.IsNullOrWhiteSpace(rootPath) AndAlso Not IO.Directory.Exists(rootPath)
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return False
        End Try
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




    Public Async Function ParseWatchers(Optional ParseAll As Boolean = False, Optional cToken As CancellationToken = Nothing) As Task
        Dim acquired = Await _parseWatchersSemaphore.WaitAsync(0)
        If Not acquired Then Return

        Try
            WatcherLog.ParsingWatchers(_logger, ParseAll)
            Await DeleteWatchersWithNonExistentFolders()

            Dim WatchersQuery = WatchedFolders.Where(Function(w) Not w.IsDriveUnavailable AndAlso (ParseAll OrElse w.HasTargetChanged)).OrderBy(Function(f) f.DisplayName)

            If Not WatchersQuery.Any() Then Return

            For Each fsWatcher In WatchersQuery
                WatcherLog.FolderChanged(_logger, fsWatcher.DisplayName)
                If cToken <> Nothing AndAlso cToken.IsCancellationRequested Then Return
                Await Analyse(fsWatcher.Folder, ParseAll, cToken)
            Next

            If cToken <> Nothing AndAlso cToken.IsCancellationRequested Then Return
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
                watchedFolder.IsDriveUnavailable = IsRootUnavailable(watchedFolder.Folder)
                If watchedFolder.IsDriveUnavailable Then Return
                Await RemoveWatched(watchedFolder)
                Return
            End If

            If watchedFolder.IsDriveUnavailable Then watchedFolder.InitializeMonitoring()
            watchedFolder.IsDriveUnavailable = False
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
                          Dim eligible = Not folder.IsDriveUnavailable AndAlso folder.DecayPercentage <> 0 AndAlso folder.CompressionLevel <> WOFCompressionAlgorithm.NO_COMPRESSION
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


    Public Async Function Analyse(folder As String, checkDiskModified As Boolean, Optional cToken As CancellationToken = Nothing) As Task(Of Boolean)

        Using analyser As New Analyser(folder, NullLogger(Of Analyser).Instance)
            Dim watched = WatchedFolders.First(Function(f) f.Folder = folder)
            watched.IsWorking = True
            Try
                Dim analysedFiles = Await analyser.GetAnalysedFilesAsync(cToken)
                If cToken <> Nothing AndAlso cToken.IsCancellationRequested Then Return False

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
            Catch ex As OperationCanceledException
                Return False
            Finally

                watched.IsWorking = False
            End Try

            Return True

        End Using

    End Function

    Public Sub Receive(message As PropertyChangedMessage(Of Boolean)) Implements IRecipient(Of PropertyChangedMessage(Of Boolean)).Receive
        If (message.Sender.GetType() IsNot GetType(Settings)) Then Return

        If message.PropertyName = NameOf(Settings.EnableBackgroundWatcher) Then IsWatchingEnabled = message.NewValue
    End Sub

    Public Sub Receive(message As PropertyChangedMessage(Of BackgroundMode)) Implements IRecipient(Of PropertyChangedMessage(Of BackgroundMode)).Receive
        If message.Sender.GetType() IsNot GetType(Settings) Then Return

        If message.PropertyName = NameOf(Settings.BackgroundModeSelection) Then IsBackgroundCompactingEnabled = message.NewValue = BackgroundMode.IdleOnly


    End Sub





End Class


