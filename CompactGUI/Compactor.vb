Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading
Imports MethodTimer

Public Class Compactor


    Private _filesList As IEnumerable(Of String)
    Private _workingDir As String
    Private _excludedFileTypes() As String
    Private _WOFcompressionLevel As ULong

    Private _EFInfo As _WOF_FILE_COMPRESSION_INFO_V1
    Private _EFInfoPtr As IntPtr

    Sub New(folder As String, cLevelIndex As Integer, excludedfiletypes As String())

        If Not verifyFolder(folder) Then Return

        _workingDir = folder
        _excludedFileTypes = excludedfiletypes
        _WOFcompressionLevel = WOFConvertCompressionLevel(cLevelIndex)
        _EFInfo = New _WOF_FILE_COMPRESSION_INFO_V1 With {.Algorithm = _WOFcompressionLevel, .Flags = 0}
        _EFInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_EFInfo))
        Marshal.StructureToPtr(_EFInfo, _EFInfoPtr, True)

    End Sub

    <Time>
    Async Function BuildWorkingFilesList() As Task

        Dim clusterSize As Integer = GetClusterSize(_workingDir)

        _filesList = Await Task.Run(Function() Directory.EnumerateFiles(_workingDir, "*", New IO.EnumerationOptions With {.RecurseSubdirectories = True, .IgnoreInaccessible = True}) _
                                                .Where(Function(st)
                                                           Dim ft = New IO.FileInfo(st)
                                                           If Not _excludedFileTypes.Contains(ft.Extension) AndAlso ft.Length > clusterSize Then Return True
                                                           Return False
                                                       End Function).AsShortPathNames)

    End Function


    Function WOFCompressFile(path As String)

        Dim length As ULong = Marshal.SizeOf(_EFInfoPtr)

        Using fs As FileStream = New FileStream(path, FileMode.Open)
            Dim hFile = fs.SafeFileHandle.DangerousGetHandle()
            Dim res = WofSetFileDataLocation(hFile, WOF_PROVIDER_FILE, _EFInfoPtr, length)
            Return res
        End Using

    End Function

    Shared Function WOFDecompressFile(path As String)

        Using fs As FileStream = New FileStream(path, FileMode.Open)
            Dim hDevice = fs.SafeFileHandle.DangerousGetHandle
            Dim res = DeviceIoControl(hDevice, FSCTL_DELETE_EXTERNAL_BACKING, IntPtr.Zero, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero)
            Return res
        End Using

    End Function

    <Time>
    Async Function RunCompactAsync(progress As IProgress(Of (percentageProgress As Integer, currentFile As String))) As Task(Of Boolean)

        Await BuildWorkingFilesList()

        Dim totalFiles As Integer = _filesList.Count
        Dim count As Integer = 0

        Await Parallel.ForEachAsync(_filesList,
                                    Function(file, _ctx)
                                        Dim res = WOFCompressFile(file)
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
    Friend Shared Async Function AnalyseFolder(folder As String) As Task(Of (uncompressed As Long, compressed As Long, containsCompressedFiles As Boolean, fileCompressionDetailsList As List(Of FileDetails)))

        Dim compressedBytes As Long
        Dim compressedFiles As Integer
        Dim uncompressedBytes As Long
        Dim uncompressedFiles As Integer

        Dim allFiles = Directory.EnumerateFiles(folder, "*", New IO.EnumerationOptions() With {.RecurseSubdirectories = True, .IgnoreInaccessible = True}).AsShortPathNames
        Dim fDetails As New Concurrent.ConcurrentBag(Of FileDetails)
        Dim GetRawFileSizes = Task.Run(Sub() uncompressedFiles = allFiles.Count)

        Dim GetCompressedFileSizes = Task.Run(
            Function() Parallel.ForEach(allFiles,
                          Sub(file)
                              Dim fInfo = New IO.FileInfo(file)
                              Dim uncompSize = fInfo.Length
                              Dim compSize = GetFileSizeOnDisk(file)
                              Dim cLevel As Algorithms = If(compSize = uncompSize, Algorithms.NO_COMPRESSION, DetectCompression(fInfo))

                              Interlocked.Add(compressedBytes, compSize)
                              Interlocked.Add(uncompressedBytes, uncompSize)
                              fDetails.Add(New FileDetails With {.FileName = file, .CompressedSize = compSize, .UncompressedSize = uncompSize, .CompressionMode = cLevel})
                              If compSize < uncompSize Then Interlocked.Increment(compressedFiles)
                          End Sub))

        Await Task.WhenAll(GetRawFileSizes, GetCompressedFileSizes)

        Return (uncompressedBytes, compressedBytes, compressedFiles <> 0, fDetails.ToList())

    End Function
    Shared Function DetectCompression(fInfo As FileInfo) As Algorithms

        Dim isextFile As Integer
        Dim prov As ULong
        Dim info As _WOF_FILE_COMPRESSION_INFO_V1
        Dim buf As UInt16 = 8
        Dim xt = fInfo.Attributes And 2048
        Dim ret = WofIsExternalFile(fInfo.FullName, isextFile, prov, info, buf)
        If isextFile = 0 Then info.Algorithm = Algorithms.NO_COMPRESSION
        If (fInfo.Attributes And 2048) <> 0 Then info.Algorithm = Algorithms.LZNT1
        Return info.Algorithm

    End Function


    Friend Shared Async Function UncompressFolder(workingDir As String, filesList As List(Of String), progress As IProgress(Of (percentageProgress As Integer, currentFile As String))) As Task

        Dim totalFiles As Integer = filesList.Count
        Dim count As Integer = 0
        Await Parallel.ForEachAsync(filesList,
                                    Function(file, _ctx)
                                        Dim res = WOFDecompressFile(file)
                                        Dim result = Interlocked.Increment(count)
                                        progress.Report((CInt(((result / totalFiles) * 100)), file))
                                    End Function).ConfigureAwait(False)
        Return


    End Function






End Class
