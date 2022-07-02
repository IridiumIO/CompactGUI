Imports System.Runtime.InteropServices

Public Module WOFHelper

    Public Const WOF_PROVIDER_FILE As ULong = 2
    Public Const FSCTL_DELETE_EXTERNAL_BACKING As UInteger = &H90314

    Public Function WOFConvertCompressionLevel(compressionlevel As Integer) As Integer

        Select Case compressionlevel
            Case 0 : Return CompressionAlgorithm.XPRESS4K
            Case 1 : Return CompressionAlgorithm.XPRESS8K
            Case 2 : Return CompressionAlgorithm.XPRESS16K
            Case 3 : Return CompressionAlgorithm.LZX
        End Select

    End Function


    Public Function WOFConvertBackCompressionLevel(WOFCompressionLevel As CompressionAlgorithm) As Integer

        Select Case WOFCompressionLevel
            Case CompressionAlgorithm.XPRESS4K : Return 0
            Case CompressionAlgorithm.XPRESS8K : Return 1
            Case CompressionAlgorithm.XPRESS16K : Return 2
            Case CompressionAlgorithm.LZX : Return 3
            Case Else : Return 0
        End Select

    End Function

    Public Structure _WOF_FILE_COMPRESSION_INFO_V1
        Public Algorithm As CompressionAlgorithm
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
