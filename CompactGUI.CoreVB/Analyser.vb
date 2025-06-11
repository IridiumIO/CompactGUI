Imports System.IO
Imports System.Security.AccessControl
Imports System.Security.Principal
Imports System.Threading

Public Class Analyser

    Public Property FolderName As String
    Public Property UncompressedBytes As Long
    Public Property CompressedBytes As Long
    Public Property ContainsCompressedFiles As Boolean
    Public Property FileCompressionDetailsList As List(Of AnalysedFileDetails)

    Public Sub New(folder As String)
        FolderName = folder
    End Sub


    Public Async Function AnalyseFolder(cancellationToken As CancellationToken) As Task(Of Boolean)
        Dim allFiles = Await Task.Run(Function() Directory.EnumerateFiles(FolderName, "*", New EnumerationOptions() With {.RecurseSubdirectories = True, .IgnoreInaccessible = True}).AsShortPathNames, cancellationToken).ConfigureAwait(False)
        Dim fileDetails As New List(Of AnalysedFileDetails)
        Dim compressedFilesCount As Integer = 0

        ' Use local variables to reduce contention
        Dim localCompressedBytes As Long = 0
        Dim localUncompressedBytes As Long = 0

        Try
            Parallel.ForEach(allFiles, New ParallelOptions With {.CancellationToken = cancellationToken},
                            Sub(file)
                                Dim details = AnalyseFile(file)
                                If details IsNot Nothing Then
                                    SyncLock fileDetails
                                        fileDetails.Add(details)
                                    End SyncLock
                                    If details.CompressionMode <> WOFCompressionAlgorithm.NO_COMPRESSION Then
                                        Interlocked.Increment(compressedFilesCount)
                                    End If
                                    Interlocked.Add(localCompressedBytes, details.CompressedSize)
                                    Interlocked.Add(localUncompressedBytes, details.UncompressedSize)
                                End If
                            End Sub)

            ' Update the shared state after the parallel loop to minimize contention
            CompressedBytes = localCompressedBytes
            UncompressedBytes = localUncompressedBytes
        Catch ex As OperationCanceledException
            Debug.WriteLine(ex.Message)
            Return Nothing
        End Try

        ContainsCompressedFiles = compressedFilesCount > 0
        FileCompressionDetailsList = fileDetails
        Return ContainsCompressedFiles
    End Function


    Private Function AnalyseFile(file As String) As AnalysedFileDetails
        Try
            Dim fInfo As New FileInfo(file)
            Dim unCompSize = fInfo.Length
            Dim compSize = GetFileSizeOnDisk(file)
            If compSize < 0 Then
                compSize = unCompSize ' GetFileSizeOnDisk failed, fall back to unCompSize
            End If
            Dim cLevel As WOFCompressionAlgorithm = If(compSize = unCompSize, WOFCompressionAlgorithm.NO_COMPRESSION, DetectCompression(fInfo))

            Return New AnalysedFileDetails With {.FileName = file, .CompressedSize = compSize, .UncompressedSize = unCompSize, .CompressionMode = cLevel, .FileInfo = fInfo}
        Catch ex As IOException
            Debug.WriteLine($"Error analysing file {file}: {ex.Message}")
            Return Nothing
        End Try
    End Function


    Public Async Function GetPoorlyCompressedExtensions() As Task(Of List(Of ExtensionResult))
        Dim extClassResults As New List(Of ExtensionResult)
        Await Task.Run(
        Sub()
            Dim extRes As New Concurrent.ConcurrentDictionary(Of String, ExtensionResult)
            Parallel.ForEach(FileCompressionDetailsList,
                               Sub(fl)
                                   Dim xt = New FileInfo(fl.FileName).Extension
                                   If fl.UncompressedSize = 0 Then Return

                                   extRes.AddOrUpdate(xt,
                                        Function(addKey) ' Add value factory
                                            Return New ExtensionResult With {
                                            .extension = xt,
                                            .totalFiles = 1,
                                            .uncompressedBytes = fl.UncompressedSize,
                                            .compressedBytes = fl.CompressedSize
                                        }
                                        End Function,
                                        Function(updateKey, oldValue) ' Update value factory
                                            Return New ExtensionResult With {
                                            .extension = xt,
                                            .totalFiles = oldValue.totalFiles + 1,
                                            .uncompressedBytes = oldValue.uncompressedBytes + fl.UncompressedSize,
                                            .compressedBytes = oldValue.compressedBytes + fl.CompressedSize
                                        }
                                        End Function)
                               End Sub)

            ' Filter and convert to list after aggregation
            extClassResults = extRes.Values.Where(Function(f) f.cRatio > 0.95).ToList()
        End Sub)

        Return extClassResults
    End Function


    Private Function DetectCompression(fInfo As FileInfo) As WOFCompressionAlgorithm

        Dim isextFile As Integer
        Dim prov As ULong
        Dim info As WOF_FILE_COMPRESSION_INFO_V1
        Dim buf As UInt16 = 8

        Dim ret = WofIsExternalFile(fInfo.FullName, isextFile, prov, info, buf)

        Dim algorithm As WOFCompressionAlgorithm = info.Algorithm

        If isextFile = 0 Then algorithm = WOFCompressionAlgorithm.NO_COMPRESSION
        If (fInfo.Attributes And 2048) <> 0 Then algorithm = WOFCompressionAlgorithm.LZNT1
        Return algorithm

    End Function


    Public Shared Function HasDirectoryWritePermission(FolderName As String) As Boolean
        Try
            Dim directoryInfo = New DirectoryInfo(FolderName)
            Dim directorySecurity = directoryInfo.GetAccessControl()

            Dim user = WindowsIdentity.GetCurrent()
            Dim userSID = user.User
            Dim userGroupSIDs = user.Groups

            Dim accessRules = directorySecurity.GetAccessRules(True, True, GetType(SecurityIdentifier))

            Dim writeAllowed = False
            Dim writeDenied = False

            For Each rule As FileSystemAccessRule In accessRules
                Dim fileSystemRights = rule.FileSystemRights
                If (fileSystemRights And FileSystemRights.Write) > 0 Then
                    Dim ruleSID = DirectCast(rule.IdentityReference, SecurityIdentifier)

                    ' Check if the rule applies to the user or any of the user's groups
                    If ruleSID.Equals(userSID) OrElse userGroupSIDs.Contains(ruleSID) Then
                        If rule.AccessControlType = AccessControlType.Allow Then
                            writeAllowed = True
                        ElseIf rule.AccessControlType = AccessControlType.Deny Then
                            writeDenied = True
                            Exit For
                        End If
                    End If
                End If
            Next

            ' Write permission is considered available if it's explicitly allowed and not explicitly denied
            Return writeAllowed And Not writeDenied
        Catch ex As UnauthorizedAccessException

            Return False
        End Try
    End Function



End Class
