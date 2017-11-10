Imports System.IO
Imports System.Globalization
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions

Public Class WikiHandler
    Shared InputFromGitHub() As String

    Shared workingname As String = "testdir"


    'Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
    '    WikiParser()

    'End Sub

    Private Shared Sub WikiParser()
        Console.WriteLine(workingname)

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
                'ListBox1.Items.Add(s)
                gameName.Add(s.Split("|")(2))
            Catch ex As Exception

            End Try

        Next

        Dim strippedgameName As New List(Of String)

        For Each s In gameName
            Dim n = Regex.Replace(s, "[^\p{L}a-zA-Z0-90]", "")
            'ListBox2.Items.Add(n.ToLower)
            strippedgameName.Add(n.ToLower)
        Next




        Dim i = 0
        Dim gcount As New List(Of Integer)
        For Each a In strippedgameName
            If a.ToString.StartsWith(workingname) Then

                gcount.Add(i)
            End If
            i += 1
        Next



        WikiPopup.GamesTable.Visible = False

        WikiPopup.GamesTable.Controls.Clear()
        WikiPopup.GamesTable.RowCount = 0

        Dim provider As CultureInfo
        provider = New CultureInfo("en-US")

        Dim GName As New Label
        GName.Text = "Game"

        Dim GSizeU As New Label
        GSizeU.Text = "Before"

        Dim GSizeC As New Label
        GSizeC.Text = "After"

        Dim GCompR As New Label
        GCompR.Text = "Ratio"

        Dim GCompAlg As New Label
        GCompAlg.Text = "Algorithm"


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


    Shared firstGame As Integer = 0
    Private Shared Sub FillTable(ps As Integer)

        If firstGame = 0 Then
            Compact.sb_FolderName.Text = InputFromGitHub(ps).Split("|")(2)
            firstGame = 1
        End If


        Dim GName As New Label
        GName.Text = InputFromGitHub(ps).Split("|")(2)
        GName.Dock = DockStyle.Left
        GName.Font = New Font("Segoe UI", 11, FontStyle.Regular)

        Dim GSizeU As New Label
        GSizeU.Text = InputFromGitHub(ps).Split("|")(3)
        GSizeU.Dock = DockStyle.Right
        GSizeU.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        Dim GSizeC As New Label
        GSizeC.Text = InputFromGitHub(ps).Split("|")(4)
        GSizeC.Dock = DockStyle.Right
        GSizeC.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        Dim GCompR As New Label
        GCompR.Text = InputFromGitHub(ps).Split("|")(6)
        GCompR.Dock = DockStyle.Right
        GCompR.Font = New Font("Segoe UI", 10, FontStyle.Regular)
        Dim GCompAlg As New Label
        GCompAlg.Text = InputFromGitHub(ps).Split("|")(1)
        GCompAlg.Dock = DockStyle.Right
        GCompAlg.Font = New Font("Segoe UI", 10, FontStyle.Regular)

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



        Dim wnpatch As String

        wnpatch = Regex.Replace(DIwDString.Name.ToString, "[^\p{L}a-zA-Z0-90]", "").ToLower
        If wnpatch.Contains("callofduty") Then
            Dim interm = wnpatch.Replace("callofduty", "cod")
            If interm.Trim().EndsWith("modernwarfare") Then
                workingname = "cod4"
            Else
                workingname = interm
            End If

        ElseIf wnpatch.Contains("gameoftheyear") Then
            Dim interm = wnpatch.Replace("gameoftheyear", "goty")
            workingname = interm
        ElseIf wnpatch.Contains("shadowofmordor") Then
            workingname = "middleearthshadowofmordor"
        ElseIf wnpatch.Contains("age2hd") Then
            workingname = "ageofempiresiihd"
        ElseIf wnpatch.Contains("pubg") Then
            workingname = "playerunknownsbattlegrounds"
        Else
            workingname = wnpatch
        End If

        Dim folderSizeraw = GetOutputSize _
                    (WikiDirectorySize(DIwDString, True), True)
        folderSize = Math.Round(Decimal.Parse(folderSizeraw.Split(" ")(0)), 2)
        suffix = folderSizeraw.Split(" ")(1)
        Compact.preSize.Text = "Uncompressed Size: " & Math.Round(folderSize, 1) & " " & suffix

        Try

            Compact.wkPreSizeVal.Text = Math.Round(folderSize, 1)
            Compact.wkPreSizeUnit.Text = suffix
            Dim wkPreSizeVal_Len = TextRenderer.MeasureText(Compact.wkPreSizeVal.Text, Compact.wkPreSizeVal.Font)
            Compact.wkPreSizeUnit.Location = New Point(Compact.wkPreSizeVal.Location.X + (Compact.wkPreSizeVal.Size.Width / 2) + (wkPreSizeVal_Len.Width / 2 - 8), Compact.wkPreSizeVal.Location.Y + 16)
        Catch ex As System.DivideByZeroException

            Compact.wkPreSizeVal.Text = "?"
            Compact.wkPreSizeUnit.Text = ""
            Dim wkPreSizeVal_Len = TextRenderer.MeasureText(Compact.wkPreSizeVal.Text, Compact.wkPreSizeVal.Font)
            Compact.wkPreSizeUnit.Location = New Point(Compact.wkPreSizeVal.Location.X + (Compact.wkPreSizeVal.Size.Width / 2) + (wkPreSizeVal_Len.Width / 2 - 8), Compact.wkPreSizeVal.Location.Y + 16)
        End Try




        WikiParser()


    End Sub


    Public Shared Sub showWikiRes()

        Dim screenpos As Point = Compact.PointToScreen(New Point(Compact.seecompest.Location.X + 670, Compact.seecompest.Location.Y + 135))

        WikiPopup.StartPosition = FormStartPosition.Manual

        'If WikiPopup.GamesTable.Width < 450 Then
        '    If WikiPopup.GamesTable.RowCount > 1 Then
        '        WikiPopup.SetBounds(screenpos.X, screenpos.Y, WikiPopup.GamesTable.Width + 35, WikiPopup.GamesTable.Height + 200)

        '    Else
        'WikiPopup.SetBounds(screenpos.X, screenpos.Y, 450, 130)
        'End If

        'Else
        WikiPopup.SetBounds(screenpos.X, screenpos.Y, WikiPopup.GamesTable.Width + 35, WikiPopup.GamesTable.Height + 100)
        'End If

        FadeTransition.FadeForm(WikiPopup, 0, 0.96, 200)


    End Sub


    'End Sub

















    Public Shared Function GetOutputSize(ByVal inputsize As Decimal, Optional ByVal showSizeType As Boolean = False) As String            'Function for converting from Bytes into various units
        Dim sizeType As String = ""
        If inputsize < 1024 Then
            sizeType = " B"
        Else
            If inputsize < (1024 ^ 3) Then
                If inputsize < (1024 ^ 2) Then
                    sizeType = " KB"
                    inputsize = inputsize / 1024
                Else
                    sizeType = " MB"
                    inputsize = inputsize / 1024 ^ 2
                End If
            Else
                sizeType = " GB"
                inputsize = inputsize / 1024 ^ 3
            End If
        End If

        If showSizeType = True Then
            Return inputsize & sizeType
        Else
            Return inputsize
        End If

    End Function

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
