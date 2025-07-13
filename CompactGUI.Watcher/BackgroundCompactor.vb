Imports System.Collections.ObjectModel
Imports System.Threading

Imports CompactGUI.Logging.Watcher

Imports Microsoft.Extensions.Logging

Imports Microsoft.Extensions.Logging.Abstractions

Public Class BackgroundCompactor

    Private _IsCompactorActive As Boolean = False
    Public Property IsCompactorActive As Boolean
        Get
            Return _IsCompactorActive
        End Get
        Set(value As Boolean)
            If _IsCompactorActive = value Then Return
            _IsCompactorActive = value
            RaiseEvent IsCompactingEvent(Me, value)
        End Set
    End Property

    Private cancellationTokenSource As New CancellationTokenSource()
    Private isCompacting As Boolean = False
    Private isCompactingPaused As Boolean = False ' Track if compacting is paused

    Private _compactor As Core.Compactor

    Private _excludedFileTypes As String()


    Private ReadOnly _logger As ILogger(Of Watcher)


    Public Event IsCompactingEvent As EventHandler(Of Boolean)

    Public Sub New(excludedFileTypes As String(), logger As ILogger(Of Watcher))

        _excludedFileTypes = excludedFileTypes
        _logger = logger
    End Sub


    Public Function BeginCompacting(folder As String, compressionLevel As Core.WOFCompressionAlgorithm) As Task(Of Boolean)

        If compressionLevel = Core.WOFCompressionAlgorithm.NO_COMPRESSION Then Return Task.FromResult(False)

        _compactor = New Core.Compactor(folder, compressionLevel, _excludedFileTypes, New Core.Analyser(folder, NullLogger(Of Core.Analyser).Instance))

        Return _compactor.RunAsync(Nothing)

    End Function

    Public Async Function StartCompactingAsync(folders As IEnumerable(Of WatchedFolder)) As Task(Of Boolean)
        WatcherLog.BackgroundCompactingStarted(_logger)
        cancellationTokenSource = New CancellationTokenSource()

        IsCompactorActive = True

        Dim currentProcess As Process = Process.GetCurrentProcess()
        currentProcess.PriorityClass = ProcessPriorityClass.Idle

        isCompacting = True

        For Each folder In folders.ToList
            folder.IsWorking = True

            WatcherLog.CompactingFolder(_logger, folder.DisplayName)
            Dim compactingTask = BeginCompacting(folder.Folder, folder.CompressionLevel)


            If cancellationTokenSource.IsCancellationRequested Then
                Trace.WriteLine("Compacting cancelled by user.")
                folder.IsWorking = False
                IsCompactorActive = False
                isCompacting = False ' Ensure compacting status is reset after operation
                _compactor.Dispose()
                Return False
            End If

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

    Public Sub CancelCompacting()
        If Not isCompacting Then
            Return
        End If
        Debug.WriteLine("Cancelling background compactor...")
        cancellationTokenSource.Cancel()
        cancellationTokenSource.Dispose()
        _compactor?.Cancel()
        _compactor?.Dispose()
        isCompacting = False
        isCompactingPaused = False ' Reset pause state on cancellation
    End Sub

End Class
