Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Public Module SharedMethods
    Public Enum FolderVerificationResult
        Valid = 0
        DirectoryDoesNotExist
        SystemDirectory
        RootDirectory
        DirectoryEmptyOrUnauthorized
        OneDriveFolder
        NonNTFSDrive
        InsufficientPermission
    End Enum

    Function verifyFolder(folder As String) As FolderVerificationResult

        If Not Directory.Exists(folder) Then : Return FolderVerificationResult.DirectoryDoesNotExist
        ElseIf folder.ToLowerInvariant.Contains((Environment.GetFolderPath(Environment.SpecialFolder.Windows)).ToLowerInvariant) Then : Return FolderVerificationResult.SystemDirectory
        ElseIf folder.EndsWith(":\") Then : Return FolderVerificationResult.RootDirectory
        ElseIf IsDirectoryEmptySafe(folder) Then : Return FolderVerificationResult.DirectoryEmptyOrUnauthorized
        ElseIf IsOneDriveFolder(folder) Then : Return FolderVerificationResult.OneDriveFolder
        ElseIf DriveInfo.GetDrives().First(Function(f) folder.StartsWith(f.Name)).DriveFormat <> "NTFS" Then : Return FolderVerificationResult.NonNTFSDrive
        ElseIf Not Analyser.HasDirectoryWritePermission(folder) Then : Return FolderVerificationResult.InsufficientPermission
        End If

        Return FolderVerificationResult.Valid

    End Function


    Function GetFolderVerificationMessage(result As FolderVerificationResult) As String
        Select Case result
            Case FolderVerificationResult.Valid
                Return ""
            Case FolderVerificationResult.DirectoryDoesNotExist
                Return "Directory does not exist"
            Case FolderVerificationResult.SystemDirectory
                Return "Cannot compress system directory"
            Case FolderVerificationResult.RootDirectory
                Return "Cannot compress root directory"
            Case FolderVerificationResult.DirectoryEmptyOrUnauthorized
                Return "This directory is either empty or you are not authorized to access its files."
            Case FolderVerificationResult.OneDriveFolder
                Return "Files synced with OneDrive cannot be compressed as they use a different storage structure"
            Case FolderVerificationResult.NonNTFSDrive
                Return "Cannot compress a directory on a non-NTFS drive"
            Case FolderVerificationResult.InsufficientPermission
                Return "Insufficient permission to access this folder."
            Case Else
                Return "Unknown error"
        End Select
    End Function



    Function IsDirectoryEmptySafe(folder As String)

        Try
            Return Not Directory.EnumerateFileSystemEntries(folder).Any()


        Catch ex As UnauthorizedAccessException
            Return False

        Catch ex As Exception
            Return False
        End Try

    End Function

    Function GetFileSizeOnDisk(file As String) As Long
        Dim hosize As UInteger
        Dim losize As UInteger = GetCompressedFileSizeW(file, hosize)
        'INVALID_FILE_SIZE (0xFFFFFFFF)
        If losize = 4294967295 Then
            Dim errCode As Integer
            errCode = Marshal.GetLastPInvokeError()
            If errCode <> 0 Then
                Return -1
            End If
        End If
        Return CLng(hosize) << 32 Or losize
    End Function


    <Extension()>
    Function AsShortPathNames(filesList As IEnumerable(Of String)) As List(Of String)

        Return filesList.Select(Of String) _
            (Function(fl)
                 If fl.Length >= 255 Then
                     Dim sfp = GetShortPath(fl)
                     If sfp IsNot Nothing Then Return sfp
                 End If
                 Return fl
             End Function).ToList

    End Function


    Function GetShortPath(filePath As String) As String

        If String.IsNullOrWhiteSpace(filePath) Then Return Nothing
        Dim hasPrefix As Boolean = False

        If filePath.Length >= 255 AndAlso Not filePath.StartsWith("\\?\") Then
            filePath = "\\?\" & filePath
            hasPrefix = True
        End If

        Dim shortPath = New StringBuilder(1024)
        Dim res As Integer = GetShortPathName(filePath, shortPath, 1024)
        If res = 0 Then Return Nothing
        filePath = shortPath.ToString()
        If hasPrefix Then filePath = filePath.Substring(4)
        Return filePath

    End Function


    Function GetClusterSize(folderPath As String)

        Dim lpSectorsPerCluster As UInteger
        Dim lpBytesPerSector As UInteger
        Dim res As Integer = GetDiskFreeSpace(New DirectoryInfo(folderPath).Root.ToString, lpSectorsPerCluster, lpBytesPerSector, Nothing, Nothing)
        Return lpSectorsPerCluster * lpBytesPerSector

    End Function

    Function IsOneDriveFolder(folderPath As String) As Boolean
        Dim userProfile As String = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        Dim oneDrivePaths As New List(Of String) From {
            Path.Combine(userProfile, "OneDrive"), ' Personal OneDrive
            Path.Combine(userProfile, "OneDrive - Personal"), ' Alternative Personal OneDrive
            Path.Combine(userProfile, "OneDrive for Business"), ' OneDrive for Business
            Path.Combine(userProfile, "OneDrive - Business") ' Alternative OneDrive for Business
        }

        ' Normalize the folder path to compare
        Dim normalizedFolderPath As String = Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar).ToLowerInvariant()

        ' Check if the folder path starts with any of the known OneDrive paths
        Return oneDrivePaths.Any(Function(odPath) normalizedFolderPath.StartsWith(Path.GetFullPath(odPath).TrimEnd(Path.DirectorySeparatorChar).ToLowerInvariant()))
    End Function

    Public Sub PreventSleep()
        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS Or EXECUTION_STATE.ES_SYSTEM_REQUIRED Or EXECUTION_STATE.ES_DISPLAY_REQUIRED)
    End Sub
    Public Sub RestoreSleep()
        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS)
    End Sub


#Region "DLL Imports"

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function GetCompressedFileSizeW(
    <[In](), MarshalAs(UnmanagedType.LPWStr)> lpFileName As String,
    <Out(), MarshalAs(UnmanagedType.U4)> ByRef lpFileSizeHigh As UInteger) _
    As UInteger
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)>
    Private Function GetShortPathName(
        <MarshalAs(UnmanagedType.LPTStr)> path As String,
        <MarshalAs(UnmanagedType.LPTStr)> shortPath As StringBuilder, shortPathLength As Integer) As Integer

    End Function



    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Function GetDiskFreeSpace(
        lpRootPathName As String,
        <Out> ByRef lpSectorsPerCluster As UInteger,
        <Out> ByRef lpBytesPerSector As UInteger,
        <Out> ByRef lpNumberOfFreeClusters As UInteger,
        <Out> ByRef lpTotalNumberOfClusters As UInteger) As Boolean
    End Function


    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function SetThreadExecutionState(esFlags As EXECUTION_STATE) As EXECUTION_STATE
    End Function

    <Flags()>
    Private Enum EXECUTION_STATE As UInteger
        ES_AWAYMODE_REQUIRED = &H40
        ES_CONTINUOUS = &H80000000UI
        ES_DISPLAY_REQUIRED = &H2
        ES_SYSTEM_REQUIRED = &H1
    End Enum

#End Region


End Module
