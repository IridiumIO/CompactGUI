Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions

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

        If My.Settings.SkipNonCompressable = True Then checkEnableNonCompressable.Checked = True

        populateNonCompressable()



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
            My.Settings.ExperimentalBrowser = False
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

    Private Sub checkEnableNonCompressable_CheckedChanged(sender As Object, e As EventArgs) Handles checkEnableNonCompressable.CheckedChanged
        If checkEnableNonCompressable.Checked Then
            My.Settings.SkipNonCompressable = True
        Else
            My.Settings.SkipNonCompressable = False
        End If
    End Sub


    Private Sub btn_options_Click(sender As Object, e As EventArgs) Handles btn_options.Click, btn_licenses.Click, btn_help.Click

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

    Private Sub btnSaveNonCompressable_Click(sender As Object, e As EventArgs) Handles btnSaveNonCompressable.Click
        My.Settings.NonCompressableList = Regex.Replace(TxtBoxNonCompressable.Text, "\s+", ";").Replace(".", "").ToLowerInvariant
        populateNonCompressable()
    End Sub

    Private Sub btnDefaultNonCompressable_Click(sender As Object, e As EventArgs) Handles btnDefaultNonCompressable.Click
        My.Settings.NonCompressableList = "dl_; gif; jpg; jpeg; bmp; png; wmf; mkv; mp4; wmv; avi; bik; flv; ogg; mpg; m2v; m4v; vob; mp3; aac; wma; flac; zip; xap; rar; 7z; cab; lzx; docx; xlsx; pptx; vssx; vstx; onepkg"
        populateNonCompressable()
    End Sub

    Private Sub populateNonCompressable()
        Dim NonCompressableFileTypes As String() = My.Settings.NonCompressableList.Replace(" ", "").Split(";"c)
        TxtBoxNonCompressable.Text = ""
        For Each i In NonCompressableFileTypes
            TxtBoxNonCompressable.Text &= i & vbTab
        Next
        TxtBoxNonCompressable.Text = TxtBoxNonCompressable.Text.Trim()
    End Sub

    Private Sub btn_Paint(sender As Object, e As PaintEventArgs) Handles btn_options.Paint, btn_help.Paint, btn_licenses.Paint

        If sender.backcolor = Color.FromArgb(255, 102, 121, 138) Then
            Dim trianglePtsArray As PointF() = {New PointF(sender.width - 1, 10), New PointF(sender.width - 1, sender.height - 10),
                New PointF(sender.width - 10, sender.height / 2), New PointF(sender.width - 1, 10)}
            Dim gp As New Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
            e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            gp.AddLines(trianglePtsArray)
            gp.CloseFigure()

            e.Graphics.FillPath(Brushes.White, gp)

            e.Graphics.DrawLines(Pens.White, trianglePtsArray)
        End If
    End Sub

    Private Sub Tab_Features_Paint(sender As Object, e As PaintEventArgs) Handles Tab_Features.Paint
        Dim p As New Pen(Color.LightGray)

        e.Graphics.DrawLine(p, New Point(46, 220), New Point(sender.width - 46, 220))
    End Sub
End Class