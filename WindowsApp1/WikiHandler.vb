Imports System.IO
Imports System.Globalization
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions

Public Class WikiHandler
    Shared InputFromGitHub() As String

    Shared workingname As String = "testdir"

    Private Shared Sub WikiParser()
        Console.WriteLine("Working Name: " & workingname)

        Dim stringSeparators() As String = {vbCrLf}
        Dim Source As String
        Dim gameName As New List(Of String)

        If InputFromGitHub Is Nothing Then
            Console.WriteLine("Getting List")
            Dim wc As New WebClient
            wc.Encoding = Encoding.UTF8
            Try
                Source = wc.DownloadString("https://raw.githubusercontent.com/ImminentFate/CompactGUI/master/Wiki/WikiDB_Games")
                InputFromGitHub = Source.TrimEnd().Split(vbLf)
            Catch ex As WebException
                Compact.sb_lblGameIssues.Text = "! No Internet Connection"
                Compact.sb_lblGameIssues.Visible = True
                Compact.wkPostSizeVal.Text = "?"
                Compact.wkPostSizeUnit.Text = ""
                Compact.wkPostSizeUnit.Location = New Point(Compact.wkPostSizeVal.Location.X + Compact.wkPostSizeVal.Size.Width, Compact.wkPostSizeVal.Location.Y)
                Compact.sb_Panel.Show()
            End Try

        End If


        For Each s As String In InputFromGitHub
            Try
                gameName.Add(s.Split("|")(2))
            Catch ex As Exception
            End Try
        Next


        Dim strippedgameName As New List(Of String)


        For Each s In gameName
            Dim n = Regex.Replace(s, "[^\p{L}a-zA-Z0-90]", "")
            strippedgameName.Add(n.ToLower.Trim)
        Next



        Dim gcount As New List(Of Integer)

        If ParseLogic(strippedgameName, workingname) = True Then
            Dim i = 0
            If isExactMatch = True Then
                For Each a In strippedgameName
                    If a.Equals(workingname) And workingname.Length > 1 Then gcount.Add(i)
                    i += 1
                Next
            Else
                For Each a In strippedgameName
                    If a.ToString.StartsWith(workingname) And Math.Abs(a.ToString.Length - workingname.Length) < 5 And workingname.Length > 1 Then gcount.Add(i)
                    i += 1
                Next
            End If

        Else
            Dim i = 0
            For Each a In strippedgameName
                If workingname.Length > 5 Then
                    If a.ToString.Contains(workingname) Then gcount.Add(i)
                End If
                i += 1
            Next
        End If


        WikiPopup.GamesTable.Visible = False
        WikiPopup.GamesTable.Controls.Clear()
        WikiPopup.GamesTable.RowCount = 0

        Dim provider As CultureInfo = New CultureInfo("en-US")

        Dim GName As New Label With {.Text = "Game"}
        Dim GSizeU As New Label With {.Text = "Before"}
        Dim GSizeC As New Label With {.Text = "After"}
        Dim GCompR As New Label With {.Text = "Ratio"}
        Dim GCompAlg As New Label With {.Text = "Algorithm"}


        WikiPopup.GamesTable.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))
        WikiPopup.GamesTable.RowCount += 1
        WikiPopup.GamesTable.Controls.Add(GName, 0, WikiPopup.GamesTable.RowCount - 1)
        WikiPopup.GamesTable.Controls.Add(GSizeU, 1, WikiPopup.GamesTable.RowCount - 1)
        WikiPopup.GamesTable.Controls.Add(GSizeC, 2, WikiPopup.GamesTable.RowCount - 1)
        WikiPopup.GamesTable.Controls.Add(GCompR, 3, WikiPopup.GamesTable.RowCount - 1)
        WikiPopup.GamesTable.Controls.Add(GCompAlg, 4, WikiPopup.GamesTable.RowCount - 1)

        For Each WikiHeader As Label In WikiPopup.GamesTable.Controls
            WikiHeader.Font = New Font("Segoe UI", 11, FontStyle.Bold)
            WikiHeader.Dock = DockStyle.Right
        Next

        GName.Dock = DockStyle.Left


        Dim ratioavg As Decimal = 1
        firstGame = 0

        For Each n In gcount
            FillTable(n)

            ratioavg += Decimal.Parse(InputFromGitHub(n).Split("|")(6), provider)

            If InputFromGitHub(n).Split("|")(7).Contains("*") Then
                Compact.sb_lblGameIssues.Visible = True
                Compact.sb_lblGameIssues.Text = "! Game has issues"
            Else
                Compact.sb_lblGameIssues.Visible = False
            End If

        Next
        Compact.sb_labelCompressed.Text = "Estimated Compressed"

        Try
            ratioavg = (ratioavg - 1) / gcount.Count

            Compact.wkPostSizeVal.Text = Math.Round(folderSize * ratioavg, 1)
            Compact.wkPostSizeUnit.Text = suffix
            Dim wkPostSizeVal_Len = TextRenderer.MeasureText(Compact.wkPostSizeVal.Text, Compact.wkPostSizeVal.Font)
            Compact.wkPostSizeUnit.Location = New Point(Compact.wkPostSizeVal.Location.X + (Compact.wkPostSizeVal.Size.Width / 2) + (wkPostSizeVal_Len.Width / 2 - 8), Compact.wkPostSizeVal.Location.Y + 16)
            Compact.wkPostSizeUnit.Visible = True

        Catch ex As System.DivideByZeroException
            Compact.wkPostSizeVal.Text = "?"
            Compact.wkPostSizeUnit.Text = ""
            Dim wkPostSizeVal_Len = TextRenderer.MeasureText(Compact.wkPostSizeVal.Text, Compact.wkPostSizeVal.Font)
            Compact.wkPostSizeUnit.Location = New Point(Compact.wkPostSizeVal.Location.X + (Compact.wkPostSizeVal.Size.Width / 2) + (wkPostSizeVal_Len.Width / 2 - 8), Compact.wkPostSizeVal.Location.Y + 16)
        End Try


        If WikiPopup.GamesTable.RowCount > 1 Then
            Compact.seecompest.Visible = True
        Else
            Compact.seecompest.Visible = False
        End If

        WikiPopup.GamesTable.Visible = True

        Compact.sb_Panel.Show()

    End Sub




    Shared isExactMatch As Boolean
    Shared Function ParseLogic(online_R As List(Of String), local_R As String) As Boolean
        isExactMatch = False
        Dim success = 0
        For Each a In online_R
            If a.StartsWith(local_R) And Math.Abs(a.Length - workingname.Length) < 5 Then
                If a.Length = workingname.Length Then isExactMatch = True
                success = 1
                Return True
                Exit For
            End If
        Next

        If success = 0 Then Return False

    End Function



    Shared firstGame As Integer = 0
    Private Shared Sub FillTable(ps As Integer)

        If firstGame = 0 Then
            Compact.sb_FolderName.Text = InputFromGitHub(ps).Split("|")(2)
            firstGame = 1
        End If


        Dim GName As New Label With {
        .Text = InputFromGitHub(ps).Split("|")(2),
        .Dock = DockStyle.Left,
        .Font = New Font("Segoe UI", 11, FontStyle.Regular)
        }
        Dim GSizeU As New Label With {
        .Text = InputFromGitHub(ps).Split("|")(3),
        .Dock = DockStyle.Right,
        .Font = New Font("Segoe UI", 10, FontStyle.Regular)
        }
        Dim GSizeC As New Label With {
        .Text = InputFromGitHub(ps).Split("|")(4),
        .Dock = DockStyle.Right,
        .Font = New Font("Segoe UI", 10, FontStyle.Regular)
        }
        Dim GCompR As New Label With {
        .Text = InputFromGitHub(ps).Split("|")(6),
        .Dock = DockStyle.Right,
        .Font = New Font("Segoe UI", 10, FontStyle.Regular)
        }
        Dim GCompAlg As New Label With {
        .Text = InputFromGitHub(ps).Split("|")(1),
        .Dock = DockStyle.Right,
        .Font = New Font("Segoe UI", 10, FontStyle.Regular)
        }

        WikiPopup.GamesTable.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))
        WikiPopup.GamesTable.RowCount += 1
        WikiPopup.GamesTable.Controls.Add(GName, 0, WikiPopup.GamesTable.RowCount - 1)
        WikiPopup.GamesTable.Controls.Add(GSizeU, 1, WikiPopup.GamesTable.RowCount - 1)
        WikiPopup.GamesTable.Controls.Add(GSizeC, 2, WikiPopup.GamesTable.RowCount - 1)
        WikiPopup.GamesTable.Controls.Add(GCompR, 3, WikiPopup.GamesTable.RowCount - 1)
        WikiPopup.GamesTable.Controls.Add(GCompAlg, 4, WikiPopup.GamesTable.RowCount - 1)

        For Each ConC As Label In WikiPopup.GamesTable.Controls
            ConC.AutoSize = True
            ConC.Padding = New Padding(2, 4, 0, 0)
        Next

    End Sub




    Shared folderSize
    Shared suffix



    Public Shared Sub localFolderParse(wdString As String, DIwDString As DirectoryInfo, rawPreSize As String)

        Dim wnpatch As String = Regex.Replace(DIwDString.Name.ToString, "[^\p{L}a-zA-Z0-90]", "").ToLower.Trim()

        Select Case True
            Case wnpatch.Contains("callofduty")
                workingname = wnpatch.Replace("callofduty", "cod")
                If workingname.EndsWith("Modernwarfare") Then workingname = "cod4"

            Case wnpatch.Contains("gameoftheyear")
                workingname = wnpatch.Replace("gameoftheyear", "goty")

            Case wnpatch.Contains("age2hd")
                workingname = "ageofempiresiihd"

            Case wnpatch.Contains("shadowofmordor")
                workingname = "middleearthshadowofmordor"

            Case wnpatch.Contains("shadowofwar")
                workingname = "middleearthshadowofwar"

            Case wnpatch.Contains("pubg")
                workingname = "playerunknownsbattlegrounds"

            Case Else
                workingname = wnpatch
        End Select

        folderSize = Math.Round(Decimal.Parse(rawPreSize.Split(" ")(0)), 2)
        suffix = rawPreSize.Split(" ")(1)

        Compact.preSize.Text = "Uncompressed Size: " & Math.Round(folderSize, 1) & " " & suffix

        Try

            Compact.wkPreSizeVal.Text = Math.Round(folderSize, 1)
            Compact.wkPreSizeUnit.Text = suffix
            Dim wkPreSizeVal_Len = TextRenderer.MeasureText(Compact.wkPreSizeVal.Text, Compact.wkPreSizeVal.Font)
            Compact.wkPreSizeUnit.Location = New Point(Compact.wkPreSizeVal.Location.X + (Compact.wkPreSizeVal.Size.Width / 2) + (wkPreSizeVal_Len.Width / 2 - 8), Compact.wkPreSizeVal.Location.Y + 16)

            ' I have no idea why this catch is needed
        Catch ex As System.DivideByZeroException

            Compact.wkPreSizeVal.Text = "?"
            Compact.wkPreSizeUnit.Text = ""
            Dim wkPreSizeVal_Len = TextRenderer.MeasureText(Compact.wkPreSizeVal.Text, Compact.wkPreSizeVal.Font)
            Compact.wkPreSizeUnit.Location = New Point(Compact.wkPreSizeVal.Location.X + (Compact.wkPreSizeVal.Size.Width / 2) + (wkPreSizeVal_Len.Width / 2 - 8), Compact.wkPreSizeVal.Location.Y + 16)
        End Try

        WikiParser()

    End Sub




    Public Shared Sub showWikiRes()

        Dim w = WikiPopup.GamesTable.Width + 35
        Dim h = WikiPopup.GamesTable.Height + 100
        Dim sc_w = Screen.PrimaryScreen.Bounds.Width

        Dim screenpos As Point = Compact.PointToScreen _
            (New Point(Compact.seecompest.Location.X + 670, Compact.seecompest.Location.Y + 135))

        'Checks to make sure the popup isn't going to go offscreen. 
        If screenpos.X + w < sc_w Then
            WikiPopup.SetBounds(screenpos.X, screenpos.Y, w, h)
        Else
            WikiPopup.SetBounds(screenpos.X - (w - (sc_w - screenpos.X)), screenpos.Y + 15, w, h)
        End If

        FadeTransition.FadeForm(WikiPopup, 0, 0.96, 200)

    End Sub




    Private Shared Function WikiDirectorySize _
        (ByVal dInfo As IO.DirectoryInfo, ByVal includeSubdirectories As Boolean) As Long

        Try
            Dim totalSize As Long = dInfo.EnumerateFiles().Sum(Function(file) file.Length)
            If includeSubdirectories Then
                totalSize += dInfo.EnumerateDirectories().Sum(Function(dir) WikiDirectorySize(dir, True))
            End If
            Return totalSize

        Catch generatedexceptionname As UnauthorizedAccessException


        Catch ex As Exception

        End Try

    End Function


End Class
