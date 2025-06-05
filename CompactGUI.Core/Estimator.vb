Imports System.IO
Imports System.IO.Compression

Imports K4os.Compression.LZ4

Imports K4os.Compression.LZ4.Streams


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


    Public Function EstimateCompressability(analysisResult As List(Of AnalysedFileDetails), ishdd As Boolean, Optional MaxParallelism As Integer = 1, Optional clusterSize As Integer = 4096, Optional cancellationToken As Threading.CancellationToken = Nothing) As List(Of (AnalysedFile As AnalysedFileDetails, CompressionRatio As Single))

        DiskClusterSize = clusterSize
        Dim _filesList As New Concurrent.ConcurrentBag(Of FileDetails)
        If MaxParallelism <= 0 Then MaxParallelism = Environment.ProcessorCount

        Me.IsHDD = ishdd

        Dim paraOptions As New ParallelOptions With {.MaxDegreeOfParallelism = MaxParallelism}

        Parallel.ForEach(analysisResult, parallelOptions:=paraOptions, Sub(fl)

                                                                           If fl.UncompressedSize > clusterSize Then
                                                                               If cancellationToken <> Nothing AndAlso cancellationToken.IsCancellationRequested Then
                                                                                   Throw New OperationCanceledException(cancellationToken)
                                                                               End If
                                                                               Dim estimatedRatio = EstimateCompressabilityLZ4(fl.FileName, fl.UncompressedSize, cancellationToken)

                                                                               _filesList.Add((New FileDetails With {.AnalysedFile = fl, .CompressionRatio = estimatedRatio}))
                                                                           End If
                                                                       End Sub)

        Dim retList As New List(Of (AnalysedFile As AnalysedFileDetails, CompressionRatio As Single))

        For Each item In _filesList
            retList.Add((item.AnalysedFile, item.CompressionRatio))
        Next

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
                    If cancellationToken <> Nothing AndAlso cancellationToken.IsCancellationRequested Then
                        Throw New OperationCanceledException(cancellationToken)
                    End If
                    compressionStream.Write(buffer, 0, bytesRead)
                    totalWritten += bytesRead
                    bytesRead = input.Read(buffer, 0, BlockSize)
                End While

            Else
                ' Sample evenly spaced blocks
                Dim stepSize As Long = BlockSize * (totalBlocks \ samplesNeeded)
                Dim buffer(BlockSize - 1) As Byte
                For i As Integer = 0 To samplesNeeded - 1
                    If cancellationToken <> Nothing AndAlso cancellationToken.IsCancellationRequested Then
                        Throw New OperationCanceledException(cancellationToken)
                    End If
                    input.Position = stepSize * i
                    Dim bytesRead As Integer = input.Read(buffer, 0, BlockSize)
                    compressionStream.Write(buffer, 0, bytesRead)
                    totalWritten += bytesRead
                Next
            End If
        End Using

        Return Math.Min(compressed.Length / Math.Max(totalWritten, 1), 1.0)
    End Function

    'Private Function EstimateCompressabilityHDD(input As FileStream, fileSize As Long, compressionFactory As CompressionStreamFactory, Optional cancellationToken As Threading.CancellationToken = Nothing) As Double
    '    Dim MiddleChunkSize As Integer = SampleSize * BlockSize ' 10KB

    '    Dim totalWritten As Long = 0
    '    Dim compressed = New MemoryStream()

    '    Using compressionStream As Stream = compressionFactory(compressed)
    '        ' If file is smaller than 10KB, just use the whole file
    '        Dim chunkSize As Integer = CInt(Math.Min(MiddleChunkSize, fileSize))
    '        Dim middleStart As Long = Math.Max(0, (fileSize \ 2) - (chunkSize \ 2))

    '        Dim buffer(chunkSize - 1) As Byte
    '        input.Position = middleStart
    '        Dim bytesRead As Integer = input.Read(buffer, 0, chunkSize)

    '        If cancellationToken <> Nothing AndAlso cancellationToken.IsCancellationRequested Then
    '            Throw New OperationCanceledException(cancellationToken)
    '        End If

    '        If bytesRead > 0 Then
    '            compressionStream.Write(buffer, 0, bytesRead)
    '            totalWritten += bytesRead
    '        End If
    '    End Using

    '    Return Math.Min(compressed.Length / Math.Max(totalWritten, 1), 1.0)
    'End Function

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



End Class