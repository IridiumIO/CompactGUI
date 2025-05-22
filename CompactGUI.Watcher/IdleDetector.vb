Imports System.Runtime.InteropServices
Imports System.Threading

Public Class IdleDetector

    Public Shared Event IsIdle As EventHandler
    Public Shared Event IsNotIdle As EventHandler

    Private Shared _timerTask As Task
    Private Shared _idletimer As PeriodicTimer
    Private Shared ReadOnly _cts As New CancellationTokenSource

    Public Shared Property Paused As Boolean = False

    Public Shared Property IsEnabled As Boolean = True

    Shared Sub New()
        _idletimer = New PeriodicTimer(TimeSpan.FromSeconds(5))
    End Sub

    Public Shared Sub Start()
        If _timerTask Is Nothing OrElse _timerTask.IsCompleted Then _timerTask = IdleTimerDoWorkAsync()
    End Sub

    Public Shared Async Sub StopAsync()
        _cts.Cancel()
        If _timerTask IsNot Nothing Then Await _timerTask
        _idletimer.Dispose()
        _cts.Dispose()
    End Sub

    Private Shared Async Function IdleTimerDoWorkAsync() As Task

        Try
            While Await _idletimer.WaitForNextTickAsync(_cts.Token) AndAlso Not _cts.Token.IsCancellationRequested

                If GetIdleTime() > 300 AndAlso Not Paused AndAlso IsEnabled Then
                    RaiseEvent IsIdle(Nothing, EventArgs.Empty)

                ElseIf Not Paused AndAlso IsEnabled Then
                    RaiseEvent IsNotIdle(Nothing, EventArgs.Empty)
                End If

            End While
        Catch ex As OperationCanceledException
            Return
        End Try

    End Function

    Public Shared Function GetIdleTime() As Double
        Dim lastInputInfo As New LASTINPUTINFO() With {.cbSize = CType(Marshal.SizeOf(GetType(LASTINPUTINFO)), UInteger)}
        If Not GetLastInputInfo(lastInputInfo) Then Return 0

        Dim idleTicks = Environment.TickCount64 - CLng(lastInputInfo.dwTime)
        Return TimeSpan.FromMilliseconds(idleTicks).TotalSeconds

    End Function


    <DllImport("user32.dll")>
    Private Shared Function GetLastInputInfo(ByRef plii As LASTINPUTINFO) As Boolean
    End Function


    Public Structure LASTINPUTINFO
        Public cbSize As UInteger
        Public dwTime As UInteger
    End Structure

End Class