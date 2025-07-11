Imports System.Collections.ObjectModel
Imports System.Threading

Imports CompactGUI.Logging.Watcher

Imports Microsoft.Extensions.Logging

Imports Microsoft.Extensions.Logging.Abstractions

Public Class BackgroundCompactor

    Public Property IsCompactorActive As Boolean = False

    Private cancellationTokenSource As New CancellationTokenSource()
    Private isCompacting As Boolean = False
    Private isCompactingPaused As Boolean = False ' Track if compacting is paused

    Private _compactor As Core.Compactor

    Private _excludedFileTypes As String()


    Private ReadOnly _logger As ILogger(Of Watcher)

    Private ReadOnly _idleSettings As IdleSettings

    Public Sub New(excludedFileTypes As String(), logger As ILogger(Of Watcher), settings As IdleSettings)

        _excludedFileTypes = excludedFileTypes
        _logger = logger
        _idleSettings = settings

    End Sub


    Public Function BeginCompacting(folder As String, compressionLevel As Core.WOFCompressionAlgorithm) As Task(Of Boolean)

        If compressionLevel = Core.WOFCompressionAlgorithm.NO_COMPRESSION Then Return Task.FromResult(False)

        _compactor = New Core.Compactor(folder, compressionLevel, _excludedFileTypes, New Core.Analyser(folder, NullLogger(Of Core.Analyser).Instance))

        Return _compactor.RunAsync(Nothing)

    End Function

    Public Async Function StartCompactingAsync(folders As ObservableCollection(Of WatchedFolder)) As Task(Of Boolean)
        WatcherLog.BackgroundCompactingStarted(_logger)
        Dim cancellationToken As CancellationToken = cancellationTokenSource.Token

        IsCompactorActive = True

        Dim currentProcess As Process = Process.GetCurrentProcess()
        currentProcess.PriorityClass = ProcessPriorityClass.Idle

        Dim foldersCopy As List(Of WatchedFolder) = folders.Where(Function(f) f.DecayPercentage <> 0 AndAlso f.CompressionLevel <> Core.WOFCompressionAlgorithm.NO_COMPRESSION).ToList()


        For Each folder In foldersCopy
            folder.IsWorking = True
            Dim recentThresholdDate As DateTime = DateTime.Now.AddSeconds(-_idleSettings.LastSystemModifiedTimeThresholdSeconds)
            If folder.LastSystemModifiedDate > recentThresholdDate Then
                WatcherLog.SkippingRecentlyModifiedFolder(_logger, folder.DisplayName)
                Continue For
            End If

            WatcherLog.CompactingFolder(_logger, folder.DisplayName)
            Dim compactingTask = BeginCompacting(folder.Folder, folder.CompressionLevel)
            isCompacting = True

            While Not cancellationToken.IsCancellationRequested AndAlso Not compactingTask.IsCompleted
                Await Task.WhenAny(compactingTask, Task.Delay(1000, cancellationToken))

                '' Check the idle state and adjust compacting status accordingly
                'If Not isSystemIdle AndAlso Not isCompactingPaused Then
                '    PauseCompacting()
                'ElseIf isSystemIdle AndAlso isCompactingPaused Then
                '    ResumeCompacting()
                'End If
            End While

            Dim result = Await compactingTask
            If result AndAlso folders.Contains(folder) Then
                ' Ensure the folder is still in the original collection before updating

                Dim analyser As New Core.Analyser(folder.Folder, NullLogger(Of Core.Analyser).Instance)

                Dim analysed = Await analyser.GetAnalysedFilesAsync(Nothing)

                folder.LastCheckedDate = DateTime.Now
                folder.LastCheckedSize = analyser.CompressedBytes
                folder.LastCompressedSize = analyser.CompressedBytes
                folder.LastSystemModifiedDate = DateTime.Now
                Dim mainCompressionLVL = analysed.Select(Function(f) f.CompressionMode).Max
                folder.CompressionLevel = mainCompressionLVL

                folder.LastCompressedDate = DateTime.Now

                folder.HasTargetChanged = False

            End If
            folder.IsWorking = False
            folder.RefreshProperties()
            _compactor.Dispose()
            WatcherLog.FinishedCompactingFolder(_logger, folder.DisplayName)
        Next

        IsCompactorActive = False
        isCompacting = False ' Ensure compacting status is reset after operation
        WatcherLog.BackgroundCompactingFinished(_logger)
        currentProcess.PriorityClass = ProcessPriorityClass.Normal
        Return True
    End Function

    Public Sub PauseCompacting()
        If Not isCompacting OrElse isCompactingPaused Then
            Return
        End If

        WatcherLog.PausingBackgroundCompactor(_logger)
        isCompactingPaused = True ' Indicate compacting is paused
        _compactor?.Pause()
    End Sub

    Public Sub ResumeCompacting()
        If Not isCompactingPaused OrElse Not isCompacting Then
            Return
        End If

        WatcherLog.ResumingBackgroundCompactor(_logger)
        isCompactingPaused = False ' Indicate compacting is no longer paused
        _compactor?.Resume()
    End Sub

End Class
