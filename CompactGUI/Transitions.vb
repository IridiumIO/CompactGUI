Class FadeTransition

    Shared FadeObj
    Shared FadeStartOp
    Shared FadeStopOp
    Shared FadeTickCount
    Shared FadeDirection
    Shared Modifier
    Shared FadeCloseOnFinish
    Shared Bounds As Decimal
    Shared FadeTickCumulation
    Shared FadeTimer As New Timer


    ''' <summary>
    ''' Fades the target form into or out of view
    ''' </summary>
    ''' <param name="targetForm">Choose the target form</param>
    ''' <param name="startopacity">Set start opacity as a decimal between 0 and 1</param>
    ''' <param name="endopacity">Set end opacity as a decimal between 0 and 1</param>
    ''' <param name="duration">Set duration of transition in milliseconds</param>
    Shared Sub FadeForm(targetForm As Form, startopacity As Decimal, endopacity As Decimal, duration As Integer, Optional closeonFinish As Boolean = False)

        If duration <= 30 Then Modifier = 10
        If duration > 30 And duration < 1000 Then Modifier = 25
        If duration >= 1000 Then Modifier = 50

        FadeTimer.Interval = Modifier

        FadeObj = targetForm
        FadeObj.opacity = startopacity
        FadeStartOp = startopacity
        FadeStopOp = endopacity
        FadeCloseOnFinish = closeonFinish
        FadeTickCount = (endopacity - startopacity) / (duration / Modifier)

        If endopacity - startopacity < 0 Then
            Bounds = -FadeTickCount
            FadeTickCumulation = -FadeTickCount
        Else
            Bounds = FadeTickCount
            FadeTickCumulation = FadeTickCount
        End If
        FadeObj.Show()
        AddHandler FadeTimer.Tick, AddressOf FadeTimer_Tick
        FadeTimer.Start()

    End Sub

    Shared Sub FadeTimer_Tick(sender As Object, e As EventArgs)

        If FadeObj.opacity < FadeStopOp + Bounds And FadeObj.opacity > FadeStopOp - Bounds Then
            FadeTimer.Stop()
            FadeObj.opacity = FadeStopOp
            RemoveHandler FadeTimer.Tick, AddressOf FadeTimer_Tick

            If FadeObj.opacity = 0 And FadeCloseOnFinish = True Then FadeObj.Close
            If FadeObj.opacity = 0 And FadeCloseOnFinish = False Then FadeObj.hide
        Else
            FadeObj.opacity += FadeTickCount
            FadeTickCumulation += FadeTickCount
        End If

    End Sub

End Class


Class UnfurlTransition

    Shared UnfurlObj As Control
    Shared UnfurlStartWidth As Integer
    Shared UnfurlEndWidth As Integer
    Shared UnfurlDuration As Integer
    Shared Modifier As Decimal = 16 + 2 / 3
    Shared UnfurlTimer As New Timer With {.Interval = Modifier}
    Shared UnfurlTickcount
    Shared Bounds As Decimal

    Public Shared Sub UnfurlControl(target As Control, startwidth As Integer, endwidth As Integer, duration As Integer)
        If endwidth - startwidth <> 0 Then
            UnfurlObj = target
            UnfurlStartWidth = startwidth
            UnfurlEndWidth = endwidth
            UnfurlDuration = duration
            UnfurlTickcount = ((endwidth - startwidth)) / (duration / Modifier)

            If endwidth - startwidth < 0 Then
                Bounds = -UnfurlTickcount
            Else
                Bounds = UnfurlTickcount
            End If

            UnfurlObj.Width = startwidth
            UnfurlObj.Show()
            AddHandler UnfurlTimer.Tick, AddressOf UnfurlTimer_Tick
            UnfurlTimer.Start()
        End If
    End Sub


    Shared Sub UnfurlTimer_Tick(sender As Object, e As EventArgs)
        If UnfurlObj.Width < UnfurlEndWidth + Bounds And UnfurlObj.Width > UnfurlEndWidth - Bounds Then
            UnfurlTimer.Stop()
            UnfurlObj.Width = UnfurlEndWidth

            If UnfurlObj Is Compact.topbar_dirchooserContainer Then
                Compact.btnAnalyze.Visible = True
                Compact.panel_topBar.Anchor -= AnchorStyles.Bottom
                Compact.panel_topBar.Height = 135
                UnfurlObj.Location = New Point(44, 69)
            End If

            RemoveHandler UnfurlTimer.Tick, AddressOf UnfurlTimer_Tick
        Else
            UnfurlObj.Width += UnfurlTickcount
        End If
    End Sub


End Class

Public Class PaintPercentageTransition
    Shared TargetControl As Panel
    Shared PaintTimer As New Timer With {.Interval = 5}
    Public Shared callpercentstep = 0
    Shared x = 1
    Shared T As Single

    Public Shared Sub PaintTarget(target As Panel, targetpercentage As Single, speed As Integer)
        TargetControl = target
        T = targetpercentage
        callpercentstep = 0
        x = 1
        AddHandler PaintTimer.Tick, AddressOf t_tick
        PaintTimer.Interval = speed
        PaintTimer.Start()
    End Sub


    Shared Sub t_tick(sender As Object, e As EventArgs)

        If callpercentstep >= T Then
            PaintTimer.Stop()
            callpercentstep = T
            TargetControl.Invalidate()
            TargetControl.Update()
            RemoveHandler PaintTimer.Tick, AddressOf t_tick
        Else
            callpercentstep += 1.6 * x
            x -= T / (1.25 * T ^ 2)
            TargetControl.Invalidate()
            TargetControl.Update()
        End If
    End Sub



End Class