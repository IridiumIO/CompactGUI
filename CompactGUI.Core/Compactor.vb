Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading

Public Class Compactor

    Public Sub New(folder As String, cLevel As CompressionAlgorithm, excludedFilesTypes As String())

        If Not verifyFolder(folder) Then Return

        _workingDir = folder
        _excludedFileTypes = excludedFilesTypes
        _WofCompressionLevel = cLevel

        _EFInfo = New WofFILECOMPRESSIONINFOV1 With {._algorithm = _WofCompressionLevel, ._flags = 0}
        _EFInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_EFInfo))
        Marshal.StructureToPtr(_EFInfo, _EFInfoPtr, True)

    End Sub

    Private ReadOnly _workingDir As String
    Private ReadOnly _excludedFileTypes As String()
    Private ReadOnly _WofCompressionLevel As CompressionAlgorithm

    Private _EFInfo As WofFILECOMPRESSIONINFOV1
    Private ReadOnly _EFInfoPtr As IntPtr



    Public Async Function RunCompactAsync(Optional progressMonitor As IProgress(Of (percentageProgress As Integer, currentFile As String)) = Nothing) As Task(Of Boolean)

        Dim FilesList = Await BuildWorkingFilesList()
        Dim totalFiles As Integer = FilesList.Count
        Dim processedFileCount As Integer = 0

        Await Parallel.ForEachAsync(FilesList,
                                    Function(file, _ctx)
                                        WofCompressFile(file)
                                        Dim incremented = Interlocked.Increment(processedFileCount)
                                        progressMonitor.Report((CInt(incremented / totalFiles * 100), file))
                                    End Function).ConfigureAwait(False)

        Return True
    End Function


    Private Function WofCompressFile(path As String) As Integer

        Dim length As Integer = Marshal.SizeOf(_EFInfoPtr)
        Try
            Using fs As New FileStream(path, FileMode.Open)
                ' TODO: https://rules.sonarsource.com/vbnet/RSPEC-3869
                Dim hFile = fs.SafeFileHandle.DangerousGetHandle()
                Dim res = WofSetFileDataLocation(hFile, WofPROVIDERFILE, _EFInfoPtr, length)
                Return res
            End Using
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
            Return Nothing
        End Try

    End Function

    Private Async Function BuildWorkingFilesList() As Task(Of IEnumerable(Of String))

        Dim clusterSize As Long = GetClusterSize(_workingDir)

        Dim _filesList As New Concurrent.ConcurrentBag(Of String)
        ' TODO: if the user has already analysed within the last minute, then skip creating a new one and use the old one
        Dim ax As New Analyser(_workingDir)
        Await ax.AnalyseFolder(Nothing)

        Parallel.ForEach(ax.FileCompressionDetailsList, Sub(fl)
                                                            Dim ft = New FileInfo(fl.FileName)
                                                            If Not _excludedFileTypes.Contains(ft.Extension) AndAlso ft.Length > clusterSize AndAlso fl.CompressionMode <> _WofCompressionLevel Then _filesList.Add(fl.FileName)
                                                        End Sub)


        Return _filesList.ToList
    End Function



End Class
