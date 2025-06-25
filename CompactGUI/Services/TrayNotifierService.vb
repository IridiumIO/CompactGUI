Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Public Class TrayNotifierService
    Private Const NIM_ADD As Integer = &H0
    Private Const NIM_MODIFY As Integer = &H1
    Private Const NIM_DELETE As Integer = &H2

    Private Const NIF_MESSAGE As Integer = &H1
    Private Const NIF_ICON As Integer = &H2
    Private Const NIF_TIP As Integer = &H4
    Private Const NIF_INFO As Integer = &H10

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Private Structure NOTIFYICONDATA
        Public cbSize As Integer
        Public hWnd As IntPtr
        Public uID As Integer
        Public uFlags As Integer
        Public uCallbackMessage As Integer
        Public hIcon As IntPtr
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
        Public szTip As String
        Public dwState As Integer
        Public dwStateMask As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=256)>
        Public szInfo As String
        Public uTimeoutOrVersion As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=64)>
        Public szInfoTitle As String
        Public dwInfoFlags As Integer
        Public guidItem As Guid
        Public hBalloonIcon As IntPtr
    End Structure

    <DllImport("shell32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function Shell_NotifyIcon(dwMessage As Integer, ByRef lpdata As NOTIFYICONDATA) As Boolean
    End Function

    Private m_data As NOTIFYICONDATA
    Private m_icon As Icon

    Public Sub New(window As Window, icon As Icon, tooltip As String)

        Application.Current.Dispatcher.Invoke(
            Sub()

                Dim helper As New WindowInteropHelper(window)
                m_icon = icon
                m_data = New NOTIFYICONDATA()
                m_data.cbSize = Marshal.SizeOf(m_data)
                m_data.hWnd = helper.Handle
                m_data.uID = 1
                m_data.uFlags = NIF_MESSAGE Or NIF_ICON Or NIF_TIP
                m_data.uCallbackMessage = 0
                m_data.hIcon = icon.Handle
                m_data.szTip = tooltip
                m_data.szInfo = ""
                m_data.szInfoTitle = ""
                m_data.dwInfoFlags = 0
                m_data.guidItem = Guid.Empty
            End Sub)


    End Sub

    Public Sub ShowBalloon(title As String, message As String, Optional infoFlags As Integer = 0)

        m_data.uFlags = NIF_INFO Or NIF_ICON Or NIF_TIP
        m_data.szInfoTitle = title
        m_data.szInfo = message
        m_data.dwInfoFlags = infoFlags ' 1=Info, 2=Warning, 3=Error

        Shell_NotifyIcon(NIM_ADD, m_data)
        Shell_NotifyIcon(NIM_DELETE, m_data)
    End Sub

    Public Sub Dispose()
        Shell_NotifyIcon(NIM_DELETE, m_data)
        If m_icon IsNot Nothing Then
            m_icon.Dispose()
        End If
    End Sub

    Public Sub Notify_Compressed(DisplayName As String, BytesSaved As Long, CompressionRatio As Decimal)
        Dim title = $"{DisplayName}"
        Dim readableSaved = $"{New BytesToReadableConverter().Convert(BytesSaved, GetType(Long), Nothing, Globalization.CultureInfo.CurrentCulture)} saved"
        Dim percentCompressed = $"{100 - CInt(CompressionRatio * 100)}% smaller"
        ShowBalloon(title, $"▸ {readableSaved}{Environment.NewLine}▸ {percentCompressed}")
    End Sub
    
    Public Sub Notify_ScheduledTaskCompleted(taskName As String)
        Dim title = "Scheduled Task Completed"
        Dim message = $"The scheduled task '{taskName}' has been completed successfully."
        ShowBalloon(title, message)
    End Sub
    
    Public Sub Notify_ScheduledTaskFailed(taskName As String, errorMessage As String)
        Dim title = "Scheduled Task Failed"
        Dim message = $"The scheduled task '{taskName}' has failed: {errorMessage}"
        ShowBalloon(title, message, 3) ' Error icon
    End Sub
    
    Public Sub Notify_BatchJobCompleted(jobName As String, folderCount As Integer, spaceSaved As Long)
        Dim title = "Batch Job Completed"
        Dim readableSaved = $"{New BytesToReadableConverter().Convert(spaceSaved, GetType(Long), Nothing, Globalization.CultureInfo.CurrentCulture)}"
        Dim message = $"The batch job '{jobName}' has completed processing {folderCount} folder(s).{Environment.NewLine}Total space saved: {readableSaved}"
        ShowBalloon(title, message)
    End Sub
    
    Public Sub Notify_BatchJobFailed(jobName As String, errorCount As Integer)
        Dim title = "Batch Job Failed"
        Dim message = $"The batch job '{jobName}' has failed with {errorCount} error(s)."
        ShowBalloon(title, message, 3) ' Error icon
    End Sub
    
    Public Sub ShowMessage(title As String, message As String)
        ShowBalloon(title, message)
    End Sub
    
    Public Sub ShowError(title As String, message As String)
        ShowBalloon(title, message, 3) ' Error icon
    End Sub

End Class
