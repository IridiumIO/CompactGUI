Imports System.IO
Imports System.Globalization
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions

Class WikiHandler

    Shared InputFromGitHub As IEnumerable(Of XElement)
    Friend Shared allResults As New List(Of Result)
    Shared workingname As String = "testdir"

    Private Shared Sub WikiParser()
        Console.WriteLine("Working Name: " & workingname)

        allResults.Clear()
        Dim SRC As XElement

        If InputFromGitHub Is Nothing Then
            Console.WriteLine("Getting List")
            Dim wc = New WebClient With {.Encoding = Encoding.UTF8}
            Try
                SRC = XElement.Parse(wc.DownloadString("https://raw.githubusercontent.com/ImminentFate/CompactGUI/master/Wiki/Database.xml"))
                InputFromGitHub = SRC.Elements()
                ParseData()

            Catch ex As WebException
                Compact.sb_lblGameIssues.Text = "! No Internet Connection"
                Compact.sb_lblGameIssues.Visible = True
                Compact.ToolTipFilesCompressed.SetToolTip(Compact.sb_lblGameIssues, "")
                Compact.wkPostSizeVal.Text = "?"
                Compact.wkPostSizeUnit.Text = ""
                Compact.wkPostSizeUnit.Location = New Point(Compact.wkPostSizeVal.Location.X + Compact.wkPostSizeVal.Size.Width, Compact.wkPostSizeVal.Location.Y)
                Compact.sb_Panel.Show()
            End Try

        Else
            ParseData()
        End If

    End Sub

    Private Shared Sub ParseData()

        For Each result In InputFromGitHub
            Dim itemName As String = result.Element("itemName").Value
            Dim itemFolder As String = result.Element("itemFolder").Value
            Dim itemSteamID As String = result.Element("itemSteamID").Value
            Dim itemAlgorithm As String = result.Element("itemAlgorithm").Value
            Dim itemBeforeSize As UInt64 = result.Element("itemBefore").Value
            Dim itemAfterSize As UInt64 = result.Element("itemAfter").Value

            Dim res As New Result(itemName, itemFolder, itemSteamID, itemAlgorithm, itemBeforeSize, itemAfterSize)

            allResults.Add(res)
        Next result


        Dim gcount As New List(Of Result)


        Dim matches As Integer = 0
        For Each r As Result In allResults
            If r.Folder.Equals(workingname) Then
                gcount.Add(r)
                matches += 1
            End If

        Next
        Console.WriteLine(vbCrLf)
        If matches = 0 Then
            For Each r As Result In allResults
                If r.Name_Sanitised.Contains(workingname) Then
                    gcount.Add(r)
                    matches += 1
                End If

            Next
        End If


        Dim ratioavg As Decimal = 1
        firstGame = 0


        PrepareTable()

        For Each r In gcount
            FillTable(r)

            ratioavg += Decimal.Parse(r.Ratio)

            Compact.sb_lblGameIssues.Visible = False   'Add check for game issues at later date


        Next
        Compact.sb_labelCompressed.Text = "Estimated Compressed"

        If gcount.Count <> Nothing Then
            ratioavg = (ratioavg - 1) / gcount.Count

            Compact.wkPostSizeVal.Text = Math.Round(folderSize * ratioavg, 1)
            Compact.wkPostSizeUnit.Text = suffix
            Dim wkPostSizeVal_Len = TextRenderer.MeasureText(Compact.wkPostSizeVal.Text, Compact.wkPostSizeVal.Font)
            Compact.wkPostSizeUnit.Location = New Point(Compact.wkPostSizeVal.Location.X + (Compact.wkPostSizeVal.Size.Width / 2) + (wkPostSizeVal_Len.Width / 2 - 8), Compact.wkPostSizeVal.Location.Y + 16)
            Compact.wkPostSizeUnit.Visible = True
        Else
            Compact.wkPostSizeVal.Text = "?"
            Compact.wkPostSizeUnit.Text = ""
            Dim wkPostSizeVal_Len = TextRenderer.MeasureText(Compact.wkPostSizeVal.Text, Compact.wkPostSizeVal.Font)
            Compact.wkPostSizeUnit.Location = New Point(Compact.wkPostSizeVal.Location.X + (Compact.wkPostSizeVal.Size.Width / 2) + (wkPostSizeVal_Len.Width / 2 - 8), Compact.wkPostSizeVal.Location.Y + 16)

        End If

        If WikiPopup.GamesTable.RowCount > 1 Then
            Compact.seecompest.Visible = True
        Else
            Compact.seecompest.Visible = False
        End If

        WikiPopup.GamesTable.Visible = True

        Compact.sb_Panel.Show()

    End Sub


    Private Shared Sub PrepareTable()
        WikiPopup.GamesTable.Visible = False
        WikiPopup.GamesTable.Controls.Clear()
        WikiPopup.GamesTable.RowCount = 0

        Dim provider As CultureInfo = New CultureInfo("en-US")

        Dim GName As New Label With {.Text = "Game"}
        Dim GSizeU As New Label With {.Text = "Before"}
        Dim GSizeC As New Label With {.Text = "After"}
        Dim GCompR As New Label With {.Text = "Ratio"}
        Dim GCompAlg As New Label With {.Text = "Algorithm"}
        Console.WriteLine(WikiPopup.GamesTable.RowCount)

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

    End Sub


    Shared firstGame As Integer = 0
    Private Shared Sub FillTable(ps As Result)

        If firstGame = 0 Then
            Compact.sb_FolderName.Text = ps.Name
            firstGame = 1
        End If


        Dim GName As New Label With {
        .Text = ps.Name, .ForeColor = Color.DimGray,
        .Dock = DockStyle.Left,
        .Font = New Font("Segoe UI", 11, FontStyle.Regular)
        }
        Dim GSizeU As New Label With {
        .Text = ps.BeforeSize_Formatted, .ForeColor = Color.DimGray,
        .Dock = DockStyle.Right,
        .Font = New Font("Segoe UI", 10, FontStyle.Regular)
        }
        Dim GSizeC As New Label With {
        .Text = ps.AfterSize_Formatted, .ForeColor = Color.DimGray,
        .Dock = DockStyle.Right,
        .Font = New Font("Segoe UI", 10, FontStyle.Regular)
        }
        Dim GCompR As New Label With {
        .Text = ps.Ratio, .ForeColor = Color.DimGray,
        .Dock = DockStyle.Right,
        .Font = New Font("Segoe UI", 10, FontStyle.Regular)
        }
        Dim GCompAlg As New Label With {
        .Text = ps.Algorithm, .ForeColor = Color.DimGray,
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



    Public Shared Sub localFolderParse(DIwDString As DirectoryInfo, rawPreSize As String)

        Dim wnpatch As String = Regex.Replace(DIwDString.Name.ToString, "[^\p{L}a-zA-Z0-90]", "").ToLower.Trim()

        workingname = wnpatch

        folderSize = Math.Round(Decimal.Parse(rawPreSize.Split(" ")(0)), 2)
        suffix = rawPreSize.Split(" ")(1)

        Try

            Compact.wkPreSizeVal.Text = Math.Round(folderSize, 1)
            Compact.wkPreSizeUnit.Text = suffix
            Dim wkPreSizeVal_Len = TextRenderer.MeasureText(Compact.wkPreSizeVal.Text, Compact.wkPreSizeVal.Font)
            Compact.wkPreSizeUnit.Location = New Point(Compact.wkPreSizeVal.Location.X + (Compact.wkPreSizeVal.Size.Width / 2) + (wkPreSizeVal_Len.Width / 2 - 8), Compact.wkPreSizeVal.Location.Y + 16)

            ' I still have no idea why this catch is needed but I'm scared to delete it
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



End Class



Public Class Result

    Property Name As String
    Property Name_Sanitised As String
    Property Folder As String
    Property SteamID As Integer
    Property Algorithm As String
    Property BeforeSize As UInt64
    Property BeforeSize_Formatted As String
    Property AfterSize As UInt64
    Property AfterSize_Formatted As String
    Property Ratio As Decimal

    Public ReadOnly Property AllData()
        Get
            Return Name & Folder & SteamID & Algorithm & AfterSize
        End Get
    End Property

    Public Sub New(ByVal nm As String,
                   ByVal fl As String,
                   ByVal stID As Integer,
                   ByVal alg As String,
                   ByVal bef As UInt64,
                   ByVal aft As UInt64)

        Name = nm
        Name_Sanitised = Regex.Replace(nm.ToLower, "[^\p{L}a-zA-Z0-90]", "")
        Folder = Regex.Replace(fl.ToLower, "[^\p{L}a-zA-Z0-90]", "")
        SteamID = stID
        Algorithm = alg
        BeforeSize = bef
        BeforeSize_Formatted = Compact.GetOutputSize(bef, True)
        AfterSize = aft
        AfterSize_Formatted = Compact.GetOutputSize(aft, True)
        Ratio = Math.Round(AfterSize / BeforeSize, 2)
    End Sub

End Class
