Public Class Info
    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lbl_CheckUpdates.LinkClicked, LinkLabel2.LinkClicked
        If sender Is lbl_CheckUpdates Then Process.Start("https://github.com/ImminentFate/CompactGUI/releases")
        If sender Is LinkLabel2 Then Process.Start("https://github.com/ImminentFate/CompactGUI")
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