Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading

Public Class Compactor

    Public Sub New(folder As String, cLevel As CompressionAlgorithm, excludedFilesTypes As String())

        If Not verifyFolder(folder) Then Return

        _workingDir = folder
        _excludedFileTypes = excludedFilesTypes
        _WOFCompressionLevel = cLevel

        _EFInfo = New _WOF_FILE_COMPRESSION_INFO_V1 With {.Algorithm = _WOFCompressionLevel, .Flags = 0}
        _EFInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_EFInfo))
        Marshal.StructureToPtr(_EFInfo, _EFInfoPtr, True)

    End Sub

    Private _workingDir As String
    Private _excludedFileTypes() As String
    Private _WOFCompressionLevel As CompressionAlgorithm

    Private _EFInfo As _WOF_FILE_COMPRESSION_INFO_V1
    Private _EFInfoPtr As IntPtr



    Public Async Function RunCompactAsync(Optional progressMonitor As IProgress(Of (percentageProgress As Integer, currentFile As String)) = Nothing) As Task(Of Boolean)

        Dim FilesList = Await BuildWorkingFilesList()
        Dim totalFiles As Integer = FilesList.Count
        Dim processedFileCount As Integer = 0

        Await Parallel.ForEachAsync(FilesList,
                                    Function(file, _ctx)
                                        Dim res = WOFCompressFile(file)
                                        Dim incremented = Interlocked.Increment(processedFileCount)
                                        progressMonitor.Report((CInt(((incremented / totalFiles) * 100)), file))
                                    End Function).ConfigureAwait(False)

        Return True
    End Function


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

    Private Async Function BuildWorkingFilesList() As Task(Of IEnumerable(Of String))

        Dim clusterSize As Integer = GetClusterSize(_workingDir)

        Dim _filesList = Await Task.Run(Function() Directory.EnumerateFiles(_workingDir, "*", New EnumerationOptions With {.RecurseSubdirectories = True, .IgnoreInaccessible = True}) _
                                                .Where(Function(st)
                                                           Dim ft = New FileInfo(st)
                                                           If Not _excludedFileTypes.Contains(ft.Extension) AndAlso ft.Length > clusterSize Then Return True
                                                           Return False
                                                       End Function).AsShortPathNames)
        Return _filesList
    End Function


    Private Function verifyFolder(folder As String) As Boolean

        If Not IO.Directory.Exists(folder) Then : Return False
        ElseIf folder.Contains(":\Windows") Then : Return False
        ElseIf folder.EndsWith(":\") Then : Return False
        End If

        Return True

    End Function

End Class
