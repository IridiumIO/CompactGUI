Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Ookii.Dialogs                                                                          'Uses Ookii Dialogs for the non-archaic filebrowser dialog. http://www.ookii.org/Software/Dialogs
Imports System.Management


Public Class Compact
    Public Shared version = "2.4.0"
    Private WithEvents MyProcess As Process
    Private Delegate Sub AppendOutputTextDelegate(ByVal text As String)



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
    Dim compressfinished As Boolean = False
    Dim uncompressfinished As Boolean = False
    Dim isQueryMode As Boolean = False
    Dim isQueryCalledByCompact As Boolean = False
    Dim isActive As Boolean = False
    Dim dirCountProgress As Int64
    Dim dirCountTotal As Int64
    Dim fileCountTotal As Int64 = 0
    Dim fileCountProgress As Int64
    Dim fileCountOutputCompressed As Int64
    Dim QdirCountProgress As Int64




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
        If FMTListing(0) = CONINPUTDATA(0) Then QdirCountProgress += 1

        'OK - FILE COUNT
        If edata.EndsWith(fixedfmt1) Then fileCountProgress += 1

        'Uncompressed - Checks if uncompression is finished
        If FMTUncompressed.Count = CONINPUTDATA.Count _
            And (FMTUncompressed(FMTUncompressed.Count - 1) = CONINPUTDATA(CONINPUTDATA.Count - 1) _
                Or FMTUncompressed(FMTUncompressed.Count - 1).Contains("%2")) Then
            dirCountProgress = 0
            fileCountProgress = fileCountTotal
            uncompressfinished = True
            isActive = False
        End If

        'Compress Finished Ratio - Checks if compression is finished
        If FMTCompressFinished.Count = CONINPUTDATA.Count And OutputlineIndex = 0 _
            And (FMTCompressFinished(FMTCompressFinished.Count - 1) = CONINPUTDATA(CONINPUTDATA.Count - 1) _
                Or CONINPUTDATA(CONINPUTDATA.Count - 1).Contains("1.")) Then
            compressfinished = True
            dirCountProgress = dirCountTotal
            fileCountProgress = fileCountTotal
            isActive = False
        End If

        'Analysis Complete - Gets the lines when analysing a folder is completed
        If FMTFilesWithin.Count = CONINPUTDATA.Count And (FMTFilesWithin(0) = CONINPUTDATA(0) Or FMTFilesWithin(0).Contains("%1")) Then _
            Return OutputLines(FMTFilesWithin, CONINPUTDATA, CON_INDEX_TOTALFILES, CON_INDEX_TOTALDIRECTORIES, ARR_FILESWITHINDIRECTORIES, "%1", "%2")

        If OutputlineIndex = 1 Then _
            Return OutputLines(FMTCompNotComp, CONINPUTDATA, CON_INDEX_FILESCOMPRESSEDCOUNT, CON_INDEX_FILESNOTCOMPRESSEDCOUNT, ARR_FILESCOMPRESSED, "%3", "%4")

        If OutputlineIndex = 2 Then _
            Return OutputLines(FMTTotalBytes, CONINPUTDATA, CON_INDEX_TOTALBYTESNOTCOMPRESSED, CON_INDEX_TOTALBYTESCOMPRESSED, ARR_TOTALBYTES, "%5", "%6")

        If OutputlineIndex = 3 Then _
            Return OutputLines(FMTCompRatio, CONINPUTDATA, CON_INDEX_COMPRESSIONRATIO, CON_INDEX_COMPRESSIONRATIO, ARR_COMPRATIO, "%7")



        Return ("Nothing")

    End Function


    Public Function OutputLines(ByRef FMTVal As String(), ByRef CONVal As String(), ByRef CON_Index1 As Integer, ByRef CON_Index2 As Integer,
                                ByRef ARRVal As String(), ByRef Val1 As String, Optional ByRef Val2 As String = "%xnull")
        Dim i = 0, b = 0
        For Each c In FMTVal
            If c.Contains(Val1) Then
                FMTVal(i) = CONVal(i)
                CON_Index1 = i
            ElseIf val2 <> "%xnull" And c.Contains(Val2) Then
                FMTVal(i) = CONVal(i)
                CON_Index2 = i
            End If
            i += 1
        Next

        Dim builder As New StringBuilder
        For Each c In FMTVal
            builder.Append(FMTVal(b) & " ")     'Removed separate builder.Append(" ")
            b += 1
        Next
        ARRVal = FMTVal
        Return builder.ToString

    End Function




    Dim intervaltime As Double
    Dim outputbuffer As New ArrayList
    Private Sub MyProcess_OutputDataReceived(ByVal sender As Object, ByVal e As DataReceivedEventArgs) Handles MyProcess.OutputDataReceived

        outputbuffer.Add(e.Data)

        If Math.Round(intervaltime + 0.05, 2) < Math.Round(Date.Now.TimeOfDay.TotalSeconds, 2) Then      'Buffers incoming strings, then outputs them to the listbox every 0.1s
            Invoke(Sub()
                       conOut.BeginUpdate()
                       For Each str As String In outputbuffer
                           AppendOutputText(vbCrLf & str)
                       Next
                       Console.WriteLine("Post: " & outputbuffer.Count)
                       outputbuffer.Clear()
                       intervaltime = Date.Now.TimeOfDay.TotalSeconds
                       conOut.EndUpdate()
                   End Sub)
        End If


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

            compressfinished = True
            dirCountProgress = dirCountTotal
            fileCountProgress = fileCountTotal
            isActive = False

            For Each str As String In outputbuffer
                AppendOutputText(vbCrLf & str)
            Next
            outputbuffer.Clear()

            canProceed = 0
            OutputlineIndex = 0
        End If


        If canProceed = 1 Then
            OutputlineIndex += 1
        End If

    End Sub




    Private Sub MyProcess_ErrorDataReceived(ByVal sender As Object, ByVal e As DataReceivedEventArgs) Handles MyProcess.ErrorDataReceived
        AppendOutputText(vbCrLf & e.Data)
    End Sub




    Private Sub Compact_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        loadFromSettings()

        If dirChooser.Text = "❯   Select Target Folder" Then
            panel_topBar.Height = Height - 1
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

        comboChooseShutdown.SelectedItem = comboChooseShutdown.Items.Item(0)

        RCMenu.WriteLocRegistry()
        VersionCheck.VC(version)
        progressTimer.Start()                                                                   'Starts a timer that keeps track of changes during any operation.

        For Each arg In My.Application.CommandLineArgs
            If Directory.Exists(arg) Then SelectFolder(arg, "cmdlineargs")
        Next

    End Sub




    Private Sub ProgressTimer_Tick(sender As Object, e As EventArgs) Handles progressTimer.Tick

        If fileCountTotal <> 0 Then
            Try

                If sb_progressbar.Width > 301 Then
                    sb_progressbar.Width = 301
                    progresspercent.Text = "100 %"

                ElseIf Not isQueryMode Then
                    sb_progressbar.Width = Math.Round((fileCountProgress / fileCountTotal * 301), 0)
                    progresspercent.Text = Math.Round((fileCountProgress / fileCountTotal * 100), 0).ToString + " %"                 'Generates an estimate of progress based on how many files have been processed out of the total. 

                ElseIf isQueryMode Then
                    sb_progressbar.Width = Math.Round((QdirCountProgress / dirCountTotal * 301), 0)
                    progresspercent.Text = Math.Round((QdirCountProgress / dirCountTotal * 100), 0).ToString + " %"                  'Generates an estimate of progress for the Query command.
                End If

            Catch ex As Exception
                Console.WriteLine("PE: " + ex.Data.ToString)
            End Try
        End If

        If compressfinished = True Then : compressfinished = False                                                                       'Hides and shows certain UI elements when compression is finished or if a compression status is being checked

            If isQueryMode And Not isQueryCalledByCompact Then
                sb_progresslabel.Text = "This folder contains compressed items"
                progresspercent.Visible = False
                conOut.Refresh()
            End If

            If checkShutdownOnCompletion.Checked And Not isQueryMode Then
                ShutdownDialog.SDProcIntent.Text = comboChooseShutdown.Text
                FadeTransition.FadeForm(ShutdownDialog, 0, 0.98, 300, True)
            End If

            buttonRevert.Visible = True
            returnArrow.Visible = True
            CalculateSaving()
            QdirCountProgress = 0
            buttonQueryCompact.Enabled = True

        End If

        If uncompressfinished = True Then : uncompressfinished = False                                                                  'Hides and shows certain UI elements when uncompression is finished 

            buttonRevert.Visible = False
            sb_progresslabel.Text = "Folder Uncompressed."
            sb_compressedSizeVisual.Height = 113
            wkPostSizeVal.Text = "?"
            wkPostSizeUnit.Text = ""
            sb_labelCompressed.Text = "Estimated Compressed"
            sb_ResultsPanel.Visible = False
            returnArrow.Visible = True
            buttonQueryCompact.Enabled = True


            If checkShutdownOnCompletion.Checked = True Then
                ShutdownDialog.SDProcIntent.Text = comboChooseShutdown.Text
                FadeTransition.FadeForm(ShutdownDialog, 0, 0.98, 200, True)
            End If
        End If

    End Sub




    Private Sub AppendOutputText(ByVal text As String)                                           'Attach output to the embedded console
        Invoke(Sub()
                   If text <> vbCrLf Then conOut.Items.Insert(0, text)
               End Sub)
    End Sub



    'Set variables for minor security and error handling
    Dim overrideCompressFolderButton = 0
    Dim directorysizeexceptionCount = 0                                                         'Used in the DirectorySize() Function to ensure the error message only shows up once, even if multiple UnauthorizedAccessException errors get thrown




    Dim uncompressedfoldersize
    Dim workingDir As String = ""




    Private Sub SelectFolderToCompress(sender As Object, e As EventArgs) Handles dirChooser.LinkClicked, dirChooser.Click
        If isActive = False And isQueryMode = False Then

            Dim folderChoice As New FileFolderDialog
            folderChoice.ShowDialog()
            If Directory.Exists(folderChoice.SelectedPath) Then
                SelectFolder(folderChoice.SelectedPath, "button")
            ElseIf File.Exists(folderChoice.SelectedPath) Then
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
        sb_AnalysisPanel.Visible = False
        buttonCompress.Visible = True
        overrideCompressFolderButton = 0

        If selectedDir.Contains("C:\Windows") Then : ThrowError(ERR_WINDOWSDIRNOTALLOWED)                                    'Makes sure you're not trying to compact the Windows directory. I should Regex this to catch all possible drives hey?
        ElseIf selectedDir.EndsWith(":\") Then : ThrowError(ERR_WHOLEDRIVENOTALLOWED)
        Else
            If selectedDir.Length >= 3 Then                                                                                    'Makes sure the chosen folder isn't a null value or an exception
                workingDir = selectedDir
                Dim DI_selectedDir = New DirectoryInfo(selectedDir)
                directorysizeexceptionCount = 0

                If DI_selectedDir.Name.Length > 0 Then _
                    sb_FolderName.Text = StrConv(DI_selectedDir.Name, VbStrConv.ProperCase)

                Try
                    If DI_selectedDir.Parent.Parent.Name <> Nothing Then
                        dirChooser.Text = "❯ " + DI_selectedDir.Parent.Parent.Name +
                            " ❯ " + DI_selectedDir.Parent.Name +
                            " ❯ " + DI_selectedDir.Name
                    Else
                        dirChooser.Text = "❯ " + DI_selectedDir.Root.Name.Replace(":\", " ❯ ") +
                             +DI_selectedDir.Parent.Name + " ❯ " + DI_selectedDir.Name
                    End If

                Catch ex As Exception
                    dirChooser.Text = "❯ " + DI_selectedDir.Root.Name.Replace(":\", " ❯ ") + DI_selectedDir.Name
                    Console.WriteLine("Folder error: " + ex.Data.ToString)
                End Try

                uncompressedfoldersize = Math.Round(DirectorySize(DI_selectedDir, True), 1)

                Dim rawpreSize = GetOutputSize(uncompressedfoldersize, True)

                preSize.Text = "Uncompressed Size: " + rawpreSize

                dirLabelResults = DI_selectedDir.Name

                Try
                    dirCountTotal = Directory.GetDirectories(selectedDir, "*", SearchOption.AllDirectories).Length + 1

                    fileCountTotal = Directory.GetFiles(selectedDir, "*", SearchOption.AllDirectories).Length

                    sb_ResultsPanel.Visible = False

                    UnfurlTransition.UnfurlControl(topbar_dirchooserContainer, topbar_dirchooserContainer.Width, Me.Width - sb_Panel.Width - 46, 100)
                    WikiHandler.localFolderParse(selectedDir, DI_selectedDir, rawpreSize)

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
                    isQueryCalledByCompact = False
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




    Private Sub CompressFolder_Click(sender As System.Object, e As System.EventArgs) Handles buttonCompress.Click
        conOut.Items.Clear()
        CreateProcess("compact")
        sb_AnalysisPanel.Visible = True
        buttonCompress.Visible = False
        buttonQueryCompact.Enabled = False
    End Sub
    Private Sub buttonQueryCompact_Click(sender As Object, e As EventArgs) Handles buttonQueryCompact.Click
        conOut.Items.Clear()
        CreateProcess("query")
        buttonCompress.Visible = False
        sb_Panel.Show()                                 'Temporary Fix - go back and work out why #124 doesn't work with WikiParser()
        sb_AnalysisPanel.Visible = True
        buttonQueryCompact.Enabled = False
    End Sub








    Private Sub ButtonRevert_Click(sender As Object, e As EventArgs) Handles buttonRevert.Click             'Handles uncompressing. For now, uncompressing can only be done through the program only to revert a compression that's just been done.
        isQueryMode = False
        fileCountProgress = 0
        dirCountProgress = 0
        progresspercent.Visible = True
        CompResultsPanel.Visible = False
        buttonQueryCompact.Enabled = False
        buttonRevert.Visible = False

        Try
            RunCompact("uncompact")
        Catch ex As Exception
        End Try

    End Sub




    Private Sub ReturnArrow_Click(sender As Object, e As EventArgs) Handles returnArrow.Click                       'Returns you to the first screen and cleans up some stuff

        returnArrow.Visible = False
        buttonRevert.Visible = False
        CompResultsPanel.Visible = False
        checkShutdownOnCompletion.Checked = False
        TabControl1.SelectedTab = InputPage
        dirCountProgress = 0
        fileCountProgress = 0
        isQueryCalledByCompact = False
        sb_AnalysisPanel.Visible = False
        buttonCompress.Visible = True
        buttonQueryCompact.Enabled = True

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
        If isActive = True Then

            If MessageBox.Show _
                ("Are you sure you want to exit?" & vbCrLf & vbCrLf & "Quitting now will finish compressing the current file, then quit safely." _
                 & vbCrLf & vbCrLf &
                 "If you do decide to quit now, you can resume compression by repeating the process later." _
                 & vbCrLf & "Click Yes to continue exiting the program.",
                 "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then

                e.Cancel = True

            Else
                Try
                    If MyProcess.HasExited = False Then MyProcess.Kill()
                Catch ex As Exception
                End Try
            End If
        Else
            Try
                If MyProcess.HasExited = False Then MyProcess.Kill()
            Catch ex As Exception
            End Try
        End If

    End Sub




    '/////////////FUNCTIONS//////////////

    Private Sub CalculateSaving()   'Calculations for all the relevant information after compression is completed. All the data is parsed from the console ouput using basic strings, but because that occurs on a different thread, information is stored to variables first (The Status Monitors at the top) then those values are used. 

        Dim numberFilesCompressed = 0
        Dim querySize As Int64 = 0

        Dim oldFolderSize As Long = uncompressedfoldersize
        Dim newFolderSize As Long = uncompressedfoldersize
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

        If GetOutputSize((oldFolderSize - newFolderSize), False) = "0" And isQueryMode = True Then

            sb_progresslabel.Text = "Folder is not compressed"
            buttonRevert.Visible = False
            isQueryCalledByCompact = False

        Else

            sb_progresslabel.Text = "Folder is compressed"

            If isQueryMode = True And isQueryCalledByCompact = False Then
                origSizeLabel.Text = GetOutputSize(oldFolderSize, True)
            Else
                origSizeLabel.Text = GetOutputSize(uncompressedfoldersize, True)
            End If

            Dim PrintOutSize = GetOutputSize(uncompressedfoldersize - (oldFolderSize - newFolderSize), True)

            compressedSizeLabel.Text = PrintOutSize
            wkPostSizeVal.Text = PrintOutSize.Split(" ")(0)
            wkPostSizeUnit.Text = PrintOutSize.Split(" ")(1)

            Dim wkPostSizeVal_Len = TextRenderer.MeasureText(wkPostSizeVal.Text, wkPostSizeVal.Font)
            wkPostSizeUnit.Location = New Point(wkPostSizeVal.Location.X + (wkPostSizeVal.Size.Width / 2) + (wkPostSizeVal_Len.Width / 2 - 8), wkPostSizeVal.Location.Y + 16)
            sb_labelCompressed.Text = "Compressed"

            Try
                compRatioLabel.Text = Math.Round(oldFolderSize / newFolderSize, 1)
                compRatioLabel.Text = Math.Round(uncompressedfoldersize / (uncompressedfoldersize - (oldFolderSize - newFolderSize)), 1)
            Catch ex As DivideByZeroException
                MsgBox("Stop trying to compress empty folders")
            End Try


            spaceSavedLabel.Text = GetOutputSize((oldFolderSize - newFolderSize), True) + " Saved"
            sb_SpaceSavedLabel.Text = spaceSavedLabel.Text

            labelFilesCompressed.Text =
                numberFilesCompressed.ToString + " / " + fileCountTotal.ToString + " files compressed"
            help_resultsFilesCompressed.Location =
                New Point(labelFilesCompressed.Location.X + labelFilesCompressed.Width + 2, labelFilesCompressed.Location.Y + 1)
            Try

                compressedSizeVisual.Width = CInt(320 / compRatioLabel.Text)
                sb_compressedSizeVisual.Height = CInt(113 / compRatioLabel.Text)
                sb_compressedSizeVisual.Location = New Point(sb_compressedSizeVisual.Location.X, 5 + 113 - sb_compressedSizeVisual.Height)


                If hasqueryfinished = 1 Then
                    isQueryCalledByCompact = False
                    isQueryMode = False
                    buttonRevert.Visible = True
                    Callpercent = (1 - (1 / CDec(ARR_COMPRATIO(CON_INDEX_COMPRESSIONRATIO)))) * 100
                    If My.Settings.ShowNotifications Then _
                        TrayIcon.ShowBalloonTip(1, "Compressed: " & StrConv(sb_FolderName.Text, VbStrConv.ProperCase), vbCrLf & "▸ " & spaceSavedLabel.Text & vbCrLf & "▸ " & Math.Round(Callpercent, 1) & "% Smaller", ToolTipIcon.None)
                End If

            Catch ex As OverflowException
                compressedSizeVisual.Width = 320
                sb_compressedSizeVisual.Height = 113
            End Try

            If isQueryCalledByCompact = False Then

                CompResultsPanel.Visible = True
                sb_ResultsPanel.Visible = True
                Callpercent = (1 - (1 / CDec(ARR_COMPRATIO(CON_INDEX_COMPRESSIONRATIO)))) * 100
                PaintPercentageTransition.PaintTarget(results_arc, Callpercent, 5)
            ElseIf isQueryCalledByCompact = True Then

                buttonRevert.Visible = False
                CompResultsPanel.Visible = False
                sb_ResultsPanel.Visible = False

            End If

        End If

        If isQueryCalledByCompact = True Then Queryaftercompact()
        If isQueryCalledByCompact = False Then isQueryMode = False

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



    Private Function DirectorySize(ByVal dInfo As DirectoryInfo, ByVal includeSubdirectories As Boolean) As Long

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

        If posA = -1 Then Return ""
        If posB = -1 Then Return ""

        Dim adjustedPosA As Integer = posA + a.Length
        If adjustedPosA >= posB Then Return ""

        Return value.Substring(adjustedPosA, posB - adjustedPosA)

    End Function


    Function Before(value As String, a As String) As String
        Dim posA As Integer = value.IndexOf(a)
        If posA = -1 Then Return ""

        Return value.Substring(0, posA)
    End Function


    Function After(value As String, a As String) As String
        Dim posA As Integer = value.LastIndexOf(a)
        If posA = -1 Then Return ""
        Dim adjustedPosA As Integer = posA + a.Length
        If adjustedPosA >= value.Length Then Return ""
        Return value.Substring(adjustedPosA)
    End Function



    '////////////////////TESTING////////////////////



    Private Sub Saveconlog_Click(sender As Object, e As EventArgs) Handles saveconlog.Click
        If MsgBox("Save console output?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Dim reverseCon As New ArrayList
            Dim sb As New StringBuilder()

            For Each ln As String In conOut.Items
                reverseCon.Add(ln.Trim())
            Next

            reverseCon.Reverse()
            sb.AppendLine("CompactGUI: Log at " & System.DateTime.Now & vbCrLf & StrDup(50, "/"))

            For Each ln As String In reverseCon
                sb.AppendLine(ln)
            Next

            sb.AppendLine("End Log at " & System.DateTime.Now & vbCrLf & StrDup(100, "/") & vbCrLf & vbCrLf)

            File.AppendAllText(Application.StartupPath & "\CompactGUILog.txt", sb.ToString, CP)

            MsgBox("Saved log to " & Application.StartupPath & "\CompactGUILog.txt")
            Process.Start(Application.StartupPath & "\CompactGUILog.txt")
        End If
    End Sub




    Private Sub submitToWiki_Click(sender As Object, e As EventArgs) Handles submitToWiki.Click
        Process.Start("https://goo.gl/forms/Udi5SUkMdCOMG3m23")
    End Sub




    Private Sub compressX8_CheckedChanged(sender As Object, e As EventArgs) Handles compressX4.Click, compressX8.Click, compressX16.Click, compressLZX.Click

        If compressX4.Checked Then My.Settings.SavedCompressionOption = 0
        If compressX8.Checked Then My.Settings.SavedCompressionOption = 1
        If compressX16.Checked Then My.Settings.SavedCompressionOption = 2
        If compressLZX.Checked Then My.Settings.SavedCompressionOption = 3

    End Sub

    Private Sub loadFromSettings()

        Select Case My.Settings.SavedCompressionOption
            Case 0 : compressX4.Checked = True
            Case 1 : compressX8.Checked = True
            Case 2 : compressX16.Checked = True
            Case 3 : compressLZX.Checked = True
        End Select

    End Sub

    Private Sub btn_Mainexit_Click(sender As Object, e As EventArgs) Handles btn_Mainexit.Click
        Close()
    End Sub

    Private Sub btn_Mainmin_Click(sender As Object, e As EventArgs) Handles btn_Mainmin.Click
        WindowState = FormWindowState.Minimized
        If My.Settings.MinimisetoTray = True Then Hide()
    End Sub


    Dim isMaximised As Boolean = False
    Private Sub btn_Mainmax_Click(sender As Object, e As EventArgs) Handles btn_Mainmax.Click
        If isMaximised = False Then
            MaximumSize = Screen.FromControl(Me).WorkingArea.Size
            Bounds = Screen.GetWorkingArea(Me)
            isMaximised = True
        ElseIf isMaximised = True Then
            Me.Height = 652
            Me.Width = 1002
            Me.CenterToScreen()
            isMaximised = False
        End If

    End Sub


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

    Private Sub panel_topBar_MouseDown(sender As Object, e As MouseEventArgs) Handles panel_topBar.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Left And isMaximised = False Then MoveForm()
    End Sub
#End Region




#Region "Paint Events and Other Visuals"

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
        Using dotted As New Pen(Brushes.ForestGreen, 1)
            dotted.DashPattern = New Single() {3, 3, 3, 3}
            e.Graphics.DrawRectangle(dotted, 225, 5, 39, 112)
        End Using
    End Sub



    Shared Callpercent As Single
    Public Sub results_arc_Paint(sender As Object, e As PaintEventArgs) Handles results_arc.Paint
        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        DrawProgress(e.Graphics, New Rectangle(21, 21, 203, 203),
                     PaintPercentageTransition.callpercentstep,
                     ColorTranslator.FromHtml("#3B668E"),
                     ColorTranslator.FromHtml("#CFD8DC"))
    End Sub




    Private Sub DrawProgress(g As Graphics, rect As Rectangle, percentage As Single, percColor As Color, remColor As Color)

        Dim progressAngle As Single = (183 / 100 * (percentage))
        Dim remainderAngle As Single = (185 * percentage / Callpercent) - progressAngle

        Using progressPen As New Pen(percColor, 41), remainderPen As New Pen(remColor, 41)
            g.DrawArc(remainderPen, rect, progressAngle - 184, remainderAngle)
            g.DrawArc(progressPen, rect, -183, progressAngle)
        End Using

        Using fnt As New Font("Segoe UI Light", 22), fb As New SolidBrush(Color.FromArgb(48, 67, 84))

            Dim perc As String = Math.Round(percentage, 0)
            Dim percSize = g.MeasureString(perc, fnt)
            Dim percPoint As New Point(rect.Left + (rect.Width / 2) - (percSize.Width / 2), rect.Top + (rect.Height / 2) - (percSize.Height * 1.25))
            g.DrawString(perc, fnt, fb, percPoint)

            Using fnt2 As New Font("Segoe UI Light", 9)
                Dim sign As String = "%"
                Dim signPoint As New Point(percPoint.X + percSize.Width - 5, percPoint.Y + 10)
                g.DrawString(sign, fnt2, fb, signPoint)
            End Using

            Using fnt3 As New Font("Segoe UI Light", 10)
                Dim lbl As String = "Reduction in size"
                Dim lblSize = g.MeasureString(lbl, fnt3)
                Dim lblPoint As New Point(rect.Left + (rect.Width / 2) - (lblSize.Width / 2), rect.Top + (rect.Height / 2) - (lblSize.Height * 0.75))
                g.DrawString(lbl, fnt3, fb, lblPoint)
            End Using
        End Using
    End Sub




    Private Sub CompResultsPanel_Paint(sender As Object, e As PaintEventArgs) Handles CompResultsPanel.Paint
        Dim p As New Pen(Brushes.Silver, 1)
        e.Graphics.DrawLine(p, New Point(12, CompResultsPanel.Height - 1), New Point(panel_console.Width - 12, CompResultsPanel.Height - 1))
    End Sub




    Private Sub updateBanner_Paint(sender As Object, e As PaintEventArgs) Handles updateBanner.Paint
        Dim p As New Pen(Brushes.DimGray, 1)
        Dim x As Integer = updateBanner.Width
        Dim y As Integer = updateBanner.Height
        e.Graphics.FillPolygon(New SolidBrush(Color.FromArgb(255, 47, 66, 83)), New PointF() {New Point(0, 0), New Point(0, y), New Point(y, y)})
        e.Graphics.FillPolygon(New SolidBrush(Color.FromArgb(255, 47, 66, 83)), New PointF() {New Point(x, 0), New Point(x, y), New Point(x - y, y)})

    End Sub



    Private Sub dirChooser_MouseEnter(sender As Object, e As EventArgs) Handles dirChooser.MouseEnter
        dirChooser.LinkColor = Color.FromArgb(200, 200, 200)
    End Sub
    Private Sub dirChooser_MouseLeave(sender As Object, e As EventArgs) Handles dirChooser.MouseLeave
        dirChooser.LinkColor = Color.White
    End Sub




    Dim isAlreadyFading = 2
    Private Sub seecompest_MouseHover(sender As Object, e As EventArgs) Handles seecompest.MouseHover, sb_labelCompressed.MouseHover
        topbar_title.Select()
        WikiHandler.showWikiRes()
        isAlreadyFading = 0
    End Sub
    Private Sub hideWikiRes(sender As Object, e As EventArgs) Handles seecompest.MouseLeave, sb_labelCompressed.MouseLeave
        If isAlreadyFading = 0 Then
            FadeTransition.FadeForm(WikiPopup, 0.96, 0, 200)
            isAlreadyFading = 1
        End If
    End Sub




    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles comboChooseShutdown.SelectedIndexChanged
        InputPage.Focus()
    End Sub

    Private Sub ComboBox1_MouseLeave(sender As Object, e As EventArgs) Handles comboChooseShutdown.MouseLeave
        InputPage.Focus()
    End Sub




    Private Sub buttonCompress_EnabledChanged(sender As Object, e As EventArgs) Handles buttonCompress.EnabledChanged
        Dim btn = DirectCast(sender, Button)
        btn.ForeColor = If(btn.Enabled, Color.White, Color.Silver)
    End Sub





#End Region

    Private Sub ViewReset()
        sb_AnalysisPanel.Visible = False
        buttonCompress.Visible = True
        overrideCompressFolderButton = 0
    End Sub





    Private Sub sb_lblGameIssues_Click(sender As Object, e As EventArgs) Handles sb_lblGameIssues.Click
        Process.Start("https://github.com/ImminentFate/CompactGUI/wiki/Compression-Results:-Games")
    End Sub

    Private Sub dirChooser_DragDrop(sender As Object, e As DragEventArgs) Handles dirChooser.DragDrop, topbar_dirchooserContainer.DragDrop
        Dim dropVar = e.Data.GetData(DataFormats.FileDrop)(0)

        If isActive = False And isQueryMode = False _
            Then SelectFolder(dropVar, "button")

    End Sub

    Private Sub dirChooser_DragEnter(sender As Object, e As DragEventArgs) Handles dirChooser.DragEnter, topbar_dirchooserContainer.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim dropVar = e.Data.GetData(DataFormats.FileDrop)(0)
            If Directory.Exists(dropVar) Then
                e.Effect = DragDropEffects.Copy
            ElseIf File.Exists(dropVar) Then
                'For use in file handling
            End If
        End If
    End Sub

    Private Sub dlUpdateLink_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles dlUpdateLink.LinkClicked
        Process.Start("https://github.com/ImminentFate/CompactGUI/releases")
    End Sub




    Private Sub ShowInfoPopup_Click(sender As Object, e As EventArgs) Handles showinfopopup.Click
        Info.Show()
    End Sub



#Region "Error Handling"
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

#End Region


    Private Sub TrayIcon_BalloonTipClicked(sender As Object, e As EventArgs) Handles TrayIcon.BalloonTipClicked, Tray_ShowMain.Click, TrayIcon.DoubleClick
        Show() : TopMost = True : Focus() : WindowState = FormWindowState.Normal : TopMost = False
    End Sub





End Class

