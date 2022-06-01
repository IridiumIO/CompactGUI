Imports System.Threading
Imports MethodTimer

Public Class Compactor

    Structure CompressionLevel
        Const XPRESS4K = "/EXE:XPRESS4K"
        Const XPRESS8K = "/EXE:XPRESS8K"
        Const XPRESS16K = "/EXE:XPRESS16K"
        Const LZX = "/EXE:LZX"
    End Structure

    Private _filesList As List(Of String)
    Private _workingDir As String
    Private _compressionLevel As String

    Sub New(folder As String, compressionLevel As String)

        If Not verifyFolder(folder) Then : Return : End If

        _workingDir = folder
        _filesList = IO.Directory.GetFiles(folder, "*.*", IO.SearchOption.AllDirectories).ToList
        _compressionLevel = compressionLevel

    End Sub


    Private Sub GenerateThread(compactArgs)

        Dim myProc = New Process
        With myProc.StartInfo
            .FileName = "compact.exe"
            .WorkingDirectory = _workingDir
            .UseShellExecute = False
            .CreateNoWindow = True
            .Arguments = compactArgs
        End With

        myProc.Start()
        myProc.WaitForExit()

    End Sub


    Async Function RunCompactAsync(progress As IProgress(Of (percentageProgress As Integer, currentFile As String))) As Task

        Dim compactArgs = "/C /I " & _compressionLevel

        Dim totalFiles As Integer = _filesList.Count
        Dim count As Integer = 0
        Await Parallel.ForEachAsync(_filesList,
                                    Function(file, _ctx)
                                        GenerateThread(compactArgs & " " & """" & file & """")
                                        Dim result = Interlocked.Increment(count)
                                        progress.Report((CInt(((result / totalFiles) * 100)), file))
                                    End Function).ConfigureAwait(False)
        Return

    End Function


    Shared Function verifyFolder(folder As String) As Boolean

        If Not IO.Directory.Exists(folder) Then : Return False
        ElseIf folder.Contains(":\Windows") Then : Return False
        ElseIf folder.EndsWith(":\") Then : Return False
        End If

        Return True

    End Function

    <Time>
    Friend Shared Async Function AnalyseFolder(folder As String, hasCompressionRun As Boolean) As Task(Of (uncompressed As Long, compressed As Long, containsCompressedFiles As Boolean, fileCompressionDetailsList As List(Of FileDetails)))

        Dim compressedBytes As Long
        Dim compressedFiles As Integer
        Dim uncompressedBytes As Long
        Dim uncompressedFiles As Integer

        Dim allFiles = IO.Directory.EnumerateFiles(folder, "*.*", New IO.EnumerationOptions() With {.RecurseSubdirectories = True})

        Dim fDetails As New Concurrent.ConcurrentBag(Of FileDetails)

        Dim GetRawFileSizes = Task.Run(Sub() uncompressedFiles = allFiles.Count)

        Dim GetCompressedFileSizes = Parallel.ForEachAsync(allFiles,
                                        Function(file, ctx)

                                            Dim compSize = GetFileSizeOnDisk(file)
                                            Dim uncompSize = New IO.FileInfo(file).Length
                                            Interlocked.Add(compressedBytes, compSize)
                                            Interlocked.Add(uncompressedBytes, uncompSize)
                                            fDetails.Add(New FileDetails With {.FileName = file, .CompressedSize = compSize, .UncompressedSize = uncompSize})
                                            If compSize < uncompSize Then Interlocked.Increment(compressedFiles)
                                        End Function)


        Await Task.WhenAll(GetRawFileSizes, GetCompressedFileSizes)

        Debug.WriteLine($"{compressedFiles} / {uncompressedFiles} Compressed")


        Return (uncompressedBytes, compressedBytes, If(compressedFiles = 0, False, True), fDetails.ToList())


    End Function

End Class

Class FileDetails

    Public FileName As String
    Public UncompressedSize As Long
    Public CompressedSize As Long


End Class
