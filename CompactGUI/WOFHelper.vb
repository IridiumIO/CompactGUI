Imports System.Runtime.InteropServices

Public Module WOFHelper

    Public Const WOF_PROVIDER_FILE As ULong = 2
    Public Const FSCTL_DELETE_EXTERNAL_BACKING As UInteger = &H90314

    Public Enum Algorithms
        NO_COMPRESSION = -2
        LZNT1 = -1
        XPRESS4K = 0
        LZX = 1
        XPRESS8K = 2
        XPRESS16K = 3
    End Enum

    Public Function WOFConvertCompressionLevel(compressionlevel As Integer) As Integer

        Select Case compressionlevel
            Case 0 : Return Algorithms.XPRESS4K
            Case 1 : Return Algorithms.XPRESS8K
            Case 2 : Return Algorithms.XPRESS16K
            Case 3 : Return Algorithms.LZX
        End Select

    End Function



    Public Structure _WOF_FILE_COMPRESSION_INFO_V1
        Public Algorithm As Algorithms
        Public Flags As ULong
    End Structure

    <DllImport("WofUtil.dll")>
    Public Function WofIsExternalFile(
    <MarshalAs(UnmanagedType.LPWStr)> ByVal Filepath As String,
    <Out> ByRef IsExternalFile As Integer,
    <Out> ByRef Provider As UInteger,
    <Out> ByRef Info As _WOF_FILE_COMPRESSION_INFO_V1,
    ByRef BufferLength As UInteger) As Integer
    End Function

    <DllImport("WofUtil.dll")>
    Public Function WofSetFileDataLocation(
        FileHandle As IntPtr,
        Provider As ULong,
        ExternalFileInfo As IntPtr,
        Length As ULong) As Integer
    End Function

    'Most of these should be optional if MS Docs are to be believed -.-
    <DllImport("kernel32.dll")>
    Public Function DeviceIoControl(
        hDevice As IntPtr,
        dwIoControlCode As UInteger,
        lpInBuffer As IntPtr,
        nInBufferSize As UInteger,
        lpOutBuffer As IntPtr,
        nOutBufferSize As UInteger,
        <Out> lpBytesReturned As IntPtr,
        <Out> lpOverlapped As IntPtr) As Integer

    End Function

End Module
