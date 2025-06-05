Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading

Imports Microsoft.Win32.SafeHandles


Public Class Compactor : Implements IDisposable, ICompressor

    Private ReadOnly workingDirectory As String
    Private ReadOnly excludedFileExtensions() As String
    Private ReadOnly wofCompressionAlgorithm As WOFCompressionAlgorithm

    Private compressionInfoPtr As IntPtr
    Private compressionInfoSize As UInteger

    Private totalProcessedBytes As Long = 0
    Private ReadOnly pauseSemaphore As New SemaphoreSlim(1, 2)
    Private ReadOnly cancellationTokenSource As New CancellationTokenSource


    Public Sub New(folder As String, compressionLevel As WOFCompressionAlgorithm, excludedFilesTypes As String())

        workingDirectory = folder
        wofCompressionAlgorithm = compressionLevel
        excludedFileExtensions = excludedFilesTypes

        InitializeCompressionInfoPointer()

    End Sub


    Private Sub InitializeCompressionInfoPointer()
        Dim _EFInfo As New WOF_FILE_COMPRESSION_INFO_V1 With {.Algorithm = wofCompressionAlgorithm, .Flags = 0}
        compressionInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_EFInfo))
        compressionInfoSize = CUInt(Marshal.SizeOf(_EFInfo))
        Marshal.StructureToPtr(_EFInfo, compressionInfoPtr, True)
    End Sub

    <MeasurePerformance.IL.Weaver.MeasurePerformance>
    Public Async Function RunAsync(filesList As List(Of String), Optional progressMonitor As IProgress(Of CompressionProgress) = Nothing, Optional MaxParallelism As Integer = 1) As Task(Of Boolean) Implements ICompressor.RunAsync

        If cancellationTokenSource.IsCancellationRequested Then Return False

        Dim workingFiles = Await BuildWorkingFilesList().ConfigureAwait(False)
        Dim totalFilesSize As Long = workingFiles.Sum(Function(f) f.UncompressedSize)
        totalProcessedBytes = 0

        If MaxParallelism <= 0 Then MaxParallelism = Environment.ProcessorCount

        Dim paraOptions As New ParallelOptions With {.MaxDegreeOfParallelism = MaxParallelism}

        Try
            Await Parallel.ForEachAsync(workingFiles, paraOptions,
            Function(file, _ctx) As ValueTask
                _ctx.ThrowIfCancellationRequested()
                Return New ValueTask(PauseAndProcessFile(file, totalFilesSize, cancellationTokenSource.Token, progressMonitor))
            End Function).ConfigureAwait(False)
        Catch ex As OperationCanceledException
            ' Swallow cancellation, return false
            Return False
        End Try

        Return Not cancellationTokenSource.IsCancellationRequested

    End Function

    Private Async Function PauseAndProcessFile(details As FileDetails, totalFilesSize As Long, _ctx As CancellationToken, Optional progressMonitor As IProgress(Of CompressionProgress) = Nothing) As Task

        Try
            Await pauseSemaphore.WaitAsync(_ctx).ConfigureAwait(False)
            pauseSemaphore.Release()
        Catch ex As OperationCanceledException
            Throw
            Return
        End Try

        _ctx.ThrowIfCancellationRequested()

        Dim res = WOFCompressFile(details.FileName)
        Interlocked.Add(totalProcessedBytes, details.UncompressedSize)
        progressMonitor?.Report(New CompressionProgress(totalProcessedBytes / totalFilesSize * 100, details.FileName))


    End Function



    Public Sub Pause() Implements ICompressor.Pause
        pauseSemaphore.Wait()
    End Sub


    Public Sub [Resume]() Implements ICompressor.Resume
        If pauseSemaphore.CurrentCount = 0 Then pauseSemaphore.Release()
    End Sub

    Public Sub Cancel() Implements ICompressor.Cancel
        [Resume]()
        cancellationTokenSource.Cancel()
    End Sub



    Private Function WOFCompressFile(path As String) As Integer

        Try
            Using fs As SafeFileHandle = File.OpenHandle(path)
                Return WofSetFileDataLocation(fs, WOF_PROVIDER_FILE, compressionInfoPtr, compressionInfoSize)
            End Using
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
            Return Nothing
        End Try

    End Function

    Private Async Function BuildWorkingFilesList() As Task(Of IEnumerable(Of FileDetails))

        Dim clusterSize As Integer = GetClusterSize(workingDirectory)

        Dim _filesList As New Concurrent.ConcurrentBag(Of FileDetails)
        'TODO: if the user has already analysed within the last minute, then skip creating a new one and use the old one
        Dim ax As New Analyser(workingDirectory)
        Dim ret = Await ax.AnalyseFolder(Nothing)

        Parallel.ForEach(ax.FileCompressionDetailsList, Sub(fl)
                                                            Dim ft = fl.FileInfo
                                                            If Not (excludedFileExtensions.Contains(ft.Extension) OrElse excludedFileExtensions.Contains(fl.FileName)) AndAlso
                                                                    ft.Length > clusterSize AndAlso
                                                                    fl.CompressionMode <> wofCompressionAlgorithm Then
                                                                _filesList.Add((New FileDetails With {.FileName = fl.FileName, .UncompressedSize = fl.UncompressedSize}))
                                                            End If
                                                        End Sub)


        Return _filesList.ToList
    End Function


    Public Sub Dispose() Implements IDisposable.Dispose
        cancellationTokenSource?.Dispose()
        pauseSemaphore?.Dispose()
        If Not compressionInfoPtr.Equals(IntPtr.Zero) Then
            Marshal.FreeHGlobal(compressionInfoPtr)
            compressionInfoPtr = IntPtr.Zero
        End If
    End Sub



    Private Structure FileDetails
        Public Property FileName As String
        Public Property UncompressedSize As Long

    End Structure


End Class
