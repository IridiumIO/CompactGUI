Imports System.Runtime.InteropServices

Public Class Info

#Region "Move And Resize"

    <DllImport("user32.dll")>
    Public Shared Function ReleaseCapture() As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
    End Function

    Private Sub MoveForm()
        ReleaseCapture()
        SendMessage(Me.Handle, &HA1, 2, 0)
    End Sub

    Private Sub panel_header_MouseDown(sender As Object, e As MouseEventArgs) Handles panel_header.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Left Then MoveForm()
    End Sub
#End Region


    Private Sub Info_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        semVersion.Text = "V " + Compact.version
        If My.Settings.IsContextMenuEnabled = True Then checkEnableRCMenu.Checked = True

        If My.Settings.MinimisetoTray = True Then checkMinimisetoTray.Checked = True

        If My.Settings.ShowNotifications = True Then checkShowNotifications.Checked = True

        If My.Settings.ExperimentalBrowser = True Then checkExperimentalBrowser.Checked = True

    End Sub


    Private Sub GithubLinks(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lbl_CheckUpdates.LinkClicked, link_Github.LinkClicked
        If sender Is lbl_CheckUpdates Then Process.Start("https://github.com/ImminentFate/CompactGUI/releases")
        If sender Is link_Github Then Process.Start("https://github.com/ImminentFate/CompactGUI")
    End Sub


    Private Sub checkEnableRCMenu_CheckedChanged(sender As Object, e As EventArgs) Handles checkEnableRCMenu.Click
        If checkEnableRCMenu.Checked = True Then
            RCMenu.WriteRCMenuRegistry()
        ElseIf checkEnableRCMenu.Checked = False Then
            RCMenu.DeleteRCMenuRegistry()
        End If
    End Sub


    Private Sub checkExperimentalBrowser_CheckedChanged(sender As Object, e As EventArgs) Handles checkExperimentalBrowser.CheckedChanged
        If checkExperimentalBrowser.Checked = True Then
            My.Settings.ExperimentalBrowser = True
        ElseIf checkExperimentalBrowser.Checked = False Then
            My.Settings.ExperimentalBrowser = True
        End If
    End Sub


    Private Sub checkMinimisetoTray_CheckedChanged(sender As Object, e As EventArgs) Handles checkMinimisetoTray.CheckedChanged
        If checkMinimisetoTray.Checked Then
            My.Settings.MinimisetoTray = True
        Else
            My.Settings.MinimisetoTray = False
        End If
    End Sub


    Private Sub checkShowNotifications_CheckedChanged(sender As Object, e As EventArgs) Handles checkShowNotifications.CheckedChanged
        If checkShowNotifications.Checked Then
            My.Settings.ShowNotifications = True
        Else
            My.Settings.ShowNotifications = False
        End If
    End Sub


    Private Sub btn_options_Click(sender As Object, e As EventArgs) Handles btn_options.Click, btn_licenses.Click, btn_help.Click
        Dim btn = sender
        Select Case True
            Case sender Is btn_options
                InfoTabControl.SelectedTab = Tab_Features
            Case sender Is btn_licenses
                InfoTabControl.SelectedTab = Tab_Licenses
            Case sender Is btn_help
                InfoTabControl.SelectedTab = Tab_Help
        End Select

        For Each b As Button In Panel1.Controls
            If b IsNot sender Then
                b.BackColor = Color.FromArgb(255, 43, 60, 75)
            ElseIf b Is sender Then
                b.BackColor = Color.FromArgb(255, 102, 121, 138)
            End If
        Next

    End Sub

    Private Sub btn_Mainexit_Click(sender As Object, e As EventArgs) Handles btn_Mainexit.Click
        Me.Close()
    End Sub

End Class