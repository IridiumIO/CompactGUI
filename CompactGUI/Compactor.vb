Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading
Imports MethodTimer

Public Class Compactor

    Friend Shared CompressionLevel As New Dictionary(Of Integer, String) From {
        {0, "XPRESS4K"},
        {1, "XPRESS8K"},
        {2, "XPRESS16K"},
        {3, "LZX"}}

    Private _filesList As IEnumerable(Of String)
    Private _workingDir As String
    Private _compressionLevel As String
    Private _excludedFileTypes As List(Of String)
    Private _WOFcompressionLevel As ULong

    Private _EFInfo As _WOF_FILE_COMPRESSION_INFO_V1
    Private _EFInfoPtr As IntPtr

    Sub New(folder As String, cLevelIndex As Integer, excludedfiletypes As List(Of String))

        If Not verifyFolder(folder) Then : Return : End If

        _workingDir = folder
        _excludedFileTypes = excludedfiletypes
        _compressionLevel = CompressionLevel(cLevelIndex)
        _WOFcompressionLevel = WOFConvertCompressionLevel(cLevelIndex)
        _EFInfo = New _WOF_FILE_COMPRESSION_INFO_V1 With {.Algorithm = _WOFcompressionLevel, .Flags = 0}
        _EFInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_EFInfo))
        Marshal.StructureToPtr(_EFInfo, _EFInfoPtr, True)

    End Sub

    Async Function BuildWorkingFilesList() As Task

        Dim clusterSize As Integer = GetClusterSize(_workingDir)

        _filesList = Await Task.Run(Function() IO.Directory.EnumerateFiles(_workingDir, "*", New IO.EnumerationOptions With {.RecurseSubdirectories = True, .IgnoreInaccessible = True}) _
            .AsShortPathNames.Where(Function(st)
                                        Dim ft = New IO.FileInfo(st)
                                        If Not _excludedFileTypes.Contains(ft.Extension) AndAlso ft.Length > clusterSize Then Return True
                                        Return False
                                    End Function))

    End Function

    Friend Shared Sub GenerateThread(workingdir, compactArgs)

        Dim myProc = New Process
        With myProc.StartInfo
            .FileName = "compact.exe"
            .WorkingDirectory = workingdir
            .UseShellExecute = False
            .CreateNoWindow = True
            .Arguments = compactArgs
        End With

        myProc.Start()
        myProc.WaitForExit()

    End Sub


    Shared Function DetectCompression(path As String)

        Dim isextFile As Integer
        Dim prov As ULong
        Dim info As _WOF_FILE_COMPRESSION_INFO_V1
        Dim buf As UInt16 = 8
        Dim ret = WofIsExternalFile(path, isextFile, prov, info, buf)

    End Function


    Sub WOFCompressFile(path As String)

        Dim length As ULong = Marshal.SizeOf(_EFInfoPtr)

        Using fs As FileStream = New FileStream(path, FileMode.Open)
            Dim hFile = fs.SafeFileHandle.DangerousGetHandle()
            Dim res = WofSetFileDataLocation(hFile, WOF_PROVIDER_FILE, _EFInfoPtr, length)
            If res <> 0 Then
                If res <> -2147024552 Then
                    Debug.WriteLine("spaghetti")
                End If

            End If
        End Using

    End Sub


    <Time>
    Async Function RunCompactAsync(progress As IProgress(Of (percentageProgress As Integer, currentFile As String))) As Task(Of Boolean)

        Await BuildWorkingFilesList()

        Dim compactArgs = "/C /I /EXE:" & _compressionLevel
        Dim totalFiles As Integer = Await Task.Run(Function() _filesList.Count)
        Dim count As Integer = 0

        Await Parallel.ForEachAsync(_filesList,
                                    Function(file, _ctx)
                                        WOFCompressFile(file)
                                        'GenerateThread(_workingDir, compactArgs & " " & """" & file & """")
                                        Dim result = Interlocked.Increment(count)
                                        progress.Report((CInt(((result / totalFiles) * 100)), file))
                                    End Function).ConfigureAwait(False)
        Return True

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

        Dim allFiles = IO.Directory.EnumerateFiles(folder, "*", New IO.EnumerationOptions() With {.RecurseSubdirectories = True, .IgnoreInaccessible = True}).AsShortPathNames
        Dim fDetails As New Concurrent.ConcurrentBag(Of FileDetails)
        Dim GetRawFileSizes = Task.Run(Sub() uncompressedFiles = allFiles.Count)

        Dim GetCompressedFileSizes = Task.Run(
            Function() Parallel.ForEach(allFiles,
                          Sub(file)
                              Dim compSize = GetFileSizeOnDisk(file)
                              Dim uncompSize = New IO.FileInfo(file).Length
                              Interlocked.Add(compressedBytes, compSize)
                              Interlocked.Add(uncompressedBytes, uncompSize)
                              fDetails.Add(New FileDetails With {.FileName = file, .CompressedSize = compSize, .UncompressedSize = uncompSize})
                              If compSize < uncompSize Then Interlocked.Increment(compressedFiles)
                          End Sub))

        Await Task.WhenAll(GetRawFileSizes, GetCompressedFileSizes)



        Return (uncompressedBytes, compressedBytes, If(compressedFiles = 0, False, True), fDetails.ToList())

    End Function


    Friend Shared Async Function UncompressFolder(workingDir As String, filesList As List(Of String), progress As IProgress(Of (percentageProgress As Integer, currentFile As String))) As Task

        Dim compactArgs = "/U /EXE "
        Dim totalFiles As Integer = filesList.Count
        Dim count As Integer = 0
        Await Parallel.ForEachAsync(filesList,
                                    Function(file, _ctx)
                                        GenerateThread(workingDir, compactArgs & " " & """" & file & """")
                                        Dim result = Interlocked.Increment(count)
                                        progress.Report((CInt(((result / totalFiles) * 100)), file))
                                    End Function).ConfigureAwait(False)
        Return


    End Function






End Class
