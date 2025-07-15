Imports System.Runtime.InteropServices
Imports System.Threading

Public Enum IdleState
    Idle
    NotIdle
End Enum

Public Class IdleDetector

    Public Event IsIdle As EventHandler
    Public Event IsNotIdle As EventHandler

    Private ReadOnly _settings As IdleSettings

    Private  _timerTask As Task
    Private _idletimer As PeriodicTimer
    Private _cts As CancellationTokenSource

    Public Property State As IdleState
    Public Property LastIdleTime As DateTime = DateTime.MinValue


    Public Sub New(settings As IdleSettings)
        _settings = settings
    End Sub


    Public Async Sub Start()
        Await StopAsync()
        _cts = New CancellationTokenSource()
        _idletimer = New PeriodicTimer(TimeSpan.FromSeconds(_settings.IdleCheckIntervalSeconds))
        _timerTask = IdleTimerDoWorkAsync()
        State = IdleState.NotIdle
    End Sub

    Public Async Function StopAsync() As Task
        _cts?.Cancel()
        _idletimer?.Dispose()
        _idletimer = Nothing
        If _timerTask IsNot Nothing Then Await _timerTask
        _cts?.Dispose()
        _cts = Nothing
    End Function

    Private Async Function IdleTimerDoWorkAsync() As Task
        Try
            While Await _idletimer.WaitForNextTickAsync(_cts.Token) AndAlso Not _cts.Token.IsCancellationRequested

                If GetIdleTime() > _settings.IdleThresholdSeconds Then
                    If State <> IdleState.Idle OrElse DateTime.Now.AddSeconds(-_settings.IdleRepeatTimeSeconds) > LastIdleTime Then
                        State = IdleState.Idle
                        LastIdleTime = DateTime.Now
                        RaiseEvent IsIdle(Nothing, EventArgs.Empty)
                    End If

                Else
                    If State = IdleState.Idle Then
                        State = IdleState.NotIdle
                        RaiseEvent IsNotIdle(Nothing, EventArgs.Empty)
                    End If

                End If

            End While
        Catch ex As OperationCanceledException
            Return
        End Try

    End Function

    Public Shared Function GetIdleTime() As Double
        Dim lastInputInfo As New LASTINPUTINFO() With {.cbSize = CType(Marshal.SizeOf(Of LASTINPUTINFO)(), UInteger)}
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