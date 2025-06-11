Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.InteropServices

Imports K4os.Compression.LZ4

Imports K4os.Compression.LZ4.Streams

Imports Microsoft.Win32.SafeHandles


'https://unix.stackexchange.com/questions/155901/estimate-compressibility-of-file

Public Class Estimator
    Private Const BlockSize As Integer = 8192 '16K? Should I get it from the ClusterSize?
    Private Const ZScore As Single = 1.96
    Private Const ErrorMargin As Single = 0.1
    Private Const SampleSize As Single = 100 '0.25 * (ZScore / ErrorMargin) ^ 2

    Private DiskClusterSize As Integer = 4096
    Private IsHDD As Boolean = False

    Public Sub New()
    End Sub


    Public Delegate Function CompressionStreamFactory(output As Stream) As Stream


    Public Class FileDetails
        Public AnalysedFile As AnalysedFileDetails
        Public CompressionRatio As Single
    End Class

    Public Shared IsAlternate As Boolean = False

    Public Function EstimateCompressability(analysisResult As List(Of AnalysedFileDetails), ishdd As Boolean, Optional MaxParallelism As Integer = 1, Optional clusterSize As Integer = 4096, Optional cancellationToken As Threading.CancellationToken = Nothing) As List(Of (AnalysedFile As AnalysedFileDetails, CompressionRatio As Single))

        DiskClusterSize = clusterSize
        Dim _filesList As New Concurrent.ConcurrentBag(Of FileDetails)
        If MaxParallelism <= 0 Then MaxParallelism = Environment.ProcessorCount

        Me.IsHDD = ishdd


        'Filter the files based on the cluster size and sort them by cluster location if it's an HDD to minimize seek time
        Dim filteredList = analysisResult.Where(Function(f) f.UncompressedSize > clusterSize)

        Dim sw = Stopwatch.StartNew()

        If ishdd Then filteredList = filteredList.OrderBy(Function(f) GetFirstLcn(f.FileName))
        Dim finalList = filteredList.ToList
        sw.Stop()
        Debug.WriteLine($"Filtered and sorted {finalList.Count} files in {sw.ElapsedMilliseconds} ms")
        If IsAlternate Then Debug.WriteLine("Alternate mode enabled, using different handle method.")


        Dim paraOptions As New ParallelOptions With {.MaxDegreeOfParallelism = MaxParallelism}

        Parallel.ForEach(finalList, parallelOptions:=paraOptions, Sub(fl)
                                                                      cancellationToken.ThrowIfCancellationRequested()

                                                                      Dim estimatedRatio = EstimateCompressabilityLZ4(fl.FileName, fl.UncompressedSize, cancellationToken)

                                                                      _filesList.Add((New FileDetails With {.AnalysedFile = fl, .CompressionRatio = estimatedRatio}))
                                                                  End Sub)

        Dim retList As New List(Of (AnalysedFile As AnalysedFileDetails, CompressionRatio As Single))

        For Each item In _filesList
            retList.Add((item.AnalysedFile, item.CompressionRatio))
        Next

        IsAlternate = Not IsAlternate ' Toggle the alternate mode for next run

        Return retList

    End Function


    Public Function EstimateCompressabilityGZip(path As String, filesize As Long) As Double

        Using fs As FileStream = File.OpenRead(path)
            Return EstimateCompressability(fs, filesize, Function(output) New GZipStream(output, CompressionLevel.Fastest, True))
        End Using

    End Function


    Public Function EstimateCompressabilitySnappy(path As String, filesize As Long) As Double

        Using fs As FileStream = File.OpenRead(path)
            Return EstimateCompressability(fs, filesize, Function(output) New Snappier.SnappyStream(output, Compression.CompressionMode.Compress, True))
        End Using

    End Function

    Public Function EstimateCompressabilityLZ4(path As String, filesize As Long, Optional cancellationToken As Threading.CancellationToken = Nothing) As Double
        Try
            Using fs As FileStream = File.OpenRead(path)
                If IsHDD Then
                    Return EstimateCompressabilityHDD(fs, filesize, Function(output) LZ4Stream.Encode(output, LZ4Level.L00_FAST, 0, True), cancellationToken)
                End If
                Return EstimateCompressability(fs, filesize, Function(output) LZ4Stream.Encode(output, LZ4Level.L00_FAST, 0, True), cancellationToken)
            End Using
        Catch cancelledEx As OperationCanceledException
            Throw
        Catch ex As Exception
            Return 1.0 ' If there's an error, assume it's incompressible
        End Try

    End Function


    Private Function EstimateCompressability(input As FileStream, fileSize As Long, compressionFactory As CompressionStreamFactory, Optional cancellationToken As Threading.CancellationToken = Nothing) As Double

        Dim totalBlocks As Long = fileSize \ BlockSize
        Dim samplesNeeded As Integer = Math.Ceiling((totalBlocks * SampleSize) / (SampleSize + totalBlocks - 1))
        Dim totalWritten As Long = 0

        Dim compressed = New MemoryStream()
        Using compressionStream As Stream = compressionFactory(compressed)

            ' If file is small or sample count is zero, compress the whole file
            If samplesNeeded = 0 OrElse fileSize < samplesNeeded * BlockSize * 2 Then

                Dim buffer(BlockSize - 1) As Byte
                input.Position = 0
                Dim bytesRead As Integer = input.Read(buffer, 0, BlockSize)

                While bytesRead > 0
                    cancellationToken.ThrowIfCancellationRequested()
                    compressionStream.Write(buffer, 0, bytesRead)
                    totalWritten += bytesRead
                    bytesRead = input.Read(buffer, 0, BlockSize)
                End While

            Else
                ' Sample evenly spaced blocks
                Dim stepSize As Long = BlockSize * (totalBlocks \ samplesNeeded)
                Dim buffer(BlockSize - 1) As Byte
                For i As Integer = 0 To samplesNeeded - 1
                    cancellationToken.ThrowIfCancellationRequested()
                    input.Position = stepSize * i
                    Dim bytesRead As Integer = input.Read(buffer, 0, BlockSize)
                    compressionStream.Write(buffer, 0, bytesRead)
                    totalWritten += bytesRead
                Next
            End If
        End Using

        Return Math.Min(compressed.Length / Math.Max(totalWritten, 1), 1.0)
    End Function


    Private Function EstimateCompressabilityHDD(input As FileStream, fileSize As Long, compressionFactory As CompressionStreamFactory, Optional cancellationToken As Threading.CancellationToken = Nothing) As Double
        Dim NumClusters As Integer = SampleSize ' or any small number you want to sample
        Dim clusterSize As Integer = DiskClusterSize

        Dim middleCluster As Long = (fileSize \ 2) \ clusterSize
        Dim alignedStart As Long = middleCluster * clusterSize
        Dim chunkSize As Integer = CInt(Math.Min(clusterSize * NumClusters, fileSize - alignedStart))

        Dim totalWritten As Long = 0
        Dim compressed = New MemoryStream()

        Using compressionStream As Stream = compressionFactory(compressed)
            Dim buffer(chunkSize - 1) As Byte
            input.Position = alignedStart
            Dim bytesRead As Integer = input.Read(buffer, 0, chunkSize)

            If cancellationToken <> Nothing AndAlso cancellationToken.IsCancellationRequested Then
                Throw New OperationCanceledException(cancellationToken)
            End If

            If bytesRead > 0 Then
                compressionStream.Write(buffer, 0, bytesRead)
                totalWritten += bytesRead
            End If
        End Using

        Return Math.Min(compressed.Length / Math.Max(totalWritten, 1), 1.0)
    End Function

    Public Function GetFirstLcn(filePath As String) As Long

        Dim handle As SafeFileHandle = File.OpenHandle(filePath)

        If handle.IsInvalid Then Throw New IOException("Failed to open file handle.")


        Dim inBuffer As NtfsInterop.STARTING_VCN_INPUT_BUFFER
        inBuffer.StartingVcn = 0
        Dim inBufferSize = Marshal.SizeOf(inBuffer)
        Dim inBufferPtr = Marshal.AllocHGlobal(inBufferSize)
        Marshal.StructureToPtr(inBuffer, inBufferPtr, False)

        Dim outBufferSize = 4096
        Dim outBufferPtr = Marshal.AllocHGlobal(outBufferSize)
        Dim bytesReturned As Integer = 0

        Try
            Dim result = WOFHelper.DeviceIoControl(
                handle,
                NtfsInterop.FSCTL_GET_RETRIEVAL_POINTERS,
                inBufferPtr,
                inBufferSize,
                outBufferPtr,
                outBufferSize,
                bytesReturned,
                IntPtr.Zero
            )



            'Probably errors because the file is empty
            If Not result Then Return Long.MaxValue


            ' Marshal the output buffer to get the first LCN
            Dim extentOffset As Integer = Marshal.OffsetOf(Of RETRIEVAL_POINTERS_BUFFER)("Extents").ToInt32()
            Dim lcn As Long = Marshal.ReadInt64(outBufferPtr, extentOffset + 8)
            Return lcn

        Finally
            Marshal.FreeHGlobal(inBufferPtr)
            Marshal.FreeHGlobal(outBufferPtr)
            handle.Close()
        End Try
    End Function

End Class
