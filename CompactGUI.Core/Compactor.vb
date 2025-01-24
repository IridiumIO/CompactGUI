Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading


Public Class Compactor : Implements IDisposable

    Public Sub New(folder As String, cLevel As CompressionAlgorithm, excludedFilesTypes As String())

        If Not verifyFolder(folder).isValid Then Return

        _workingDir = folder
        _excludedFileTypes = excludedFilesTypes
        _WOFCompressionLevel = cLevel

        _EFInfo = New WOF_FILE_COMPRESSION_INFO_V1 With {.Algorithm = _WOFCompressionLevel, .Flags = 0}
        _EFInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_EFInfo))
        Marshal.StructureToPtr(_EFInfo, _EFInfoPtr, True)

    End Sub

    Private _workingDir As String
    Private _excludedFileTypes() As String
    Private _WOFCompressionLevel As CompressionAlgorithm

    Private _EFInfo As WOF_FILE_COMPRESSION_INFO_V1
    Private _EFInfoPtr As IntPtr

    Private _pauseSemaphore As New SemaphoreSlim(1, 2)

    Private _processedFilesBytes As Long = 0

    Private _cancellationTokenSource As New CancellationTokenSource



    Public Async Function RunCompactAsync(Optional progressMonitor As IProgress(Of (percentageProgress As Integer, currentFile As String)) = Nothing, Optional MaxParallelism As Integer = 1) As Task(Of Boolean)
        If _cancellationTokenSource.IsCancellationRequested Then Return False

        Dim FilesList = Await BuildWorkingFilesList()
        Dim totalFilesSize As Long = FilesList.Sum(Function(f) f.Item2)
        _processedFilesBytes = 0

        If MaxParallelism <= 0 Then MaxParallelism = Environment.ProcessorCount


        Dim sw As New Stopwatch
        sw.Start()

        Dim paraOptions As New ParallelOptions With {.MaxDegreeOfParallelism = MaxParallelism}

        Await Parallel.ForEachAsync(FilesList, paraOptions,
                                Function(file, _ctx) As ValueTask
                                    If _ctx.IsCancellationRequested Then Return ValueTask.FromCanceled(_ctx)
                                    Return New ValueTask(PauseAndProcessFile(file.Item1, _cancellationTokenSource.Token, file.Item2, totalFilesSize, progressMonitor))
                                End Function).ConfigureAwait(False)


        sw.Stop()

        Debug.WriteLine($"Completed in {sw.Elapsed.TotalSeconds} s")


        If _cancellationTokenSource.IsCancellationRequested Then Return False

        Return True
    End Function

    Private Async Function PauseAndProcessFile(file As String, _ctx As CancellationToken, fileSize As Long, totalFilesSize As Long, Optional progressMonitor As IProgress(Of (percentageProgress As Integer, currentFile As String)) = Nothing) As Task

        If _ctx.IsCancellationRequested Then Return
        Try
            Await _pauseSemaphore.WaitAsync(_ctx).ConfigureAwait(False)
            _pauseSemaphore.Release()

        Catch ex As OperationCanceledException
            Return
        End Try

        If _ctx.IsCancellationRequested Then Return

        Dim res = WOFCompressFile(file)

        Interlocked.Add(_processedFilesBytes, fileSize)
        Dim incremented = _processedFilesBytes

        progressMonitor?.Report((CInt(((incremented / totalFilesSize) * 100)), file))

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

    Private Function WOFCompressFile(path As String)

        Dim length As ULong = Marshal.SizeOf(_EFInfoPtr)
        Try
            Using fs As FileStream = New FileStream(path, FileMode.Open)
                Dim hFile = fs.SafeFileHandle.DangerousGetHandle()
                Dim res = WofSetFileDataLocation(hFile, WOF_PROVIDER_FILE, _EFInfoPtr, length)
                Return res
            End Using
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
            Return Nothing
        End Try

    End Function

    Private Async Function BuildWorkingFilesList() As Task(Of IEnumerable(Of (String, Long)))

        Dim clusterSize As Integer = GetClusterSize(_workingDir)

        Dim _filesList As New Concurrent.ConcurrentBag(Of (String, Long))
        'TODO: if the user has already analysed within the last minute, then skip creating a new one and use the old one
        Dim ax As New Analyser(_workingDir)
        Dim ret = Await ax.AnalyseFolder(Nothing)

        Parallel.ForEach(ax.FileCompressionDetailsList, Sub(fl)
                                                            Dim ft = fl.FileInfo
                                                            If Not _excludedFileTypes.Contains(ft.Extension) AndAlso ft.Length > clusterSize AndAlso fl.CompressionMode <> _WOFCompressionLevel Then _filesList.Add((fl.FileName, fl.UncompressedSize))
                                                        End Sub)


        Return _filesList.ToList
    End Function


    Public Sub Dispose() Implements IDisposable.Dispose
        _cancellationTokenSource.Dispose()
        _pauseSemaphore.Dispose()
        If _EFInfoPtr <> IntPtr.Zero Then
            Marshal.FreeHGlobal(_EFInfoPtr)
            _EFInfoPtr = IntPtr.Zero
        End If
    End Sub
End Class
