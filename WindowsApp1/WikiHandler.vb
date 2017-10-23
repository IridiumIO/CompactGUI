Imports System.IO
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

            Dim wc As New WebClient
            wc.Encoding = Encoding.UTF8
            Source = wc.DownloadString("https://raw.githubusercontent.com/ImminentFate/CompactGUI/master/Wiki/WikiDB_Games")
            InputFromGitHub = Source.TrimEnd().Split(vbLf)
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



        Form2.GamesTable.Visible = False

        Form2.GamesTable.Controls.Clear()
        Form2.GamesTable.RowCount = 0



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


        Form2.GamesTable.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))
        Form2.GamesTable.RowCount += 1
        Form2.GamesTable.Controls.Add(GName, 0, Form2.GamesTable.RowCount - 1)
        Form2.GamesTable.Controls.Add(GSizeU, 1, Form2.GamesTable.RowCount - 1)
        Form2.GamesTable.Controls.Add(GSizeC, 2, Form2.GamesTable.RowCount - 1)
        Form2.GamesTable.Controls.Add(GCompR, 3, Form2.GamesTable.RowCount - 1)
        Form2.GamesTable.Controls.Add(GCompAlg, 4, Form2.GamesTable.RowCount - 1)


        For Each WikiHeader As Label In Form2.GamesTable.Controls
            WikiHeader.Font = New Font("Segoe UI", 11, FontStyle.Bold)

            WikiHeader.Dock = DockStyle.Right
        Next

        GName.Dock = DockStyle.Left

        Dim ratioavg As Decimal = 1
        For Each n In gcount

            FillTable(n)

            ratioavg += Decimal.Parse(InputFromGitHub(n).Split("|")(6))

        Next

        Try
            ratioavg = (ratioavg - 1) / gcount.Count

            'Compact.seecompest.Text = "Compression Estimate: " & Math.Round(folderSize * ratioavg, 2) & " " & suffix
            Form2.wkPostSizeVal.Text = Math.Round(folderSize * ratioavg, 1)
            Form2.wkPostSizeUnit.Text = suffix
            Form2.wkPostSizeUnit.Location = New Point(Form2.wkPostSizeVal.Location.X + Form2.wkPostSizeVal.Size.Width - 10, Form2.wkPostSizeVal.Location.Y + 10)
        Catch ex As System.DivideByZeroException
            'Compact.seecompest.Text = "Compression Estimate: Unknown"
            Form2.wkPostSizeVal.Text = "?"
            Form2.wkPostSizeUnit.Text = ""
            Form2.wkPostSizeUnit.Location = New Point(Form2.wkPostSizeVal.Location.X + Form2.wkPostSizeVal.Size.Width, Form2.wkPostSizeVal.Location.Y)
        End Try


        Form2.GamesTable.Visible = True



    End Sub

    Private Shared Sub FillTable(ps As Integer)




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

        Form2.GamesTable.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))
        Form2.GamesTable.RowCount += 1
        Form2.GamesTable.Controls.Add(GName, 0, Form2.GamesTable.RowCount - 1)
        Form2.GamesTable.Controls.Add(GSizeU, 1, Form2.GamesTable.RowCount - 1)
        Form2.GamesTable.Controls.Add(GSizeC, 2, Form2.GamesTable.RowCount - 1)
        Form2.GamesTable.Controls.Add(GCompR, 3, Form2.GamesTable.RowCount - 1)
        Form2.GamesTable.Controls.Add(GCompAlg, 4, Form2.GamesTable.RowCount - 1)

        For Each ConC As Label In Form2.GamesTable.Controls
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

        Else
            workingname = wnpatch
        End If

        Dim folderSizeraw = GetOutputSize _
                    (WikiDirectorySize(DIwDString, True), True)
        folderSize = Math.Round(Decimal.Parse(folderSizeraw.Split(" ")(0)), 2)
        suffix = folderSizeraw.Split(" ")(1)
        Compact.preSize.Text = "Uncompressed Size: " & Math.Round(folderSize, 1) & " " & suffix

        Try

            Form2.wkPreSizeVal.Text = Math.Round(folderSize, 1)
            Form2.wkPreSizeUnit.Text = suffix
            Form2.wkPreSizeUnit.Location = New Point(Form2.wkPreSizeVal.Location.X + Form2.wkPreSizeVal.Size.Width - 10, Form2.wkPreSizeVal.Location.Y + 10)
        Catch ex As System.DivideByZeroException

            Form2.wkPreSizeVal.Text = "?"
            Form2.wkPreSizeUnit.Text = ""
            Form2.wkPreSizeUnit.Location = New Point(Form2.wkPreSizeVal.Location.X + Form2.wkPreSizeVal.Size.Width, Form2.wkPreSizeVal.Location.Y)
        End Try




        WikiParser()


    End Sub


    Public Shared Sub showWikiRes()

        Dim screenpos As Point = Compact.PointToScreen(New Point(Compact.seecompest.Location.X - 1, Compact.seecompest.Location.Y + 12))

        Form2.StartPosition = FormStartPosition.Manual

        If Form2.GamesTable.Width < 450 Then
            If Form2.GamesTable.RowCount > 1 Then
                Form2.SetBounds(screenpos.X, screenpos.Y, Form2.GamesTable.Width + 35, Form2.GamesTable.Height + 200)
            Else
                Form2.SetBounds(screenpos.X, screenpos.Y, 450, 130)
            End If

        Else
            Form2.SetBounds(screenpos.X, screenpos.Y, Form2.GamesTable.Width + 35, Form2.GamesTable.Height + 200)
        End If

        Form2.Show()

        Compact.FadeWikiInfo.Start()

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
