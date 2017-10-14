Public Class Info
    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("https://github.com/ImminentFate/CompactGUI/releases")
    End Sub

    Private Sub checkEnableRCMenu_CheckedChanged(sender As Object, e As EventArgs) Handles checkEnableRCMenu.Click
        If checkEnableRCMenu.Checked = True Then
            RCMenu.WriteRCMenuRegistry()


        ElseIf checkEnableRCMenu.Checked = False Then
            RCMenu.DeleteRCMenuRegistry()

        End If

    End Sub

    Private Sub Info_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If My.Settings.IsContextMenuEnabled = True Then
            checkEnableRCMenu.Checked = True
        Else
            checkEnableRCMenu.Checked = False
        End If

    End Sub
End Class