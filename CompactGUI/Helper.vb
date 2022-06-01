Imports System.IO
Imports System.Runtime.InteropServices
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


End Module
