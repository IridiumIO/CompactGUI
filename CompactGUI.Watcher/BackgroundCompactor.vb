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
    Private _compactorAnalyser As Core.Analyser

    Private _excludedFileTypes As String()


    Private ReadOnly _logger As ILogger(Of Watcher)


    Public Event IsCompactingEvent As EventHandler(Of Boolean)

    Public Sub New(excludedFileTypes As String(), logger As ILogger(Of Watcher))

        _excludedFileTypes = excludedFileTypes
        _logger = logger
    End Sub


    Public Function BeginCompacting(folder As String, compressionLevel As Core.WOFCompressionAlgorithm, Optional excludedFileTypes As String() = Nothing) As Task(Of Boolean)

        If compressionLevel = Core.WOFCompressionAlgorithm.NO_COMPRESSION Then Return Task.FromResult(False)

        Dim effectiveExclusions = If(excludedFileTypes Is Nothing, _excludedFileTypes, excludedFileTypes)

        _compactorAnalyser = New Core.Analyser(folder, NullLogger(Of Core.Analyser).Instance)

        _compactor = New Core.Compactor(folder, compressionLevel, effectiveExclusions, _compactorAnalyser)

        Return _compactor.RunAsync(Nothing)

    End Function

    Public Async Function StartCompactingAsync(folders As IEnumerable(Of WatchedFolder)) As Task(Of Boolean)
        WatcherLog.BackgroundCompactingStarted(_logger)

        cancellationTokenSource = New CancellationTokenSource()

        Dim currentProcess = Process.GetCurrentProcess()
        Dim originalPriority = currentProcess.PriorityClass

        Try
            IsCompactorActive = True
            isCompacting = True
            currentProcess.PriorityClass = ProcessPriorityClass.Idle

            For Each folder In folders.ToList()
                If cancellationTokenSource.IsCancellationRequested Then Return False
                If Not Await CompactFolderAsync(folder, folders) Then Return False

                WatcherLog.FinishedCompactingFolder(_logger, folder.DisplayName)
            Next

            WatcherLog.BackgroundCompactingFinished(_logger)
            Return True
        Finally
            IsCompactorActive = False
            isCompacting = False
            isCompactingPaused = False

            cancellationTokenSource.Dispose()
            cancellationTokenSource = Nothing

            currentProcess.PriorityClass = originalPriority
            currentProcess.Dispose()
        End Try
    End Function

    Private Async Function CompactFolderAsync(folder As WatchedFolder, folders As IEnumerable(Of WatchedFolder)) As Task(Of Boolean)
        folder.IsWorking = True

        Try
            WatcherLog.CompactingFolder(_logger, folder.DisplayName)

            Dim folderSkipList = If(folder.SkipList Is Nothing, Array.Empty(Of String)(), folder.SkipList.ToArray())
            Dim compactingTask = BeginCompacting(folder.Folder, folder.CompressionLevel, folderSkipList)

            If cancellationTokenSource.IsCancellationRequested Then _compactor?.Cancel()

            Dim result = Await compactingTask

            If cancellationTokenSource.IsCancellationRequested Then Return False

            If result AndAlso folders.Contains(folder) Then
                Await UpdateFolderStatisticsAsync(folder)
            End If

            Return True
        Finally
            DisposeCurrentCompactor()
            folder.IsWorking = False
            folder.RefreshProperties()
        End Try
    End Function

    Private Async Function UpdateFolderStatisticsAsync(folder As WatchedFolder) As Task
        Using analyser As New Core.Analyser(folder.Folder, NullLogger(Of Core.Analyser).Instance)
            Dim analysed = Await analyser.GetAnalysedFilesAsync(CancellationToken.None)

            folder.LastCheckedDate = DateTime.Now
            folder.LastCheckedSize = analyser.CompressedBytes
            folder.LastCompressedSize = analyser.CompressedBytes
            folder.LastSystemModifiedDate = DateTime.Now

            If analysed IsNot Nothing AndAlso analysed.Count > 0 Then
                folder.CompressionLevel = analysed.Max(Function(file) file.CompressionMode)
            End If

            folder.LastCompressedDate = DateTime.Now
            folder.HasTargetChanged = False
        End Using
    End Function

    Private Sub FinishRun(runCancellation As CancellationTokenSource, currentProcess As Process, originalPriority As ProcessPriorityClass)
        If ReferenceEquals(cancellationTokenSource, runCancellation) Then cancellationTokenSource = Nothing

        runCancellation.Dispose()

        IsCompactorActive = False
        isCompacting = False
        isCompactingPaused = False

        Try
            currentProcess.PriorityClass = originalPriority
        Finally
            currentProcess.Dispose()
        End Try
    End Sub

    Public Sub PauseCompacting()
        If Not isCompacting OrElse isCompactingPaused Then Return


        WatcherLog.PausingBackgroundCompactor(_logger)
        isCompactingPaused = True ' Indicate compacting is paused
        _compactor?.Pause()
    End Sub

    Public Sub ResumeCompacting()
        If Not isCompactingPaused OrElse Not isCompacting Then Return

        WatcherLog.ResumingBackgroundCompactor(_logger)
        isCompactingPaused = False ' Indicate compacting is no longer paused
        _compactor?.Resume()
    End Sub

    Public Sub CancelCompacting()
        If Not isCompacting Then Return

        Debug.WriteLine("Cancelling background compactor...")

        cancellationTokenSource?.Cancel()
        _compactor?.Cancel()
    End Sub

    Private Sub DisposeCurrentCompactor()
        Dim compactor = _compactor
        _compactor = Nothing
        compactor?.Dispose()

        Dim analyser = _compactorAnalyser
        _compactorAnalyser = Nothing
        analyser?.Dispose()
    End Sub

End Class
