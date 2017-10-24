Imports System.Windows.Forms

Public Class ShutdownDialog



    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Abort
        shutdownTimer.Stop
        FadeTransition.FadeForm(Me, 0.96, 0, 300, True)
        'Me.Close()
    End Sub


    Private Sub Dialog1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        shutdownTimer.Start()
        Select Case SDProcIntent.Text
            Case "Shutdown"
                SDProgText.Text = "Shutting down in 10 seconds"
            Case "Restart"
                SDProgText.Text = "Restarting in 10 seconds"
            Case "Sleep"
                SDProgText.Text = "Sleeping in 10 seconds"
        End Select
    End Sub

    Dim secCount As Integer = 9
    Private Sub shutdownTimer_Tick(sender As Object, e As EventArgs) Handles shutdownTimer.Tick

        Select Case SDProcIntent.Text
            Case "Shutdown"
                SDProgText.Text = "Shutting down in " & secCount & " seconds"
            Case "Restart"
                SDProgText.Text = "Restarting in " & secCount & " seconds"
            Case "Sleep"
                SDProgText.Text = "Sleeping in " & secCount & " seconds"
        End Select
        SDProgText.Visible = True

        If secCount = 0 Then
            secCount = 10
            shutdownTimer.Stop()

            Select Case SDProcIntent.Text
                Case "Shutdown"
                    Process.Start("shutdown", "/s /t 0")
                    Me.Close()
                Case "Restart"
                    Process.Start("shutdown", "/r /t 0")
                    Me.Close()
                Case "Sleep"
                    Application.SetSuspendState(PowerState.Suspend, False, False)
                    Me.Close()
            End Select

            Me.DialogResult = System.Windows.Forms.DialogResult.OK
        End If

        secCount -= 1
    End Sub
End Class
