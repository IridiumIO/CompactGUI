Imports System.IO
Imports System.Security.AccessControl
Imports System.Threading

Public Class Analyser

    Public Sub New(folder As String)
        FolderName = folder
    End Sub

    Public Property FolderName As String
    Public Property UncompressedBytes As Long
    Public Property CompressedBytes As Long
    Public Property ContainsCompressedFiles As Boolean
    Public Property FileCompressionDetailsList As List(Of AnalysedFileDetails)
    Private _testField As Integer = 0

    Public Async Function AnalyseFolder(cancellationToken As CancellationToken) As Task(Of Boolean)

        Dim allFiles = Await Task.Run(Function() Directory.EnumerateFiles(FolderName, "*", New EnumerationOptions() With {.RecurseSubdirectories = True, .IgnoreInaccessible = True}).AsShortPathNames, cancellationToken).ConfigureAwait(False)
        Dim compressedFilesCount As Integer
        Dim fileDetails As New Concurrent.ConcurrentBag(Of AnalysedFileDetails)

        Try
            Await Task.Run(Function() Parallel.ForEach(allFiles, New ParallelOptions With {.CancellationToken = cancellationToken}, Sub(file) AnalyseFile(file, compressedFilesCount, fileDetails)))
        Catch ex As OperationCanceledException
            Debug.WriteLine(ex.Message)
            Return Nothing
        End Try

        ContainsCompressedFiles = compressedFilesCount <> 0
        FileCompressionDetailsList = fileDetails.ToList
        Return ContainsCompressedFiles

    End Function


    Private Sub AnalyseFile(file As String, ByRef compressedFilesCount As Integer, ByRef fileDetails As Concurrent.ConcurrentBag(Of AnalysedFileDetails))
        Dim fInfo As New FileInfo(file)
        Dim unCompSize = fInfo.Length
        Dim compSize = GetFileSizeOnDisk(file)
        Dim cLevel As CompressionAlgorithm = If(compSize = unCompSize, CompressionAlgorithm.NOCOMPRESSION, DetectCompression(fInfo))

        'Sets the backing private fields directly because Interlocked doesn't play nice with properties!
        Interlocked.Add(_CompressedBytes, compSize)
        Interlocked.Add(_UncompressedBytes, unCompSize)
        Interlocked.Add(_testField, 1)
        fileDetails.Add(New AnalysedFileDetails With {.FileName = file, .CompressedSize = compSize, .UncompressedSize = unCompSize, .CompressionMode = cLevel})
        If cLevel <> CompressionAlgorithm.NOCOMPRESSION Then Interlocked.Increment(compressedFilesCount)
    End Sub


    Public Async Function GetPoorlyCompressedExtensions() As Task(Of List(Of ExtensionResult))
        Dim extClassResults As List(Of ExtensionResult) = Await Task.Run(
            Function()
                Dim extRes As New List(Of ExtensionResult)
                For Each fl In FileCompressionDetailsList
                    Dim fInfo As New IO.FileInfo(fl.FileName)
                    Dim xt = fInfo.Extension
                    If fl.UncompressedSize = 0 Then Continue For
                    Dim obj = extRes.FirstOrDefault(Function(x) x.extension = xt, Nothing)
                    If obj Is Nothing Then
                        extRes.Add(New ExtensionResult With {.extension = xt, .totalFiles = 1, .uncompressedBytes = fl.UncompressedSize, .compressedBytes = fl.CompressedSize})
                        Continue For
                    End If
                    obj.uncompressedBytes += fl.UncompressedSize
                    obj.compressedBytes += fl.CompressedSize
                    obj.totalFiles += 1
                Next
                Return extRes
            End Function)

        Return extClassResults.Where(Function(f) f.cRatio > 0.95).ToList()
    End Function

    Private Shared Function DetectCompression(fInfo As FileInfo) As CompressionAlgorithm

        Dim isextFile As Integer
        Dim prov As UInteger
        Dim info As WofFILECOMPRESSIONINFOV1
        Dim buf As UInteger = 8

        WofIsExternalFile(fInfo.FullName, isextFile, prov, info, buf)
        If isextFile = 0 Then info._algorithm = CompressionAlgorithm.NOCOMPRESSION
        If (fInfo.Attributes And 2048) <> 0 Then info._algorithm = CompressionAlgorithm.LZNT1
        Return info._algorithm

    End Function


    Public Function HasDirectoryWritePermission() As Boolean

        Try
            Dim writeAllow = False

            Dim ACRules = New DirectoryInfo(FolderName).GetAccessControl().GetAccessRules(True, True, GetType(Security.Principal.NTAccount))
            If ACRules Is Nothing Then Return False

            Dim identity = Security.Principal.WindowsIdentity.GetCurrent
            Dim principal = New Security.Principal.WindowsPrincipal(identity)
            For Each FSRule As FileSystemAccessRule In ACRules

                If (FSRule.FileSystemRights And FileSystemRights.Write) <= 0 Then Continue For
                Dim ntAccount As Security.Principal.NTAccount = CType(FSRule.IdentityReference, Security.Principal.NTAccount)
                If ntAccount Is Nothing Then Continue For
                If Not principal.IsInRole(ntAccount.Value) Then Continue For
                If FSRule.AccessControlType = AccessControlType.Deny Then Return False
                writeAllow = True

            Next

            Return writeAllow

        Catch ex As System.UnauthorizedAccessException

            Return False

        End Try

    End Function

End Class
