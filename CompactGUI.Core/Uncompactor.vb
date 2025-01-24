Imports System.IO
Imports System.Threading

Public Class Uncompactor


    Private _pauseSemaphore As New SemaphoreSlim(1, 2)
    Private _processedFileCount As New Concurrent.ConcurrentDictionary(Of String, Integer)
    Private _cancellationTokenSource As New CancellationTokenSource


    Public Async Function UncompactFiles(filesList As List(Of String), Optional progressMonitor As IProgress(Of (percentageProgress As Integer, currentFile As String)) = Nothing, Optional MaxParallelism As Integer = 1) As Task(Of Boolean)

        Dim totalFiles As Integer = filesList.Count

        If MaxParallelism <= 0 Then MaxParallelism = Environment.ProcessorCount

        Dim paraOptions As New ParallelOptions With {.MaxDegreeOfParallelism = MaxParallelism}


        Dim sw As New Stopwatch
        sw.Start()


        _processedFileCount.Clear()
        Await Parallel.ForEachAsync(filesList, paraOptions,
                                   Function(file, _ctx) As ValueTask
                                       If _ctx.IsCancellationRequested Then Return ValueTask.FromCanceled(_ctx)
                                       Return New ValueTask(PauseAndProcessFile(file, _cancellationTokenSource.Token, totalFiles, progressMonitor))
                                   End Function).ConfigureAwait(False)


        sw.Stop()
        Debug.WriteLine($"Completed in {sw.Elapsed.TotalSeconds} s")

        Return True
    End Function

    Private Async Function PauseAndProcessFile(file As String, _ctx As CancellationToken, totalFiles As Integer, progressMonitor As IProgress(Of (percentageProgress As Integer, currentFile As String))) As Task
        If _ctx.IsCancellationRequested Then Return

        Try
            Await _pauseSemaphore.WaitAsync(_ctx).ConfigureAwait(False)
            _pauseSemaphore.Release()

        Catch ex As OperationCanceledException
            Return
        End Try

        If _ctx.IsCancellationRequested Then Return
        Dim res = WOFDecompressFile(file)
        _processedFileCount.TryAdd(file, 1)
        Dim incremented = _processedFileCount.Count
        progressMonitor?.Report((CInt(((incremented / totalFiles) * 100)), file))
    End Function

    Private Function WOFDecompressFile(path As String)

        Try
            Using fs As FileStream = New FileStream(path, FileMode.Open)
                Dim hDevice = fs.SafeFileHandle.DangerousGetHandle
                Dim res = DeviceIoControl(hDevice, FSCTL_DELETE_EXTERNAL_BACKING, IntPtr.Zero, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero)
                Return res
            End Using
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
            Return Nothing
        End Try

    End Function


    Public Sub PauseCompression()
        _pauseSemaphore.Wait()
    End Sub

    Public Sub ResumeCompression()
        If _pauseSemaphore.CurrentCount = 0 Then _pauseSemaphore.Release()
    End Sub
    Public Sub Cancel()
        ResumeCompression()
        _cancellationTokenSource.Cancel()
    End Sub

End Class
