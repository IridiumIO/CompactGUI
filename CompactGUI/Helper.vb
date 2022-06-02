Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Imports Gameloop.Vdf
Imports MethodTimer

Module Helper



    Function GetFileSizeOnDisk(file As String) As Long
        Dim info As New FileInfo(file)

        Dim hosize As UInteger
        Dim losize As UInteger = GetCompressedFileSizeW(file, hosize)
        Return CLng(hosize) << 32 Or losize

    End Function

    <DllImport("kernel32.dll")>
    Private Function GetCompressedFileSizeW(
        <[In](), MarshalAs(UnmanagedType.LPWStr)> lpFileName As String,
        <Out(), MarshalAs(UnmanagedType.U4)> ByRef lpFileSizeHigh As UInteger) _
        As UInteger
    End Function


    <Time>
    Function GetSteamIDFromFolder(path As String) As Integer

        Dim workingDir = New IO.DirectoryInfo(path)
        Dim parentfolder = workingDir.Parent.Parent

        If Not parentfolder?.Name = "steamapps" Then Return 0

        For Each file In parentfolder.EnumerateFiles("*.acf").Where(Function(f) f.Length > 0)
            Dim vConv = VdfConvert.Deserialize(IO.File.ReadAllText(file.FullName))

            If vConv.Value.Item("installdir").ToString = workingDir.Name Then
                Dim vr = CInt(vConv.Value.Item("appid").ToString)
                Return vr
                'TODO: Maybe add check to see when game was last updated?
            End If

        Next

        Return 0

    End Function

    <Extension()>
    Function AsShortPathNames(filesList As IEnumerable(Of String)) As IEnumerable(Of String)

        Return filesList.Select(Of String)(Function(fl)
                                               If fl.Length >= 255 Then
                                                   Dim sfp = GetShortPath(fl)
                                                   If sfp IsNot Nothing Then Return sfp
                                               End If
                                               Return fl
                                           End Function)


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

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)>
    Private Function GetShortPathName(
    <MarshalAs(UnmanagedType.LPTStr)> ByVal path As String,
    <MarshalAs(UnmanagedType.LPTStr)> ByVal shortPath As StringBuilder, ByVal shortPathLength As Integer) As Integer

    End Function

End Module
