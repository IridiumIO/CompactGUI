Imports System.IO
Imports System.Management
Imports System.Runtime.InteropServices
Imports System.Security.Policy
Imports System.Text.RegularExpressions
Imports System.Web

Public Class WikiSubmission

    Friend Shared Name_Submit As String
    Friend Shared SteamID_Submit As String = 0
    Friend Shared Type_Submit As String
    Private UniqueID_Submit As String
    Friend Shared Folder_Submit As String
    Friend Shared CompMode_Submit As String
    Friend Shared BeforeSize_Submit As ULong
    Friend Shared AfterSize_Submit As ULong




    Private Sub WikiSubmission_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint
        Dim rect As New Rectangle(0, 0, Panel1.Width - 1, Panel1.Height + 1)
        e.Graphics.DrawRectangle(New Pen(Color.White), rect)
    End Sub

    Private Sub ParseforSteamData()

        Dim steamID As Integer = 0
        Dim steamName As String = ""

        Dim targetACFFile As FileInfo = ParseACFFiles()

        If targetACFFile IsNot Nothing Then
            Dim ACFText As String() = File.ReadAllText(targetACFFile.FullName).Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
            For Each l In ACFText
                Dim lf = l.TrimStart()
                If lf.StartsWith("""" & "appid" & """") Then
                    steamID = lf.Substring(lf.LastIndexOf(vbTab) + 1).Replace("""", "")
                End If
                If lf.StartsWith("""" & "name" & """") Then
                    steamName = lf.Substring(lf.LastIndexOf(vbTab) + 1).Replace("""", "")
                    GoTo Assignment
                End If
            Next
        End If

Assignment:

        Dim rx As New Regex("[\?&|%™®©]")

        txtbox_Name.Text = rx.Replace(steamName, "")
        txtbox_SteamID.Value = steamID

    End Sub


    Private Function ParseACFFiles() As FileInfo
        Dim dI As DirectoryInfo = New DirectoryInfo(Compact.workingDir)
        If Directory.GetParent(dI.Parent.FullName) IsNot Nothing AndAlso Directory.GetParent(dI.Parent.FullName).Name = "steamapps" Then
            For Each f As FileInfo In Directory.GetParent(dI.Parent.FullName).GetFiles
                If f.Extension = ".acf" Then
                    Dim ACFText As String() = File.ReadAllText(f.FullName).Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)

                    For Each l In ACFText
                        Dim lf = l.TrimStart()
                        If lf.StartsWith("""" & "installdir" & """") AndAlso Folder_Submit = lf.Substring(lf.LastIndexOf(vbTab) + 1).Replace("""", "") Then
                            Return f
                        End If
                    Next

                End If
            Next
        End If

    End Function


    Private Sub btn_NextPage_Click(sender As Object, e As EventArgs) Handles btn_NextPage.Click
        If TabControl1.SelectedTab Is Page1 Then
            If Radio_Game.Checked Then
                ParseforSteamData()
                Type_Submit = "Game"

                panel_SteamID.Visible = True
                lbl_GameorProgram.Text = "Game Name:"

                TabControl1.SelectedTab = Page2

            ElseIf Radio_Program.Checked Then

                Type_Submit = "Program"

                panel_SteamID.Visible = False
                lbl_GameorProgram.Text = "Program Name:"

                TabControl1.SelectedTab = Page2

            End If

        ElseIf TabControl1.SelectedTab Is Page2 Then

            Dim rx As New Regex("[\?&|%™®©]")


            If rx.Match(txtbox_Name.Text).Success Then
                MsgBox("Name cannot contain '?', '&', '|', '%', '™', '®', or '©'")

            Else

                PrepareSubmission()


            End If

        ElseIf TabControl1.SelectedTab Is Page3 Then
            Me.Close()
        End If


    End Sub


    Private Sub PrepareSubmission()
        Dim alreadyExists As Boolean
        For Each res As Result In WikiHandler.allResults
            If CInt(txtbox_SteamID.Value) = If(res.SteamID = 0, 999999, res.SteamID) OrElse Folder_Submit = res.Folder OrElse txtbox_Name.Text.Trim() = res.Name Then
                If CompMode_Submit = res.Algorithm Then
                    If BeforeSize_Submit >= res.BeforeSize * 0.92 AndAlso BeforeSize_Submit <= res.BeforeSize * 1.08 Then
                        alreadyExists = True
                    End If
                End If
            End If
        Next


        Name_Submit = HttpUtility.UrlPathEncode(txtbox_Name.Text.Trim())
        Folder_Submit = HttpUtility.UrlPathEncode(Folder_Submit)
        SteamID_Submit = txtbox_SteamID.Text
        UniqueID_Submit = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(getMacAddress()))

        FillDataCollection()


        If alreadyExists = False Then
            Dim URL_First As String = "https://docs.google.com/forms/d/e/1FAIpQLSfAzlQAhyPEueFyQiTEmpudcKaVLnpRPmzrIuBZxnR8f7PjPg/formResponse?&ifq&entry.630201004=%3CCompactGUI%3E"

            Dim URL_Last As String = "&submit=Submit"

            Dim URL_All As String = URL_First & UniqueID_Submit & "%7C" & Type_Submit & "%7C" & Name_Submit & "%7C" & Folder_Submit & "%7C" & SteamID_Submit & "%7C" & CompMode_Submit & "%7C" & BeforeSize_Submit & "%7C" & AfterSize_Submit & URL_Last
            lbl_Title.Text = "Sending Results"
            Panel1.Refresh()

            SendPageRequest(URL_All)
        Else

        End If

        lbl_Title.Text = "Results Sent"
        TabControl1.SelectedTab = Page3
        btn_NextPage.Text = "Close"
        btn_Cancel.Visible = False
    End Sub

    Private Sub SendPageRequest(ByVal URL As String, Optional ByVal proxy As Net.WebProxy = Nothing)
        Try
            Dim webReq As Net.HttpWebRequest = CType(Net.WebRequest.Create(URL), Net.HttpWebRequest)
            If proxy IsNot Nothing Then webReq.Proxy = proxy
            Using webResp As Net.HttpWebResponse = CType(webReq.GetResponse(), Net.HttpWebResponse)
                'Get the response, then close it as we don't actually need anything but to send the request.
            End Using
        Catch ex As Net.WebException
            MsgBox("An internet connection could not be established. Please try again later.")
            Me.Close()
        End Try

    End Sub






    Private Sub FillDataCollection()

        Dim output As String =
"UID: " & UniqueID_Submit & "
Name: " & HttpUtility.UrlDecode(Name_Submit) & "
Type: " & Type_Submit & "
Folder: " & HttpUtility.UrlDecode(Folder_Submit) & "
Compression Mode: " & CompMode_Submit & "
Size Before: " & BeforeSize_Submit & "
Size After: " & AfterSize_Submit

        Dim ite As String() = output.Split(vbCrLf)

        For Each i In ite
            Dim splitp() = i.Split(New Char() {":"c}, 2)
            ListView1.Items.Add(New ListViewItem(splitp))
        Next


    End Sub




    Public Function getMacAddress() As String
        Dim MacID As String = String.Empty
        Dim mc As ManagementClass = New ManagementClass("Win32_NetworkAdapterConfiguration")
        Dim moc As ManagementObjectCollection = mc.GetInstances()
        For Each mo As ManagementObject In moc
            If (MacID = String.Empty And CBool(mo.Properties("IPEnabled").Value) = True) Then
                MacID = mo.Properties("MacAddress").Value.ToString()
            End If
        Next
        Return MacID
    End Function

    Private Sub btn_Cancel_Click(sender As Object, e As EventArgs) Handles btn_Cancel.Click
        Me.Close()
    End Sub

#Region "Fancy Stuff"
    Protected Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Const CS_DROPSHADOW = &H20000
            Dim cp As CreateParams = MyBase.CreateParams
            cp.Style = cp.Style Or &H20000                                                  '&H20000 = WS_MINIMIZEBOX
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW Or &H8                           '&H8 = CS_DBLCLKS
            Return cp
        End Get
    End Property


    <DllImport("user32.dll")>
    Shared Function ReleaseCapture() As Boolean
    End Function


    <DllImport("user32.dll")>
    Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
    End Function


    Private Sub MoveForm()
        ReleaseCapture()
        SendMessage(Me.Handle, &HA1, 2, 0)
    End Sub


    Private Sub panel_topBar_MouseDown(sender As Object, e As MouseEventArgs) Handles Panel1.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Left Then MoveForm()
    End Sub

#End Region




End Class