Public Class Info
    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("https://github.com/ImminentFate/CompactGUI/releases")
    End Sub

    Private Sub checkEnableRCMenu_CheckedChanged(sender As Object, e As EventArgs) Handles checkEnableRCMenu.CheckedChanged
        If checkEnableRCMenu.Checked = True Then
            RCMenu.WriteRCMenuRegistry()
        Else

        End If

    End Sub
End Class