Imports System.Runtime.InteropServices

Module WOFHelper

    Public Const WOF_PROVIDER_FILE As ULong = 2
    Public Const FILE_PROVIDER_COMPRESSION_XPRESS4K As ULong = 0
    Public Const FILE_PROVIDER_COMPRESSION_LZX As ULong = 1
    Public Const FILE_PROVIDER_COMPRESSION_XPRESS8K As ULong = 2
    Public Const FILE_PROVIDER_COMPRESSION_XPRESS16K As ULong = 3
    Public Const FSCTL_DELETE_EXTERNAL_BACKING As UInteger = &H90314


    Public Function WOFConvertCompressionLevel(compressionlevel As Integer) As ULong

        Select Case compressionlevel
            Case 0 : Return FILE_PROVIDER_COMPRESSION_XPRESS4K
            Case 1 : Return FILE_PROVIDER_COMPRESSION_XPRESS8K
            Case 2 : Return FILE_PROVIDER_COMPRESSION_XPRESS16K
            Case 3 : Return FILE_PROVIDER_COMPRESSION_LZX
        End Select

    End Function



    Public Structure _WOF_FILE_COMPRESSION_INFO_V1
        Public Algorithm As ULong
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
