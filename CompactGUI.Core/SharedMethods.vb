Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Public Module SharedMethods


    Function verifyFolder(folder As String) As Boolean

        If Not IO.Directory.Exists(folder) Then : Return False
        ElseIf folder.Contains(":\Windows") Then : Return False
        ElseIf folder.EndsWith(":\") Then : Return False
        End If

        Return True

    End Function

    Function GetFileSizeOnDisk(file As String) As Long
        Dim hosize As UInteger
        Dim losize As UInteger = GetCompressedFileSizeW(file, hosize)
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



#Region "DLL Imports"

    <DllImport("kernel32.dll")>
    Private Function GetCompressedFileSizeW(
    <[In](), MarshalAs(UnmanagedType.LPWStr)> lpFileName As String,
    <Out(), MarshalAs(UnmanagedType.U4)> ByRef lpFileSizeHigh As UInteger) _
    As UInteger
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)>
    Private Function GetShortPathName(
        <MarshalAs(UnmanagedType.LPTStr)> ByVal path As String,
        <MarshalAs(UnmanagedType.LPTStr)> ByVal shortPath As StringBuilder, ByVal shortPathLength As Integer) As Integer

    End Function



    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Function GetDiskFreeSpace(
        ByVal lpRootPathName As String,
        <Out> ByRef lpSectorsPerCluster As UInteger,
        <Out> ByRef lpBytesPerSector As UInteger,
        <Out> ByRef lpNumberOfFreeClusters As UInteger,
        <Out> ByRef lpTotalNumberOfClusters As UInteger) As Boolean
    End Function


#End Region


End Module
