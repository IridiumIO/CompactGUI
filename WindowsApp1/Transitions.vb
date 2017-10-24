Public Class FadeTransition

    Shared FadeObj
    Shared FadeStartOp
    Shared FadeStopOp
    Shared FadeTickCount
    Shared FadeDirection
    Shared Modifier

    Shared Bounds As Decimal
    Shared FadeTickCumulation
    Shared FadeTimer As New Timer
    Shared SW As New Stopwatch


    ''' <summary>
    ''' Fades the target form into or out of view
    ''' </summary>
    ''' <param name="targetForm">Choose the target form</param>
    ''' <param name="startopacity">Set start opacity as a decimal between 0 and 1</param>
    ''' <param name="endopacity">Set end opacity as a decimal between 0 and 1</param>
    ''' <param name="duration">Set duration of transition in milliseconds</param>
    Public Shared Sub FadeForm(targetForm As Form, startopacity As Decimal, endopacity As Decimal, duration As Integer)

        If duration <= 30 Then Modifier = 10
        If duration > 30 And duration < 1000 Then Modifier = 25
        If duration >= 1000 Then Modifier = 50

        FadeTimer.Interval = Modifier

        FadeObj = targetForm
        FadeObj.opacity = startopacity
        FadeStartOp = startopacity
        FadeStopOp = endopacity

        FadeTickCount = (endopacity - startopacity) / (duration / Modifier)

        If endopacity - startopacity < 0 Then
            Bounds = -FadeTickCount
            FadeTickCumulation = -FadeTickCount
        Else
            Bounds = FadeTickCount
            FadeTickCumulation = FadeTickCount
        End If

        AddHandler FadeTimer.Tick, AddressOf FadeTimer_Tick
        FadeTimer.Start()

    End Sub

    Shared Sub FadeTimer_Tick(sender As Object, e As EventArgs)
        If FadeObj.opacity > 0.01 Then FadeObj.show
        If FadeObj.opacity < 0.01 Then FadeObj.hide
        If FadeObj.opacity < FadeStopOp + Bounds And FadeObj.opacity > FadeStopOp - Bounds Then
            FadeTimer.Stop()
            FadeObj.opacity = FadeStopOp
            RemoveHandler FadeTimer.Tick, AddressOf FadeTimer_Tick

            If FadeObj.opacity = 0 Then FadeObj.close
        Else
            FadeObj.opacity += FadeTickCount
            FadeTickCumulation += FadeTickCount
        End If

    End Sub




End Class