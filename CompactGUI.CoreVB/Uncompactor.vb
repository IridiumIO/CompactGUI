Imports System.IO
Imports System.Threading

Imports Microsoft.Win32.SafeHandles

Public Class Uncompactor : Implements ICompressor, IDisposable


    Private _pauseSemaphore As New SemaphoreSlim(1, 2)
    Private _processedFileCount As New Concurrent.ConcurrentDictionary(Of String, Integer)
    Private _cancellationTokenSource As New CancellationTokenSource


    Public Async Function RunAsync(filesList As List(Of String), Optional progressMonitor As IProgress(Of CompressionProgress) = Nothing, Optional MaxParallelism As Integer = 1) As Task(Of Boolean) Implements ICompressor.RunAsync

        Dim totalFiles As Integer = filesList.Count
        If MaxParallelism <= 0 Then MaxParallelism = Environment.ProcessorCount
        Dim paraOptions As New ParallelOptions With {.MaxDegreeOfParallelism = MaxParallelism}

        _processedFileCount.Clear()
        Try
            Await Parallel.ForEachAsync(filesList, paraOptions,
                                  Function(file, _ctx) As ValueTask
                                      _ctx.ThrowIfCancellationRequested()
                                      Return New ValueTask(PauseAndProcessFile(file, totalFiles, _cancellationTokenSource.Token, progressMonitor))
                                  End Function).ConfigureAwait(False)
        Catch ex As OperationCanceledException
            Return False
        End Try

        Return True

    End Function

    Private Async Function PauseAndProcessFile(file As String, totalFiles As Integer, _ctx As CancellationToken, progressMonitor As IProgress(Of CompressionProgress)) As Task

        Try
            Await _pauseSemaphore.WaitAsync(_ctx).ConfigureAwait(False)
            _pauseSemaphore.Release()
        Catch ex As OperationCanceledException
            Throw
            Return
        End Try

        _ctx.ThrowIfCancellationRequested()

        Dim res = WOFDecompressFile(file)
        _processedFileCount.TryAdd(file, 1)

        progressMonitor?.Report(New CompressionProgress((CInt(((_processedFileCount.Count / totalFiles) * 100))), file))

    End Function

    Private Function WOFDecompressFile(path As String)

        Try
            Using fs As SafeFileHandle = File.OpenHandle(path)
                Dim res = WOFHelper.DeviceIoControl(fs, FSCTL_DELETE_EXTERNAL_BACKING, IntPtr.Zero, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero)
                Return res
            End Using
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
            Return Nothing
        End Try

    End Function


    Public Sub Pause() Implements ICompressor.Pause
        _pauseSemaphore.Wait()
    End Sub

    Public Sub [Resume]() Implements ICompressor.Resume
        If _pauseSemaphore.CurrentCount = 0 Then _pauseSemaphore.Release()
    End Sub
    Public Sub Cancel() Implements ICompressor.Cancel
        [Resume]()
        _cancellationTokenSource.Cancel()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        _pauseSemaphore?.Dispose()
        _cancellationTokenSource?.Dispose()
    End Sub
End Class
