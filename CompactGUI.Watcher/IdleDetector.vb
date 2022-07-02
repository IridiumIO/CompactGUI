Imports System.Runtime.InteropServices
Imports System.Threading

Public Class IdleDetector

    Public Event IsIdle As EventHandler

    Private _timerTask As Task
    Private _idletimer As PeriodicTimer
    Private ReadOnly _cts As New CancellationTokenSource


    Sub New()
        _idletimer = New PeriodicTimer(TimeSpan.FromMilliseconds(30000))
    End Sub

    Public Sub Start()
        _timerTask = IdleTimerDoWorkAsync()
    End Sub

    Public Async Sub StopAsync()
        _cts.Cancel()
        Await _timerTask
        _cts.Dispose()
    End Sub

    Private Async Function IdleTimerDoWorkAsync() As Task

        While Await _idletimer.WaitForNextTickAsync
            If GetIdleTime() > 300 Then RaiseEvent IsIdle(Me, Nothing)
        End While

    End Function

    Public Shared Function GetIdleTime()

        Dim idleTicks = 0

        Dim lastInputInfo As LASTINPUTINFO = New LASTINPUTINFO()
        lastInputInfo.cbSize = CType(Marshal.SizeOf(lastInputInfo), UInteger)
        lastInputInfo.dwTime = 0

        If GetLastInputInfo(lastInputInfo) Then idleTicks = Environment.TickCount64 - CLng(lastInputInfo.dwTime)
        Return TimeSpan.FromMilliseconds(idleTicks).TotalSeconds

    End Function


    <DllImport("user32.dll")>
    Public Shared Function GetLastInputInfo(ByRef plii As LASTINPUTINFO) As Boolean
    End Function


    Public Structure LASTINPUTINFO
        Public cbSize As UInteger
        Public dwTime As UInteger
    End Structure

End Class