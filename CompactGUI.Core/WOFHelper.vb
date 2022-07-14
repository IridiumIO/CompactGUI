Imports System.Runtime.InteropServices

<CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")>
Public Module WofHelper

    Public Const WofPROVIDERFILE As Long = 2
    Public Const FSCTLDELETEEXTERNALBACKING As UInteger = &H90314

    Public Function WofConvertCompressionLevel(compressionlevel As Integer) As Integer

        Select Case compressionlevel
            Case 0 : Return CompressionAlgorithm.XPRESS4K
            Case 1 : Return CompressionAlgorithm.XPRESS8K
            Case 2 : Return CompressionAlgorithm.XPRESS16K
            Case 3 : Return CompressionAlgorithm.LZX
        End Select

        Return CompressionAlgorithm.NOCOMPRESSION

    End Function

    Public Function WofConvertBackCompressionLevel(WofCompressionLevel As CompressionAlgorithm) As Integer

        Select Case WofCompressionLevel
            Case CompressionAlgorithm.XPRESS4K : Return 0
            Case CompressionAlgorithm.XPRESS8K : Return 1
            Case CompressionAlgorithm.XPRESS16K : Return 2
            Case CompressionAlgorithm.LZX : Return 3
            Case Else : Return 0
        End Select

    End Function

    Public Structure WofFILECOMPRESSIONINFOV1
        Public _algorithm As CompressionAlgorithm
        Public _flags As Long
    End Structure


    <DllImport("WofUtil.dll")>
    Public Function WofIsExternalFile(
    <MarshalAs(UnmanagedType.LPWStr)> filepath As String,
    <Out> ByRef isExternalFile As Integer,
    <Out> ByRef provider As UInteger,
    <Out> ByRef info As WofFILECOMPRESSIONINFOV1,
    ByRef bufferLength As UInteger) As Long
    End Function

    <DllImport("WofUtil.dll")>
    Public Function WofSetFileDataLocation(
        fileHandle As IntPtr,
        provider As Long,
        externalFileInfo As IntPtr,
        length As Integer) As Integer
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
