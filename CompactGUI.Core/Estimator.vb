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

    Public Sub New()
    End Sub


    Public Delegate Function CompressionStreamFactory(output As Stream) As Stream


    Public Class FileDetails
        Public AnalysedFile As AnalysedFileDetails
        Public CompressionRatio As Single
    End Class


    Public Function EstimateCompressability(analysisResult As List(Of AnalysedFileDetails), Optional MaxParallelism As Integer = 1, Optional clusterSize As Integer = 4096, Optional cancellationToken As Threading.CancellationToken = Nothing) As List(Of (AnalysedFile As AnalysedFileDetails, CompressionRatio As Single))

        Dim _filesList As New Concurrent.ConcurrentBag(Of FileDetails)
        If MaxParallelism <= 0 Then MaxParallelism = Environment.ProcessorCount

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





    'Public Function EstimateEntropy(path As String) As Double
    '    Using fs As FileStream = File.OpenRead(path)
    '        Return EntropyCompressionRatio(fs)
    '    End Using
    'End Function

    'Public Function EntropyCompressionRatio(input As Stream) As Double
    '    Dim counts(255) As Long
    '    Dim total As Long = 0
    '    Dim bufferSize As Integer = 65536 ' 64 KB buffer for efficiency
    '    Dim buffer(bufferSize - 1) As Byte

    '    input.Position = 0
    '    Dim bytesRead As Integer

    '    Do
    '        bytesRead = input.Read(buffer, 0, bufferSize)
    '        If bytesRead = 0 Then Exit Do
    '        For i As Integer = 0 To bytesRead - 1
    '            counts(buffer(i)) += 1
    '        Next
    '        total += bytesRead
    '    Loop While bytesRead > 0

    '    If total = 0 Then Return 1.0 ' empty stream, treat as incompressible

    '    Dim entropy As Double = 0.0
    '    For i = 0 To 255
    '        If counts(i) > 0 Then
    '            Dim p = counts(i) / total
    '            entropy -= p * Math.Log(p, 2)
    '        End If
    '    Next
    '    ' Convert entropy (bits/byte) to compression ratio
    '    Return Math.Min(entropy / 8.0, 1.0)
    'End Function

    'Public Function EstimateZlibCompressionRatio(filePath As String, fileSize As Long) As Double

    '    Dim blocks As Single = fileSize \ BlockSize
    '    Dim samples As Integer = Math.Ceiling((blocks * N0) / (N0 + blocks - 1))
    '    Dim skipBlocks As Integer = 1
    '    If samples > 0 Then
    '        skipBlocks = Math.Max(1, blocks \ samples)
    '    End If



    '    Dim totalOriginal As Long = 0
    '    Dim totalCompressed As Long = 0

    '    Using inputStream As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
    '        ' Create a memory stream to hold the compressed output
    '        Using compressedOut As New MemoryStream()
    '            Using zlibStream As New ZLibStream(compressedOut, CompressionLevel.Fastest, True)
    '                Dim buffer(BlockSize - 1) As Byte
    '                Dim bytesRead As Integer
    '                Dim blockIndex As Integer = 0

    '                Do
    '                    bytesRead = inputStream.Read(buffer, 0, BlockSize)
    '                    If bytesRead <= 0 Then Exit Do

    '                    If blockIndex Mod skipBlocks = 0 Then
    '                        zlibStream.Write(buffer, 0, bytesRead)
    '                        totalOriginal += bytesRead
    '                    End If

    '                    blockIndex += 1
    '                Loop

    '                zlibStream.Flush()
    '            End Using
    '            totalCompressed = compressedOut.Length
    '        End Using
    '    End Using

    '    If totalOriginal = 0 Then Return 1.0 ' Prevent division by zero
    '    Return Math.Round(totalCompressed / totalOriginal, 3)
    'End Function

End Class