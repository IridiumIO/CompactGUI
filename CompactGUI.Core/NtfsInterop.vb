Imports System.Runtime.InteropServices

Imports Microsoft.Win32.SafeHandles

Friend Module NtfsInterop
    Public Const FSCTL_GET_RETRIEVAL_POINTERS As UInteger = &H90073
    Public Const OPEN_EXISTING As Integer = 3
    Public Const FILE_FLAG_BACKUP_SEMANTICS As Integer = &H2000000
    Public Const FILE_SHARE_READ As Integer = 1
    Public Const FILE_SHARE_WRITE As Integer = 2
    Public Const GENERIC_READ As Integer = &H80000000

    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
    Public Function CreateFile(
        lpFileName As String,
        dwDesiredAccess As Integer,
        dwShareMode As Integer,
        lpSecurityAttributes As IntPtr,
        dwCreationDisposition As Integer,
        dwFlagsAndAttributes As Integer,
        hTemplateFile As IntPtr
    ) As SafeFileHandle
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure STARTING_VCN_INPUT_BUFFER
        Public StartingVcn As Long
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure RETRIEVAL_POINTERS_BUFFER
        Public ExtentCount As Integer
        Public StartingVcn As Long
        Public Extents As LCN_EXTENT ' This is actually an array, but we only need the first. ?Perhaps I should get the LCN of the middle cluster in future?
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure LCN_EXTENT
        Public NextVcn As Long
        Public Lcn As Long
    End Structure
End Module