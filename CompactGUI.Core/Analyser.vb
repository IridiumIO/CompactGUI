Imports System.IO
Imports System.Security.AccessControl
Imports System.Security.Principal
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
            Dim res = Await Task.Run(Function() Parallel.ForEach(allFiles, New ParallelOptions With {.CancellationToken = cancellationToken}, Sub(file) AnalyseFile(file, compressedFilesCount, fileDetails)))
        Catch ex As OperationCanceledException
            Debug.WriteLine(ex.Message)
            Return Nothing
        End Try

        ContainsCompressedFiles = compressedFilesCount <> 0
        FileCompressionDetailsList = fileDetails.ToList
        Return ContainsCompressedFiles

    End Function


    Private Sub AnalyseFile(file As String, ByRef compressedFilesCount As Integer, ByRef fileDetails As Concurrent.ConcurrentBag(Of AnalysedFileDetails))

        Try
            Dim fInfo As New FileInfo(file)
            Dim unCompSize = fInfo.Length
            Dim compSize = GetFileSizeOnDisk(file)
            If compSize < 0 Then
                'GetFileSizeOnDisk failed, fall back to unCompSize
                compSize = unCompSize
            End If
            Dim cLevel As CompressionAlgorithm = If(compSize = unCompSize, CompressionAlgorithm.NO_COMPRESSION, DetectCompression(fInfo))

            'Sets the backing private fields directly because Interlocked doesn't play nice with properties!
            Interlocked.Add(_CompressedBytes, compSize)
            Interlocked.Add(_UncompressedBytes, unCompSize)
            Interlocked.Add(_testField, 1)
            fileDetails.Add(New AnalysedFileDetails With {.FileName = file, .CompressedSize = compSize, .UncompressedSize = unCompSize, .CompressionMode = cLevel})
            If cLevel <> CompressionAlgorithm.NO_COMPRESSION Then Interlocked.Increment(compressedFilesCount)
        Catch ex As IOException
            Debug.WriteLine($"Error analysing file {file}: {ex.Message}")
        End Try

    End Sub
    Public Async Function GetPoorlyCompressedExtensions() As Task(Of List(Of ExtensionResult))
        Dim extClassResults As New List(Of ExtensionResult)
        Await Task.Run(
        Sub()
            Dim extRes As New Concurrent.ConcurrentDictionary(Of String, ExtensionResult)
            Parallel.ForEach(FileCompressionDetailsList,
                               Sub(fl)
                                   Dim xt = New IO.FileInfo(fl.FileName).Extension
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

    Private Function DetectCompression(fInfo As FileInfo) As CompressionAlgorithm

        Dim isextFile As Integer
        Dim prov As ULong
        Dim info As _WOF_FILE_COMPRESSION_INFO_V1
        Dim buf As UInt16 = 8

        Dim ret = WofIsExternalFile(fInfo.FullName, isextFile, prov, info, buf)
        If isextFile = 0 Then info.Algorithm = CompressionAlgorithm.NO_COMPRESSION
        If (fInfo.Attributes And 2048) <> 0 Then info.Algorithm = CompressionAlgorithm.LZNT1
        Return info.Algorithm

    End Function



    'Public Function HasDirectoryWritePermission() As Boolean

    '    Try
    '        Dim ACRules = New DirectoryInfo(FolderName).GetAccessControl().GetAccessRules(True, True, GetType(Security.Principal.SecurityIdentifier))

    '        Dim identity = Security.Principal.WindowsIdentity.GetCurrent
    '        Dim principal = New Security.Principal.WindowsPrincipal(identity)
    '        Dim writeDenied = False

    '        For Each FSRule As FileSystemAccessRule In ACRules
    '            If (FSRule.FileSystemRights And FileSystemRights.Write) = 0 Then Continue For


    '            ' Use Translate to safely convert to NTAccount
    '            Dim ntAccount As Security.Principal.NTAccount = Nothing
    '            Try
    '                ntAccount = DirectCast(FSRule.IdentityReference.Translate(GetType(Security.Principal.NTAccount)), System.Security.Principal.NTAccount)
    '            Catch ex As Exception
    '                Continue For
    '            End Try

    '            If ntAccount Is Nothing OrElse Not principal.IsInRole(ntAccount.Value) Then Continue For

    '            If FSRule.AccessControlType = AccessControlType.Deny Then
    '                writeDenied = True
    '                Exit For
    '            End If

    '        Next

    '        Return Not writeDenied
    '    Catch ex As UnauthorizedAccessException
    '        
    '        Return False
    '    End Try

    'End Function

    Public Function HasDirectoryWritePermission() As Boolean
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
