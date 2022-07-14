Imports System.IO
Imports System.Threading

Public Class Uncompactor

    Public Shared Async Function UncompactFiles(filesList As List(Of String), Optional progressMonitor As IProgress(Of (percentageProgress As Integer, currentFile As String)) = Nothing) As Task(Of Boolean)

        Dim totalFiles As Integer = filesList.Count
        Dim processedFiles As Integer = 0

        Await Parallel.ForEachAsync(filesList,
                                    Function(file, _ctx)
                                        WofDecompressFile(file)
                                        Dim incremented = Interlocked.Increment(processedFiles)
                                        progressMonitor.Report((CInt(incremented / totalFiles * 100), file))
                                    End Function).ConfigureAwait(False)
        Return True
    End Function

    Private Shared Function WofDecompressFile(path As String) As Integer

        Try
            Using fs As New FileStream(path, FileMode.Open)
                Dim hDevice = fs.SafeFileHandle.DangerousGetHandle
                Dim res = DeviceIoControl(hDevice, FSCTLDELETEEXTERNALBACKING, IntPtr.Zero, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero)
                Return res
            End Using
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
            Return Nothing
        End Try

    End Function

End Class
