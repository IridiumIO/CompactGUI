Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Ookii.Dialogs                                                                          'Uses Ookii Dialogs for the non-archaic filebrowser dialog. http://www.ookii.org/Software/Dialogs
Imports System.Management


Public Class Compact
    Shared version = "2.3.3"
    Private WithEvents MyProcess As Process
    Private Delegate Sub AppendOutputTextDelegate(ByVal text As String)


    Private Shared Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(CompactGUI.Compact)
    End Sub


    Protected Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Const CS_DROPSHADOW = &H20000
            Dim cp As CreateParams = MyBase.CreateParams
            cp.Style = cp.Style Or &H20000                                                  '&H20000 = WS_MINIMIZEBOX
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW Or &H8                           '&H8 = CS_DBLCLKS
            Return cp
        End Get
    End Property



    'Status Monitors
    Dim compressFinished = 0
    Dim uncompressFinished = 0
    Dim isQueryMode = 0
    Dim isQueryCalledByCompact = 0
    Dim isActive = 0
    Dim dirCountProgress As Int64
    Dim dirCountTotal As Int64
    Dim fileCountTotal As Int64 = 0
    Dim fileCountProgress As Int64
    Dim fileCountOutputCompressed As Int64
    Dim QdirCountProgress As Int64


    Private Sub MyProcess_ErrorDataReceived _
        (ByVal sender As Object, ByVal e As System.Diagnostics.DataReceivedEventArgs) _
        Handles MyProcess.ErrorDataReceived

        AppendOutputText(vbCrLf & e.Data)                                                       'Ensures error data is printed to the output console

    End Sub


#Region "Format Messages by MUI Table"
    Private Const MSG_INDEX_TOTALFILES As String = "%1"
    Private Const MSG_INDEX_TOTALDIRECTORIES As String = "%2"
    Private Const MSG_INDEX_FILESCOMPRESSEDCOUNT As String = "%3"
    Private Const MSG_INDEX_FILESNOTCOMPRESSEDCOUNT As String = "%4"
    Private Const MSG_INDEX_TOTALBYTESUNCOMPRESSED As String = "%5"
    Private Const MSG_INDEX_TOTALBYTESCOMPRESSED As String = "%6"
    Private Const MSG_INDEX_COMPRESSIONRATIO As String = "%7"



    'FormatMessage substrings   
    Dim fmt8 As String = GetMessageFromModule("compact.exe", 8)   'Analysis Endlines
    Dim fmt7 As String = GetMessageFromModule("compact.exe", 7)   'Listing[] Lines - directory count
    Dim fmt1 As String = GetMessageFromModule("compact.exe", 1)   '[OK] Line - file count
    Dim fmt10 As String = GetMessageFromModule("compact.exe", &H10) 'Uncompression finished line
    Dim fmtC As String = GetMessageFromModule("compact.exe", &HC)   'Compression finished Endlines

    Dim fixedfmt8 = fmt8.Trim()                            'removes the two leading spaces before the analysis endstring so that formatting works. 
    Dim fixedfmt7 = fmt7.Trim()
    Dim fixedfmt1 = fmt1.Trim()
    Dim fixedfmt10 = fmt10.Trim()
    Dim fixedfmtc = fmtC.Trim()


    Dim FMT_ANALYSIS_MSG As String() = fixedfmt8.Split(vbCrLf)                   'splits the lone message into its four components (see the above ANALYSIS OUTPUT comment
    Dim FMT_LISTING_MSG As String = fixedfmt7.split(vbCrLf)(0)
    Dim FMT_UNCOMPRESSED_MSG As String = fixedfmt10.split(vbCrLf)(0)
    Dim FMT_COMPRESSED_MSG As String() = fixedfmtc.split(vbCrLf)

    'These aren't currently used
    Dim FMT_FILESWITHINDIRECTORIES1 As String = Before(fmt8.Replace(vbCrLf, ""), MSG_INDEX_TOTALFILES)
    Dim FMT_FILESWITHINDIRECTORIES2 As String = Between(fmt8.Replace(vbCrLf, ""), MSG_INDEX_TOTALFILES, MSG_INDEX_TOTALDIRECTORIES)
    Dim FMT_FILESCOMPRESSED As String = ""

    'Gets the relevant lines from the FMT_XXX_MSG Arrays



    'Index values that are found while parsing the console output. 
    Dim CON_INDEX_TOTALFILES As Integer = 1
    Dim CON_INDEX_TOTALDIRECTORIES As Integer = 1
    Dim CON_INDEX_FILESCOMPRESSEDCOUNT As Integer = 1
    Dim CON_INDEX_FILESNOTCOMPRESSEDCOUNT As Integer = 1
    Dim CON_INDEX_TOTALBYTESCOMPRESSED As Integer = 1
    Dim CON_INDEX_TOTALBYTESNOTCOMPRESSED As Integer = 1
    Dim CON_INDEX_COMPRESSIONRATIO As Integer = 1

    'e.Data Output Strings from the console - each of these is one of the four lines at the end of the console output. 
    Dim CON_FILESWITHINDIRECTORIESLINE
    Dim CON_FILESCOMPRESSEDLINE
    Dim CON_TOTALBYTESLINE
    Dim CON_COMPRATIO


    'Output Arrays - These start of with the %n values in them from the MUI tables, but when the console output is parsed the %n is replaced with the actual data, and the index of that is stored in CON_INDEX variables above
    Dim ARR_FILESWITHINDIRECTORIES As String() = fixedfmt8.split()
    Dim ARR_FILESCOMPRESSED As String() = fixedfmt8.split()
    Dim ARR_TOTALBYTES As String() = fixedfmt8.split()
    Dim ARR_COMPRATIO As String() = fixedfmt8.split()
    Dim ARR_LISTING As String() = fixedfmt7.split()



    'Counts up from the first results line until it find four lines
    Dim OutputlineIndex = 0
    Dim canProceed = 0



    Dim REGEX_NUMBERFORMATTER As New Regex("(?<=\d+)\s+(?=\d+)")

