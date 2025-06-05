Imports System.Runtime.InteropServices

Imports Microsoft.Win32.SafeHandles

Public Module WOFHelper

    Public Const WOF_PROVIDER_FILE As ULong = 2
    Public Const FSCTL_DELETE_EXTERNAL_BACKING As UInteger = &H90314

    Public Function WOFConvertCompressionLevel(compressionlevel As Integer) As Integer

        Select Case compressionlevel
            Case 0 : Return WOFCompressionAlgorithm.XPRESS4K
            Case 1 : Return WOFCompressionAlgorithm.XPRESS8K
            Case 2 : Return WOFCompressionAlgorithm.XPRESS16K
            Case 3 : Return WOFCompressionAlgorithm.LZX
            Case Else : Return WOFCompressionAlgorithm.XPRESS4K
        End Select

    End Function

    Public Function WOFConvertCompressionLevel(compressionlevel As CompressionMode) As Integer
        Select Case compressionlevel
            Case CompressionMode.XPRESS4K : Return WOFCompressionAlgorithm.XPRESS4K
            Case CompressionMode.XPRESS8K : Return WOFCompressionAlgorithm.XPRESS8K
            Case CompressionMode.XPRESS16K : Return WOFCompressionAlgorithm.XPRESS16K
            Case CompressionMode.LZX : Return WOFCompressionAlgorithm.LZX
            Case Else : Return WOFCompressionAlgorithm.XPRESS4K
        End Select
    End Function


    Public Function WOFConvertBackCompressionLevel(WOFCompressionLevel As WOFCompressionAlgorithm) As Integer

        Select Case WOFCompressionLevel
            Case WOFCompressionAlgorithm.XPRESS4K : Return 0
            Case WOFCompressionAlgorithm.XPRESS8K : Return 1
            Case WOFCompressionAlgorithm.XPRESS16K : Return 2
            Case WOFCompressionAlgorithm.LZX : Return 3
            Case Else : Return 0
        End Select

    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure WOF_FILE_COMPRESSION_INFO_V1
        Public Algorithm As UInteger
        Public Flags As UInteger
    End Structure


    <DllImport("WofUtil.dll")>
    Friend Function WofIsExternalFile(
    <MarshalAs(UnmanagedType.LPWStr)> Filepath As String,
    <Out> ByRef IsExternalFile As Integer,
    <Out> ByRef Provider As UInteger,
    <Out> ByRef Info As WOF_FILE_COMPRESSION_INFO_V1,
    ByRef BufferLength As UInteger) As Integer
    End Function

    <DllImport("WofUtil.dll")>
    Friend Function WofSetFileDataLocation(
        FileHandle As SafeFileHandle,
        Provider As ULong,
        ExternalFileInfo As IntPtr,
        Length As ULong) As Integer
    End Function

    'Most of these should be optional if MS Docs are to be believed -.-
    <DllImport("kernel32.dll")>
    Friend Function DeviceIoControl(
        hDevice As SafeFileHandle,
        dwIoControlCode As UInteger,
        lpInBuffer As IntPtr,
        nInBufferSize As UInteger,
        lpOutBuffer As IntPtr,
        nOutBufferSize As UInteger,
        <Out> ByRef lpBytesReturned As IntPtr,
        <Out> lpOverlapped As IntPtr) As Boolean

    End Function

End Module