#End Region

    Public Function CALC_OUTPUT(edata As String)
        Dim CONINPUTDATA As String() = REGEX_NUMBERFORMATTER.Replace(edata, "").Trim().Split(" ")

        Dim FMTFilesWithin As String() = FMT_ANALYSIS_MSG(0).Split(" ")
        Dim FMTCompNotComp As String() = FMT_ANALYSIS_MSG(1).Trim(vbCrLf).Split(" ")
        Dim FMTTotalBytes As String() = (FMT_ANALYSIS_MSG(2).Trim(vbCrLf)).Split(" ")
        Dim FMTCompRatio As String() = FMT_ANALYSIS_MSG(3).Trim(vbCrLf).Split(" ")
        Dim FMTListing As String() = FMT_LISTING_MSG.Trim().Split(" ")
        Dim FMTUncompressed As String() = FMT_UNCOMPRESSED_MSG.Split(" ")
        Dim FMTCompressFinished As String() = FMT_COMPRESSED_MSG(FMT_COMPRESSED_MSG.Count - 1).Trim(vbCrLf).Split(" ")





        'LISTING - DIRECTORY COUNT
        If FMTListing(0) = CONINPUTDATA(0) Then
            QdirCountProgress += 1
        End If


        'OK - FILE COUNT
        If edata.EndsWith(fixedfmt1) Then
            fileCountProgress += 1
        End If


        'Uncompressed - Checks if uncompression is finished
        If FMTUncompressed.Count = CONINPUTDATA.Count _
            And (FMTUncompressed(FMTUncompressed.Count - 1) = CONINPUTDATA(CONINPUTDATA.Count - 1) _
                Or FMTUncompressed(FMTUncompressed.Count - 1).Contains("%2")) Then

            dirCountProgress = 0
            fileCountProgress = fileCountTotal
            uncompressFinished = 1
            isActive = 0

        End If


        'Compress Finished Ratio - Checks if compression is finished
        If FMTCompressFinished.Count = CONINPUTDATA.Count _
            And OutputlineIndex = 0 _
            And (FMTCompressFinished(FMTCompressFinished.Count - 1) = CONINPUTDATA(CONINPUTDATA.Count - 1) _
                Or CONINPUTDATA(CONINPUTDATA.Count - 1).Contains("1.")) Then

            compressFinished = 1
            dirCountProgress = dirCountTotal
            fileCountProgress = fileCountTotal
            isActive = 0

        End If


        'Analysis Complete - Gets the lines when analysing a folder is completed
        If FMTFilesWithin.Count = CONINPUTDATA.Count _
            And (FMTFilesWithin(0) = CONINPUTDATA(0) _
                Or FMTFilesWithin(0).Contains("%1")) Then

            Return OutputLines(FMTFilesWithin, CONINPUTDATA, CON_INDEX_TOTALFILES, CON_INDEX_TOTALDIRECTORIES, ARR_FILESWITHINDIRECTORIES, "%1", "%2")

        End If

        If OutputlineIndex = 1 Then
            Return OutputLines(FMTCompNotComp, CONINPUTDATA, CON_INDEX_FILESCOMPRESSEDCOUNT, CON_INDEX_FILESNOTCOMPRESSEDCOUNT, ARR_FILESCOMPRESSED, "%3", "%4")
        End If
        If OutputlineIndex = 2 Then
            Return OutputLines(FMTTotalBytes, CONINPUTDATA, CON_INDEX_TOTALBYTESNOTCOMPRESSED, CON_INDEX_TOTALBYTESCOMPRESSED, ARR_TOTALBYTES, "%5", "%6")
        End If
        If OutputlineIndex = 3 Then
            Return OutputLines(FMTCompRatio, CONINPUTDATA, CON_INDEX_COMPRESSIONRATIO, CON_INDEX_COMPRESSIONRATIO, ARR_COMPRATIO, "%7")
        End If


        Return ("Nothing")

    End Function


    Public Function OutputLines(ByRef FMTVal As Object, ByRef CONVal As Object, ByRef CON_Index1 As Object, ByRef CON_Index2 As Object, ByRef ARRVal As Object, ByRef Val1 As String, Optional ByRef Val2 As String = "%xnull")

        Dim i = 0
        For Each c In FMTVal
            If c.Contains(Val1) Then
                FMTVal(i) = CONVal(i)
                CON_Index1 = i

            ElseIf c.Contains(Val2) Then
                FMTVal(i) = CONVal(i)
                CON_Index2 = i
            End If
            i += 1
        Next

        Dim builder As New StringBuilder
        Dim b = 0
        For Each c In FMTVal
            builder.Append(FMTVal(b))
            builder.Append(" ")
            b += 1
        Next
        ARRVal = FMTVal
        Return builder.ToString

    End Function





    Private Sub MyProcess_OutputDataReceived _
        (ByVal sender As Object, ByVal e As System.Diagnostics.DataReceivedEventArgs) _
        Handles MyProcess.OutputDataReceived

        AppendOutputText(vbCrLf & e.Data)                                                               'Sends output to the embedded console


        If e.Data <> Nothing Then
            If e.Data.Contains(CALC_OUTPUT(e.Data).ToString.Trim(" ")) And canProceed = 0 Then          'If the output line of the console is the "%files within" line then do stuff. Trim gets rid of the spaces before and after some lines
                CON_FILESWITHINDIRECTORIESLINE = e.Data.Trim(" ")                                           ' This variable can't get set if the first criteria fails. This means that the console output is not parsing the russian properly. 
                Console.WriteLine("Files: " +
                    ARR_FILESWITHINDIRECTORIES(CON_INDEX_TOTALFILES) + " Directories: " +
                    ARR_FILESWITHINDIRECTORIES(CON_INDEX_TOTALDIRECTORIES))
                canProceed = 1
            End If
        End If



        If OutputlineIndex = 1 And canProceed = 1 Then                                                  ' These all run after the one above is met, since if the one above is met then it means there's only 3 lines left. 
            CON_FILESCOMPRESSEDLINE = e.Data
            CALC_OUTPUT(e.Data)
            Console.WriteLine("Compressed: " +
              ARR_FILESCOMPRESSED(CON_INDEX_FILESCOMPRESSEDCOUNT) + " Not Compressed: " +
              ARR_FILESCOMPRESSED(CON_INDEX_FILESNOTCOMPRESSEDCOUNT))
        End If


        If OutputlineIndex = 2 Then
            CON_TOTALBYTESLINE = e.Data
            CALC_OUTPUT(e.Data)
            Console.WriteLine("Bytes Compressed: " +
                ARR_TOTALBYTES(CON_INDEX_TOTALBYTESNOTCOMPRESSED) + " In Total Bytes: " +
                ARR_TOTALBYTES(CON_INDEX_TOTALBYTESCOMPRESSED))
        End If

        If OutputlineIndex = 3 Then
            CON_COMPRATIO = e.Data
            CALC_OUTPUT(e.Data)

            Console.WriteLine("Ratio: " +
                ARR_COMPRATIO(CON_INDEX_COMPRESSIONRATIO) + " to 1.")

            compressFinished = 1
            dirCountProgress = dirCountTotal
            fileCountProgress = fileCountTotal
            isActive = 0

            canProceed = 0
            OutputlineIndex = 0
        End If


        If canProceed = 1 Then
            OutputlineIndex += 1
        End If

    End Sub






    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        loadFromSettings()

        If dirChooser.Text = "❯   Select Target Folder" Then
            panel_topBar.Height = Me.Height - 1
            panel_topBar.Anchor += AnchorStyles.Bottom
            With topbar_title
                .AutoSize = False
                .TextAlign = ContentAlignment.MiddleCenter
                .Font = New Font(topbar_title.Font.Name, 32, FontStyle.Regular)
                .Width = panel_topBar.Width
                .Height = topbar_title.Font.Height
                .Location = New Point(0, panel_topBar.Height / 2 - 150)
                .Anchor += AnchorStyles.Right
            End With
            topbar_dirchooserContainer.Location = New Point(44, panel_topBar.Height / 2 - 22)
        End If

        dirChooser.LinkColor = Color.White

        comboChooseShutdown.SelectedItem = comboChooseShutdown.Items.Item(0)

        RCMenu.WriteLocRegistry()
        VersionCheck.VC(version)
        progressTimer.Start()                                                                   'Starts a timer that keeps track of changes during any operation.

        For Each arg In My.Application.CommandLineArgs
            If arg.ToString IsNot Nothing Then
                SelectFolder(arg, "cmdlineargs")
            End If
        Next

    End Sub




    Private Sub ShowInfoPopup_Click(sender As Object, e As EventArgs) Handles showinfopopup.Click
        Info.semVersion.Text = "V " + version
        Info.Show()
    End Sub




    Private Sub ProgressTimer_Tick(sender As Object, e As EventArgs) Handles progressTimer.Tick

        If fileCountTotal <> 0 Then                                                                         'Makes sure that there are actually files being counted before attempting a calculation


            Try
                If sb_progressbar.Width > 301 Then                                                 'Avoids a /r/softwaregore scenario
                    sb_progressbar.Width = 301
                    progresspercent.Text = "100 %"
                    topbar_progress.Width = topbar_dirchooserContainer.Width

                ElseIf isQueryMode = 0 Then
                    sb_progressbar.Width = Math.Round _
                            ((fileCountProgress / fileCountTotal * 301), 0)
                    topbar_progress.Width = Math.Round _
                            ((fileCountProgress / fileCountTotal * topbar_dirchooserContainer.Width), 0)
                    progresspercent.Text = Math.Round _
                            ((fileCountProgress / fileCountTotal * 100), 0).ToString + " %"                 'Generates an estimate of progress based on how many files have been processed out of the total. 

                ElseIf isQueryMode = 1 Then
                    sb_progressbar.Width = Math.Round _
                            ((QdirCountProgress / dirCountTotal * 301), 0)
                    topbar_progress.Width = Math.Round _
                            ((QdirCountProgress / dirCountTotal * topbar_dirchooserContainer.Width), 0)
                    progresspercent.Text = Math.Round _
                            ((QdirCountProgress / dirCountTotal * 100), 0).ToString + " %"                  'Generates an estimate of progress for the Query command.
                End If
            Catch ex As Exception
                Console.WriteLine("PE: " + ex.Data.ToString)
            End Try

        End If



        If compressFinished = 1 Then                                                                        'Hides and shows certain UI elements when compression is finished or if a compression status is being checked

            If isQueryMode And isQueryCalledByCompact = 0 Then
                sb_progresslabel.Text = "This folder contains compressed items"
                progresspercent.Visible = False
                'sb_AnalysisPanel.Visible = False

            End If

            compressFinished = 0
            If checkShutdownOnCompletion.Checked = True And isQueryMode = 0 Then
                ShutdownDialog.SDProcIntent.Text = comboChooseShutdown.Text
                FadeTransition.FadeForm(ShutdownDialog, 0, 0.98, 300, True)

            End If

            buttonRevert.Visible = True
            returnArrow.Visible = True
            CalculateSaving()
            QdirCountProgress = 0
            buttonQueryCompact.Enabled = True
            'dirChooser.Enabled = True

        End If

        If uncompressFinished = 1 Then                                                                      'Hides and shows certain UI elements when uncompression is finished 

            uncompressFinished = 0

            buttonRevert.Visible = False
            sb_progresslabel.Text = "Folder Uncompressed."
            sb_compressedSizeVisual.Height = 113
            wkPostSizeVal.Text = "?"
            wkPostSizeUnit.Text = ""
            sb_labelCompressed.Text = "Estimated Compressed"
            sb_ResultsPanel.Visible = False
            returnArrow.Visible = True
            buttonQueryCompact.Enabled = True
            ' dirChooser.Enabled = True

            If checkShutdownOnCompletion.Checked = True Then
                ShutdownDialog.SDProcIntent.Text = comboChooseShutdown.Text
                FadeTransition.FadeForm(ShutdownDialog, 0, 0.98, 200, True)
            End If

        End If



    End Sub




    Private Sub AppendOutputText(ByVal text As String)                                           'Attach output to the embedded console
        Try
            If conOut.InvokeRequired Then
                Dim serverOutDelegate As New AppendOutputTextDelegate(AddressOf AppendOutputText)
                Me.Invoke(serverOutDelegate, text)
            Else
                If text <> vbCrLf Then
                    conOut.Items.Insert(0, text)
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub




    'Set variables for minor security and error handling
    Dim overrideCompressFolderButton = 0
    Dim directorysizeexceptionCount = 0                                                         'Used in the DirectorySize() Function to ensure the error message only shows up once, even if multiple UnauthorizedAccessException errors get thrown




    Dim uncompressedfoldersize
    Dim workingDir As String = ""




    Private Sub SelectFolderToCompress(sender As Object, e As EventArgs) Handles dirChooser.LinkClicked, dirChooser.Click
        If isActive = 0 And isQueryMode = 0 Then
            sb_AnalysisPanel.Visible = False
            buttonCompress.Visible = True
            overrideCompressFolderButton = 0

            Dim folderChoice As New FileFolderDialog
            folderChoice.ShowDialog()
            If Directory.Exists(folderChoice.SelectedPath) Then
                SelectFolder(folderChoice.SelectedPath, "button")
            ElseIf File.Exists(folderChoice.selectedpath) Then
                If folderChoice.MultipleFiles IsNot Nothing Then
                    MsgBox("Multiple Files Selected")
                Else
                    MsgBox("File selected")
                End If

            End If
            folderChoice.Dispose()
        End If

    End Sub

    Dim dirLabelResults As String = ""

    Private Sub SelectFolder(selectedDir As String, senderID As String)
        Cursor.Current = Cursors.WaitCursor

        Dim wDString = selectedDir

        If selectedDir.Contains("C:\Windows") Then : ThrowError(ERR_WINDOWSDIRNOTALLOWED)                                    'Makes sure you're not trying to compact the Windows directory. I should Regex this to catch all possible drives hey?
        ElseIf selectedDir.EndsWith(":\") Then : ThrowError(ERR_WHOLEDRIVENOTALLOWED)
        Else
            If selectedDir.Length >= 3 Then                                                                                    'Makes sure the chosen folder isn't a null value or an exception
                workingDir = selectedDir
                Dim DIwDString = New DirectoryInfo(selectedDir)
                directorysizeexceptionCount = 0

                If DIwDString.Name.ToString.Length > 0 Then sb_FolderName.Text =
                    DIwDString.Name.ToString.Substring(0, 1).ToUpper + DIwDString.Name.ToString.Substring(1)

                Try
                    If DIwDString.Parent.Parent.ToString <> Nothing Then
                        dirChooser.Text = "❯ " + DIwDString.Parent.Parent.ToString +
                            " ❯ " + DIwDString.Parent.ToString +
                            " ❯ " + DIwDString.Name.ToString
                    Else
                        dirChooser.Text = "❯ " + DIwDString.Root.ToString.Replace(":\", "") +
                            " ❯ " + DIwDString.Parent.ToString +
                            " ❯ " + DIwDString.Name.ToString
                    End If

                Catch ex As Exception
                    dirChooser.Text = "❯ " + DIwDString.Root.ToString.Replace(":\", "") +
                        " ❯ " + DIwDString.Name.ToString
                End Try

                uncompressedfoldersize = Math.Round(DirectorySize(DIwDString, True), 1)

                Dim rawpreSize = GetOutputSize _
                   (uncompressedfoldersize, True)

                preSize.Text = "Uncompressed Size: " + rawpreSize

                dirLabelResults = DIwDString.Name.ToString

                Try
                    Dim directories() As String = Directory.GetDirectories _
                        (selectedDir, "*", SearchOption.AllDirectories)

                    dirCountTotal = directories.Length + 1

                    Dim numberOfFiles As Int64 = Directory.GetFiles _
                       (selectedDir, "*", SearchOption.AllDirectories).Length

                    fileCountTotal = numberOfFiles
                    sb_ResultsPanel.Visible = False

                    UnfurlTransition.UnfurlControl(topbar_dirchooserContainer, topbar_dirchooserContainer.Width, Me.Width - sb_Panel.Width - 44, 100)
                    WikiHandler.localFolderParse(selectedDir, DIwDString, rawpreSize)

                    With topbar_title
                        .Anchor -= AnchorStyles.Right
                        .AutoSize = True
                        .TextAlign = ContentAlignment.MiddleLeft
                        .Font = New Font(topbar_title.Font.Name, 15.75, FontStyle.Regular)
                        .Location = New Point(59, 18)
                    End With
                    returnArrow.Visible = False
                    buttonRevert.Visible = False
                    CompResultsPanel.Visible = False
                    checkShutdownOnCompletion.Checked = False
                    TabControl1.SelectedTab = InputPage
                    dirCountProgress = 0
                    fileCountProgress = 0
                    isQueryCalledByCompact = 0
                    MyProcess.Kill()
                    sb_AnalysisPanel.Visible = False


                Catch ex As Exception
                End Try

                If overrideCompressFolderButton = 0 Then                                        'Used as a security measure to stop accidental compression of folders that should not be compressed - even though the compact.exe process will throw an error if you try, I'd prefer to catch it here anyway. 
                    buttonCompress.Enabled = True
                Else
                    buttonCompress.Enabled = False
                End If

            Else

                If senderID = "button" Then Console.Write("No folder selected")

            End If

        End If

    End Sub




    Dim compactArgs As String




    Private Sub CompressFolder_Click(sender As System.Object, e As System.EventArgs) Handles buttonCompress.Click
        conOut.Items.Clear()
        CreateProcess("compact")
        sb_AnalysisPanel.Visible = True
        buttonCompress.Visible = False
        buttonQueryCompact.Enabled = False
        'dirChooser.Enabled = False
    End Sub
    Private Sub buttonQueryCompact_Click(sender As Object, e As EventArgs) Handles buttonQueryCompact.Click
        conOut.Items.Clear()
        CreateProcess("query")
        buttonCompress.Visible = False
        sb_Panel.Show()                                 'Temporary Fix - go back and work out why #124 doesn't work with WikiParser()
        sb_AnalysisPanel.Visible = True
        buttonQueryCompact.Enabled = False
        'dirChooser.Enabled = False
    End Sub








    Private Sub ButtonRevert_Click(sender As Object, e As EventArgs) Handles buttonRevert.Click             'Handles uncompressing. For now, uncompressing can only be done through the program only to revert a compression that's just been done.
        isQueryMode = 0
        fileCountProgress = 0
        dirCountProgress = 0
        progresspercent.Visible = True
        CompResultsPanel.Visible = False
        buttonQueryCompact.Enabled = False
        'dirChooser.Enabled = False
        buttonRevert.Visible = False


        Try
            RunCompact("uncompact")
        Catch ex As Exception
        End Try

    End Sub




    Private Sub compressLZX_CheckedChanged(sender As Object, e As EventArgs) Handles compressLZX.CheckedChanged     'Cautions the user if they're about to use LZX compression

        If compressLZX.Checked = True Then

            If MsgBox("LZX is recommended only for folders that are not going to be used very often. Do not use this on program or game folders!" _
                      & vbCrLf & vbCrLf & "Do you wish to continue?", MsgBoxStyle.YesNo, "Warning") = MsgBoxResult.No Then

                compressX8.Checked = True

            End If

        End If
    End Sub




    Private Sub ReturnArrow_Click(sender As Object, e As EventArgs) Handles returnArrow.Click                       'Returns you to the first screen and cleans up some stuff

        returnArrow.Visible = False
        buttonRevert.Visible = False
        CompResultsPanel.Visible = False
        checkShutdownOnCompletion.Checked = False
        TabControl1.SelectedTab = InputPage
        dirCountProgress = 0
        fileCountProgress = 0
        isQueryCalledByCompact = 0
        'MyProcess.Kill()
        sb_AnalysisPanel.Visible = False
        buttonCompress.Visible = True
        buttonQueryCompact.Enabled = True
        'dirChooser.Enabled = True
    End Sub




    Private Sub CheckShowConOut_CheckedChanged(sender As Object, e As EventArgs) Handles checkShowConOut.CheckedChanged     'Handles showing the embedded console
        If checkShowConOut.Checked Then
            conOut.Visible = True
            saveconlog.Visible = True
        Else
            conOut.Visible = False
            saveconlog.Visible = False
        End If
    End Sub





    Private Sub MyForm_Closing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If isActive = 1 Then

            If MessageBox.Show _
                ("Are you sure you want to exit?" & vbCrLf & vbCrLf & "Quitting now will finish compressing the current file, then quit safely." _
                 & vbCrLf & vbCrLf &
                 "If you do decide to quit now, you can resume compression by repeating the process later." _
                 & vbCrLf & "Click Yes to continue exiting the program.",
                 "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then

                e.Cancel = True

            Else
                Try
                    LetsKillStuffButSafelyNow()
                Catch ex As Exception
                End Try
            End If
        Else
            Try
                LetsKillStuffButSafelyNow()
            Catch ex As Exception
            End Try
        End If

    End Sub




    Private Sub dirChooser_MouseEnter(sender As Object, e As EventArgs) Handles dirChooser.MouseEnter
        dirChooser.LinkColor = Color.FromArgb(200, 200, 200)
    End Sub
    Private Sub dirChooser_MouseLeave(sender As Object, e As EventArgs) Handles dirChooser.MouseLeave
        dirChooser.LinkColor = Color.White
    End Sub







    '/////////////FUNCTIONS//////////////

    Private Sub CalculateSaving()   'Calculations for all the relevant information after compression is completed. All the data is parsed from the console ouput using basic strings, but because that occurs on a different thread, information is stored to variables first (The Status Monitors at the top) then those values are used. 

        Dim numberFilesCompressed = 0
        Dim querySize As Int64 = 0


        'If isQueryMode = 0 Then querySize = Long.Parse(Regex.Replace(ARR_TOTALBYTES(CON_INDEX_TOTALBYTESNOTCOMPRESSED), "[^\d]", ""))


        Dim oldFolderSize As Long = 999999999

        Dim newFolderSize As Long = 999999999
        Try
            oldFolderSize = Long.Parse(Regex.Replace(ARR_TOTALBYTES(CON_INDEX_TOTALBYTESNOTCOMPRESSED), "[^\d]", ""))
        Catch ex As Exception

        End Try

        Try
            newFolderSize = Long.Parse(Regex.Replace(ARR_TOTALBYTES(CON_INDEX_TOTALBYTESCOMPRESSED), "[^\d]", ""))
        Catch ex As Exception

        End Try


        Try
            numberFilesCompressed = Long.Parse(Regex.Replace(ARR_FILESCOMPRESSED(CON_INDEX_FILESCOMPRESSEDCOUNT), "[^\d]", ""))
        Catch ex As Exception
        End Try

        If GetOutputSize((oldFolderSize - newFolderSize), False) = "0" And isQueryMode = 1 Then

            sb_progresslabel.Text = "Folder is not compressed"
            buttonRevert.Visible = False
            isQueryCalledByCompact = 0

        Else

            sb_progresslabel.Text = "Folder is compressed"

            If isQueryMode = 1 And isQueryCalledByCompact = 0 Then
                origSizeLabel.Text = GetOutputSize(oldFolderSize, True)
            Else
                origSizeLabel.Text = GetOutputSize(uncompressedfoldersize, True)
            End If

            Dim PrintOutSize = GetOutputSize _
                (uncompressedfoldersize - (oldFolderSize - newFolderSize), True)
            Dim suffix = PrintOutSize.Split(" ")(1)
            compressedSizeLabel.Text = PrintOutSize
            wkPostSizeVal.Text = PrintOutSize.Split(" ")(0)
            wkPostSizeUnit.Text = suffix
            Dim wkPostSizeVal_Len = TextRenderer.MeasureText(wkPostSizeVal.Text, wkPostSizeVal.Font)
            wkPostSizeUnit.Location = New Point(wkPostSizeVal.Location.X + (wkPostSizeVal.Size.Width / 2) + (wkPostSizeVal_Len.Width / 2 - 8), wkPostSizeVal.Location.Y + 16)
            sb_labelCompressed.Text = "Compressed"

            Try
                compRatioLabel.Text = Math.Round _
                (oldFolderSize / newFolderSize, 1)

                compRatioLabel.Text = Math.Round _
                (uncompressedfoldersize / (uncompressedfoldersize - (oldFolderSize - newFolderSize)), 1)
            Catch ex As DivideByZeroException
                MsgBox("Stop trying to compress empty folders")
            End Try


            spaceSavedLabel.Text = GetOutputSize _
                ((oldFolderSize - newFolderSize), True) + " Saved"
            sb_SpaceSavedLabel.Text = spaceSavedLabel.Text
            dirChosenLabel.Text = "❯ In " + dirLabelResults

            labelFilesCompressed.Text =
                numberFilesCompressed.ToString + " / " + fileCountTotal.ToString + " files compressed"
            help_resultsFilesCompressed.Location = New Point(labelFilesCompressed.Location.X + labelFilesCompressed.Width + 2, labelFilesCompressed.Location.Y + 1)
            Try

                compressedSizeVisual.Width = CInt(320 / compRatioLabel.Text)
                Callpercent = (1 - (1 / CDec(ARR_COMPRATIO(CON_INDEX_COMPRESSIONRATIO)))) * 100
                sb_compressedSizeVisual.Height = CInt(113 / compRatioLabel.Text)
                sb_compressedSizeVisual.Location = New Point(sb_compressedSizeVisual.Location.X, 5 + 113 - sb_compressedSizeVisual.Height)
                'sb_Panel.Refresh()

                If hasqueryfinished = 1 Then
                    isQueryCalledByCompact = 0
                    isQueryMode = 0
                    buttonRevert.Visible = True
                End If

            Catch ex As System.OverflowException
                compressedSizeVisual.Width = 320
                sb_compressedSizeVisual.Height = 113
            End Try

            If isQueryCalledByCompact = 0 Then

                CompResultsPanel.Visible = True
                sb_ResultsPanel.Visible = True
                PaintPercentageTransition.PaintTarget(results_arc, Callpercent)
            ElseIf isQueryCalledByCompact = 1 Then

                sb_progresslabel.Text = "Analyzing..."

                buttonRevert.Visible = False
                CompResultsPanel.Visible = False
                sb_ResultsPanel.Visible = False

            End If

        End If

        If isQueryCalledByCompact = 1 Then Queryaftercompact()
        If isQueryCalledByCompact = 0 Then isQueryMode = 0

    End Sub







    Public Function GetOutputSize(ByVal inputsize As Decimal, Optional ByVal showSizeType As Boolean = False) As String            'Function for converting from Bytes into various units
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
            Return Math.Round(inputsize, 1) & sizeType
        Else
            Return Math.Round(inputsize, 1)
        End If

    End Function



    Private Function DirectorySize _
        (ByVal dInfo As IO.DirectoryInfo, ByVal includeSubdirectories As Boolean) As Long


        Try

            Dim totalSize As Long = dInfo.EnumerateFiles().Sum(Function(file) file.Length)

            If includeSubdirectories Then
                totalSize += dInfo.EnumerateDirectories().Sum(Function(dir) DirectorySize(dir, True))
            End If

            Return totalSize

        Catch generatedexceptionname As UnauthorizedAccessException
            directorysizeexceptionCount += 1


            If directorysizeexceptionCount = 1 Then

                overrideCompressFolderButton = 1
                directorysizeexceptionCount += 1

                If My.User.IsInRole(ApplicationServices.BuiltInRole.Administrator) = False Then
                    ThrowError(ERR_UNAUTHORISEDREQUIRESADMIN)
                Else
                    ThrowError(ERR_UNAUTHORISEDREQUIRESSYSTEM)
                End If

            End If

        Catch ex As Exception

        End Try

    End Function




    Private Function getEncoding() As Encoding
        Dim CPGet = New Process
        With CPGet.StartInfo
            .FileName = "cmd.exe"
            .Arguments = "/c chcp"
            .UseShellExecute = False
            .CreateNoWindow = True
            .StandardOutputEncoding = Encoding.Default
            .StandardErrorEncoding = Encoding.Default
            .WorkingDirectory = workingDir
            .RedirectStandardInput = True
            .RedirectStandardOutput = True
            .RedirectStandardError = True
        End With
        CPGet.Start()

        Dim Res = CPGet.StandardOutput.ReadLine()
        Dim CPa = Integer.Parse(Regex.Replace(Res, "[^\d]", ""))
        CPGet.StandardInput.WriteLine("exit")
        CPGet.StandardInput.Flush()
        CPGet.WaitForExit()
        Return Encoding.GetEncoding(CPa)
    End Function




    Private Sub LetsKillStuffButSafelyNow()
        If MyProcess.HasExited = False Then MyProcess.Kill()
    End Sub




    '//////////////FORMAT MESSAGES FROM MUITABLE FOR LOCALISATION///////////////////////////////////////////

    <DllImport("Kernel32.dll", EntryPoint:="FormatMessageW",
               SetLastError:=True, CharSet:=CharSet.Unicode, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function FormatMessage(
        ByVal dwFlags As Integer,
        ByVal lpSource As Integer,
        ByVal dwMessageId As Integer,
        ByVal dwLanguageId As Integer,
        <MarshalAs(UnmanagedType.LPWStr)> ByRef lpBuffer As String,
        ByVal nSize As Integer,
        ByRef Arguments As IntPtr) As Integer
    End Function


    <DllImport("kernel32.dll")>
    Private Shared Function LoadLibraryEx(
        lpFileName As String,
        hReservedNull As IntPtr,
        dwFlags As UInteger) As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Private Shared Function LoadLibraryA(
        lpFileName As String) As IntPtr
    End Function

    Private Const FORMAT_MESSAGE_FROM_HMODULE As Long = &H800


    Public Function GetMessageFromModule(
        ByRef strModuleName As String,
        ByVal msgID As Long) As String

        Dim rt As Long
        Dim sCodes As String
        Dim bufferStr As String
        Dim hModule As Integer

        'hModule = LoadLibraryEx("kernel32.dll", IntPtr.Zero, &H2)
        hModule = LoadLibraryA(strModuleName)

        If hModule <> 0 Then
            bufferStr = Space(12)
            Try
                rt = FormatMessage(FORMAT_MESSAGE_FROM_HMODULE Or &H100 Or &H200,
                       hModule, msgID, 0&, bufferStr, Len(bufferStr), 0&)
            Catch ex As Exception
            End Try

            If rt Then
                bufferStr = Microsoft.VisualBasic.Left$(bufferStr, rt)
                Return bufferStr
                sCodes = "Dec: " & msgID & vbTab & "Hex: " & Hex(msgID)
                GetMessageFromModule = bufferStr & vbCrLf & sCodes
            End If

        End If

    End Function


    '/////END FORMAT MESSAGES FROM MUI///////////////////////////////////////////


    '///////EXTRA FUNCTIONS/////////////

    Function Between(value As String, a As String, b As String) As String

        Dim posA As Integer = value.IndexOf(a)
        Dim posB As Integer = value.LastIndexOf(b)
        If posA = -1 Then
            Return ""
        End If
        If posB = -1 Then
            Return ""
        End If

        Dim adjustedPosA As Integer = posA + a.Length
        If adjustedPosA >= posB Then
            Return ""
        End If

        Return value.Substring(adjustedPosA, posB - adjustedPosA)

    End Function


    Function Before(value As String, a As String) As String

        Dim posA As Integer = value.IndexOf(a)
        If posA = -1 Then
            Return ""
        End If
        Return value.Substring(0, posA)
    End Function


    Function After(value As String, a As String) As String

        Dim posA As Integer = value.LastIndexOf(a)
        If posA = -1 Then
            Return ""
        End If
        Dim adjustedPosA As Integer = posA + a.Length
        If adjustedPosA >= value.Length Then
            Return ""
        End If
        Return value.Substring(adjustedPosA)
    End Function



    '////////////////////TESTING////////////////////



    Private Sub Saveconlog_Click(sender As Object, e As EventArgs) Handles saveconlog.Click
        If MsgBox("Save console output?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Dim reverseCon As New List(Of Object)
            Dim sb As New System.Text.StringBuilder()

            For Each ln As Object In conOut.Items
                reverseCon.Add(ln)
            Next

            reverseCon.Reverse()
            sb.AppendLine("CompactGUI: Log at " & System.DateTime.Now & vbCrLf _
                          & "//////////////////////////////////////////////////////////////////////////////////" _
                          & "//////////////////////////////////////////////////////////////////////////////////")

            For Each ln As Object In reverseCon
                sb.AppendLine(ln)
            Next

            sb.AppendLine("End Log at " & System.DateTime.Now & vbCrLf _
                          & "//////////////////////////////////////////////////////////////////////////////////" _
                          & "//////////////////////////////////////////////////////////////////////////////////" & vbCrLf & vbCrLf)

            System.IO.File.WriteAllText(Application.StartupPath & "\CompactGUILog.txt", sb.ToString, CP)

            MsgBox("Saved log to " & Application.StartupPath & "\CompactGUILog.txt")
        End If
    End Sub




    Private Sub seecompest_MouseHover(sender As Object, e As EventArgs) Handles seecompest.MouseHover
        topbar_title.Select()
        WikiHandler.showWikiRes()
        isAlreadyFading = 0

    End Sub

    Dim isAlreadyFading = 2
    Private Sub hideWikiRes(sender As Object, e As EventArgs) Handles MyBase.MouseEnter, TabControl1.MouseEnter,
                                InputPage.MouseEnter, FlowLayoutPanel1.MouseEnter, Panel3.MouseEnter, Panel4.MouseEnter, seecompest.MouseLeave
        If isAlreadyFading = 0 Then
            FadeTransition.FadeForm(WikiPopup, 0.96, 0, 200)
            isAlreadyFading = 1
        End If




    End Sub


    Private Sub submitToWiki_Click(sender As Object, e As EventArgs) Handles submitToWiki.Click
        Process.Start("https://goo.gl/forms/Udi5SUkMdCOMG3m23")
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles comboChooseShutdown.SelectedIndexChanged
        InputPage.Focus()

    End Sub

    Private Sub ComboBox1_MouseLeave(sender As Object, e As EventArgs) Handles comboChooseShutdown.MouseLeave
        InputPage.Focus()

    End Sub

    Private Sub compressX8_CheckedChanged(sender As Object, e As EventArgs) Handles compressX4.Click, compressX8.Click, compressX16.Click, compressLZX.Click

        If compressX4.Checked Then My.Settings.SavedCompressionOption = 0
        If compressX8.Checked Then My.Settings.SavedCompressionOption = 1
        If compressX16.Checked Then My.Settings.SavedCompressionOption = 2
        If compressLZX.Checked Then My.Settings.SavedCompressionOption = 3

    End Sub

    Private Sub loadFromSettings()

        If My.Settings.SavedCompressionOption = 0 Then compressX4.Checked = True
        If My.Settings.SavedCompressionOption = 1 Then compressX8.Checked = True
        If My.Settings.SavedCompressionOption = 2 Then compressX16.Checked = True
        If My.Settings.SavedCompressionOption = 3 Then compressLZX.Checked = True

    End Sub

    Private Sub btn_Mainexit_Click(sender As Object, e As EventArgs) Handles btn_Mainexit.Click
        Me.Close()
    End Sub

    Private Sub btn_Mainmin_Click(sender As Object, e As EventArgs) Handles btn_Mainmin.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub


    Dim isMaximised As Boolean = False
    Private Sub btn_Mainmax_Click(sender As Object, e As EventArgs) Handles btn_Mainmax.Click
        If isMaximised = False Then
            Me.MaximumSize = Screen.FromControl(Me).WorkingArea.Size
            Me.Bounds = Screen.GetWorkingArea(Me)
            isMaximised = True
        ElseIf isMaximised = True Then
            Me.Height = 652
            Me.Width = 1002
            Me.CenterToScreen()
            isMaximised = False
        End If

    End Sub

    Private Sub sb_Panel_Paint(sender As Object, e As PaintEventArgs) Handles sb_Panel.Paint
        Dim p As New Pen(Brushes.DimGray, 1)
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
        'e.Graphics.DrawLine(p, New Point(40, 180), New Point(313, 180))





    End Sub






#Region "Move And Resize"

    '//////////////////////////MOVE  AND  RESIZE  FORM//////////////////////////////////'

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

    Private Sub panel_topBar_MouseDown(sender As Object, e As MouseEventArgs) Handles panel_topBar.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Left And isMaximised = False Then MoveForm()
    End Sub




    'Dim bordercapture As Integer = 8

    'Public Enum ResizeDirection
    '    Normal = 0
    '    T = 1
    '    TR = 2
    '    R = 3
    '    RB = 4
    '    B = 5
    '    LB = 6
    '    L = 7
    '    TL = 8
    'End Enum

    'Dim RD As ResizeDirection = ResizeDirection.Normal

    'Public Property ResizeCursor() As ResizeDirection
    '    Get
    '        Return RD
    '    End Get
    '    Set(value As ResizeDirection)
    '        RD = value
    '        Select Case value
    '            Case ResizeDirection.T
    '                Me.Cursor = Cursors.SizeNS

    '            Case ResizeDirection.TR
    '                Me.Cursor = Cursors.SizeNESW

    '            Case ResizeDirection.R
    '                Me.Cursor = Cursors.SizeWE

    '            Case ResizeDirection.RB
    '                Me.Cursor = Cursors.SizeNWSE

    '            Case ResizeDirection.B
    '                Me.Cursor = Cursors.SizeNS

    '            Case ResizeDirection.LB
    '                Me.Cursor = Cursors.SizeNESW

    '            Case ResizeDirection.L
    '                Me.Cursor = Cursors.SizeWE

    '            Case ResizeDirection.TL
    '                Me.Cursor = Cursors.SizeNWSE

    '            Case Else
    '                Me.Cursor = Cursors.Default

    '        End Select
    '    End Set
    'End Property

    'Private Sub panel_MainWindow_MouseDown(sender As Object, e As MouseEventArgs) Handles panel_MainWindow.MouseDown, TabControl1.MouseDown, panel_topBar.MouseDown
    '    If e.Button = Windows.Forms.MouseButtons.Left Then
    '        ResizeForm(ResizeCursor)
    '    End If
    'End Sub

    'Private Sub panel_MainWindow_MouseMove(sender As Object, e As MouseEventArgs) Handles panel_MainWindow.MouseMove, sb_Panel.MouseMove, panel_topBar.MouseMove, TabControl1.MouseMove
    '    If e.Location.Y < bordercapture Then
    '        ResizeCursor = ResizeDirection.T

    '    ElseIf e.Location.X > Me.Width - bordercapture And e.Location.Y < bordercapture Then
    '        ResizeCursor = ResizeDirection.TR

    '    ElseIf e.Location.X > Me.Width - bordercapture Then
    '        ResizeCursor = ResizeDirection.R

    '    ElseIf e.Location.X > Me.Width - bordercapture And e.Location.Y > Me.Height - bordercapture Then
    '        ResizeCursor = ResizeDirection.RB

    '    ElseIf e.Location.Y > Me.Height - bordercapture Then
    '        ResizeCursor = ResizeDirection.B

    '    ElseIf e.Location.X < bordercapture And e.Location.Y > Me.Height - bordercapture Then
    '        ResizeCursor = ResizeDirection.LB

    '    ElseIf e.Location.X < bordercapture Then
    '        ResizeCursor = ResizeDirection.L

    '    ElseIf e.Location.X < bordercapture And e.Location.Y < bordercapture Then
    '        ResizeCursor = ResizeDirection.TL

    '    Else
    '        ResizeCursor = ResizeDirection.Normal
    '    End If
    'End Sub




    'Private Sub ResizeForm(ByVal resdirection As ResizeDirection)
    '    Dim d As Integer = -1
    '    Select Case resdirection
    '        Case ResizeDirection.T
    '            d = 12

    '        Case ResizeDirection.TR
    '            d = 14

    '        Case ResizeDirection.R
    '            d = 11

    '        Case ResizeDirection.RB
    '            d = 17

    '        Case ResizeDirection.B
    '            d = 15

    '        Case ResizeDirection.LB
    '            d = 16

    '        Case ResizeDirection.L
    '            d = 10

    '        Case ResizeDirection.TL
    '            d = 13
    '    End Select

    '    If d <> -1 Then
    '        ReleaseCapture()
    '        SendMessage(Me.Handle, &HA1, d, 0)
    '    End If
    'End Sub
#End Region


    Private Sub buttonCompress_EnabledChanged(sender As Object, e As EventArgs) Handles buttonCompress.EnabledChanged
        Dim btn = DirectCast(sender, Button)
        btn.ForeColor = If(btn.Enabled, Color.White, Color.Silver)
    End Sub

    Private Sub buttonCompress_Paint(sender As Object, e As PaintEventArgs) Handles buttonCompress.Paint
        Dim btn = DirectCast(sender, Button)
        Dim drawBrush = New SolidBrush(btn.ForeColor)
        Dim sf = New StringFormat() With {
            .Alignment = StringAlignment.Center,
            .LineAlignment = StringAlignment.Center
        }
        buttonCompress.Text = String.Empty
        e.Graphics.DrawString("Compress Folder", btn.Font, drawBrush, e.ClipRectangle, sf)
        drawBrush.Dispose()
        sf.Dispose()
    End Sub


    Private Sub sb_AnalysisPanel_Paint(sender As Object, e As PaintEventArgs) Handles sb_AnalysisPanel.Paint
        Dim p As New Pen(Brushes.DimGray, 1)
        e.Graphics.DrawLine(p, New Point(30, 0), New Point(303, 0))
    End Sub

    Private Sub sb_ResultsPanel_Paint(sender As Object, e As PaintEventArgs) Handles sb_ResultsPanel.Paint
        Dim p As New Pen(Brushes.DimGray, 1)
        Dim dotted As New Pen(Brushes.ForestGreen, 1)
        dotted.DashPattern = New Single() {3, 3, 3, 3}


        e.Graphics.DrawRectangle(dotted, 225, 5, 39, 112)

    End Sub

    Public Sub results_arc_Paint(sender As Object, e As PaintEventArgs) Handles results_arc.Paint

        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        DrawProgress(e.Graphics, New Rectangle(21, 21, 203, 203), PaintPercentageTransition.callpercentstep, ColorTranslator.FromHtml("#3B668E"), ColorTranslator.FromHtml("#9B9B9B"))

    End Sub


    Shared Callpercent As Single

    Private Sub DrawProgress(g As Graphics, rect As Rectangle, percentage As Single, percColor As Color, remColor As Color)
        'work out the angles for each arc
        Dim progressAngle = CInt(183 / 100 * (percentage))
        Dim remainderAngle = 185 - progressAngle

        'create pens to use for the arcs
        Using progressPen As New Pen(percColor, 41), remainderPen As New Pen(remColor, 41)
            'set the smoothing to high quality for better output
            'draw the blue and white arcs

            g.DrawArc(progressPen, rect, -183, progressAngle)
            g.DrawArc(remainderPen, rect, progressAngle - 184, remainderAngle)
        End Using

        'draw the text in the centre by working out how big it is and adjusting the co-ordinates accordingly
        Dim fb = New SolidBrush(Color.FromArgb(48, 67, 84))
        Using fnt As New Font("Segoe UI Light", 22)

            Dim perc As String = Math.Round(percentage, 0)
            Dim percSize = g.MeasureString(perc, fnt)
            Dim percPoint As New Point(CInt(rect.Left + (rect.Width / 2) - (percSize.Width / 2)), CInt(rect.Top + (rect.Height / 2) - (percSize.Height * 0.85)))
            'now we have all the values draw the text
            g.DrawString(perc, fnt, fb, percPoint)
            Using fnt2 As New Font("Segoe UI Light", 9)
                Dim sign As String = "%"
                Dim signPoint As New Point(percPoint.X + percSize.Width - 5, percPoint.Y + 10)
                'now we have all the values draw the text
                g.DrawString(sign, fnt2, fb, signPoint)
            End Using
            Using fnt3 As New Font("Segoe UI Light", 9)
                Dim lbl As String = "Size Reduction"
                Dim lblSize = g.MeasureString(lbl, fnt3)
                Dim lblPoint As New Point(CInt(rect.Left + (rect.Width / 2) - (lblSize.Width / 2)), CInt(rect.Top + (rect.Height / 2) - (percSize.Height * 1.25)))
                'now we have all the values draw the text
                g.DrawString(lbl, fnt3, fb, lblPoint)
            End Using
        End Using

    End Sub







    Private Sub CompResultsPanel_Paint(sender As Object, e As PaintEventArgs) Handles CompResultsPanel.Paint
        Dim p As New Pen(Brushes.Silver, 1)
        e.Graphics.DrawLine(p, New Point(12, CompResultsPanel.Height - 1), New Point(panel_console.Width - 12, CompResultsPanel.Height - 1))
    End Sub

    Private Sub sb_lblGameIssues_Click(sender As Object, e As EventArgs) Handles sb_lblGameIssues.Click
        Process.Start("https://github.com/ImminentFate/CompactGUI/wiki/Compression-Results:-Games")
    End Sub

    Private Sub dirChooser_DragDrop(sender As Object, e As DragEventArgs) Handles dirChooser.DragDrop, topbar_dirchooserContainer.DragDrop
        Dim dropVar = e.Data.GetData(DataFormats.FileDrop)(0)
        If isActive = 0 And isQueryMode = 0 Then
            sb_AnalysisPanel.Visible = False
            buttonCompress.Visible = True
            overrideCompressFolderButton = 0
            SelectFolder(dropVar, "button")
        End If
    End Sub

    Private Sub dirChooser_DragEnter(sender As Object, e As DragEventArgs) Handles dirChooser.DragEnter, topbar_dirchooserContainer.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim dropVar = e.Data.GetData(DataFormats.FileDrop)(0)
            If System.IO.Directory.Exists(dropVar) Then
                e.Effect = DragDropEffects.Copy
            ElseIf System.IO.File.Exists(dropVar) Then
                'MsgBox("it's a file")
            End If
        End If
    End Sub

    Private Sub dlUpdateLink_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles dlUpdateLink.LinkClicked
        Process.Start("https://github.com/ImminentFate/CompactGUI/releases")
    End Sub

    Private Sub updateBanner_Paint(sender As Object, e As PaintEventArgs) Handles updateBanner.Paint
        Dim p As New Pen(Brushes.DimGray, 1)
        Dim x As Integer = updateBanner.Width
        Dim y As Integer = updateBanner.Height
        e.Graphics.FillPolygon(New SolidBrush(Color.FromArgb(255, 47, 66, 83)), New PointF() {New Point(0, 0), New Point(0, y), New Point(y, y)})
        e.Graphics.FillPolygon(New SolidBrush(Color.FromArgb(255, 47, 66, 83)), New PointF() {New Point(x, 0), New Point(x, y), New Point(x - y, y)})

    End Sub


    Private Const ERR_WINDOWSDIRNOTALLOWED = 515
    Private Const ERR_WHOLEDRIVENOTALLOWED = 516
    Private Const ERR_UNAUTHORISEDREQUIRESADMIN = 517
    Private Const ERR_UNAUTHORISEDREQUIRESSYSTEM = 518

    Private Sub ThrowError(e As Integer)

        Select Case e
            Case ERR_WHOLEDRIVENOTALLOWED
                MsgBox("Compressing an entire drive with this tool is not allowed")

            Case ERR_WINDOWSDIRNOTALLOWED
                MsgBox("Compressing items in the Windows folder using this program " _
                & "is not recommended. Please use the command line instead")

            Case ERR_UNAUTHORISEDREQUIRESADMIN
                If MessageBox.Show("This directory contains a subfolder that you do not have permission to access. Please try running the program again as an Administrator." & vbCrLf & vbCrLf &
                                   "If the problem persists, the subfolder is most likely protected by the System, and by design this program will refuse to let you proceed." & vbCrLf & vbCrLf &
                                   "Would you like to restart the program as an Administrator?", "Permission Error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) = DialogResult.Yes Then
                    RCMenu.RunAsAdmin()
                    Me.Close()
                End If

            Case ERR_UNAUTHORISEDREQUIRESSYSTEM
                MsgBox("This directory contains a subfolder that you do not have permission To access." & vbCrLf & vbCrLf &
                       "The subfolder is most likely protected by the System, and by design this program will refuse to let you proceed.")

        End Select
    End Sub







End Class

Public Class GraphicsPanel
    Inherits Panel
    Private Const WS_EX_COMPOSITED As Integer = &H2000000

    Protected Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_COMPOSITED
            Return cp
        End Get
    End Property

    Public Sub New()
        Me.DoubleBuffered = True
    End Sub
End Class