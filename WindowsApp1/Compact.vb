Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Ookii.Dialogs                                                                          'Uses Ookii Dialogs for the non-archaic filebrowser dialog. http://www.ookii.org/Software/Dialogs
Imports System.Management

Public Class Compact
    Dim version = "1.4.0_rc3"
    Private WithEvents MyProcess As Process
    Private Delegate Sub AppendOutputTextDelegate(ByVal text As String)


    Private Shared Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(CompactGUI.Compact)
    End Sub






    'Status Monitors
    Dim compressFinished = 0
    Dim uncompressFinished = 0
    Dim isQueryMode = 0
    Dim isQueryCalledByCompact = 0
    Dim isActive = 0
    Dim byteComparisonRaw As String = ""
    Dim byteComparisonRawFilesCompressed As String = ""
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
            Console.WriteLine(edata)
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

        AppendOutputText(vbCrLf & e.Data)


        If MyProcess.HasExited = False Then
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
            byteComparisonRawFilesCompressed = e.Data
            Console.WriteLine("Compressed: " +
              ARR_FILESCOMPRESSED(CON_INDEX_FILESCOMPRESSEDCOUNT) + " Not Compressed: " +
              ARR_FILESCOMPRESSED(CON_INDEX_FILESNOTCOMPRESSEDCOUNT))
        End If


        If OutputlineIndex = 2 Then
            CON_TOTALBYTESLINE = e.Data
            CALC_OUTPUT(e.Data)
            byteComparisonRaw = e.Data
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


    Private Sub MyProcess_OutputDataReceived1 _
        (ByVal sender As Object, ByVal e As System.Diagnostics.DataReceivedEventArgs)


        AppendOutputText(vbCrLf & e.Data)                                                       'Sends output to the embedded console

        Try

            If e.Data.Contains("total bytes of data are stored in") Then                        'Gets the output line that contains both the pre- and post-compression folder sizes
                byteComparisonRaw = e.Data
            End If

            If e.Data.EndsWith("1.") Then                                 'Gets the output line that contains the compression ratio and forces the progress bar to 100% (indirectly due to threading)
                compressFinished = 1
                dirCountProgress = dirCountTotal
                fileCountProgress = fileCountTotal
                isActive = 0
            End If

            If e.Data.Contains("directories were uncompressed") Then                            'Gets the output line that identifies that an uncompression event has finished.    
                dirCountProgress = 0
                fileCountProgress = fileCountTotal
                uncompressFinished = 1
                isActive = 0
            End If

            If e.Data.StartsWith(" Compressing files in") Then                                  'Gets each directory that is compressed. Used for the old progressbar.   
                dirCountProgress += 1
            End If

            If e.Data.EndsWith("[OK]") Then                                                     'Gets each file that was successfully compressed OR uncompressed. 
                fileCountProgress += 1
            End If

            If e.Data.EndsWith(" are not compressed.") Then                                     'Gets the output line that identifies the total number of files compressed. 
                byteComparisonRawFilesCompressed = e.Data
            End If
            If e.Data.StartsWith(" Listing ") Then                                               'Gets the output line that identifies the query folder count
                QdirCountProgress += 1
            End If
        Catch ex As Exception

        End Try

    End Sub




    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        With dirChooser
            .LinkBehavior = LinkBehavior.HoverUnderline

            .LinkColor = Color.FromArgb(37, 110, 196)
        End With

        RCMenu.WriteLocRegistry()

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

        If fileCountTotal <> 0 Then                                                             'Makes sure that there are actually files being counted before attempting a calculation

            If isQueryMode = 0 Then

                progresspercent.Text = Math.Round _
                ((fileCountProgress / fileCountTotal * 100), 0).ToString + " %"                 'Generates an estimate of progress based on how many files have been processed out of the total. 

                Try
                    If compactprogressbar.Value >= 101 Then                                         'Avoids a /r/softwaregore scenario
                        compactprogressbar.Value = 1
                    Else
                        compactprogressbar.Value = Math.Round _
                            ((fileCountProgress / fileCountTotal * 100), 0)
                    End If
                Catch ex As Exception
                End Try

            ElseIf isQueryMode = 1 Then

                progresspercent.Text = Math.Round _
                ((QdirCountProgress / dirCountTotal * 100), 0).ToString + " %"                 'Generates an estimate of progress for the Query command.

                Try
                    If compactprogressbar.Value >= 101 Then                                         'Avoids a /r/softwaregore scenario
                        compactprogressbar.Value = 1
                    Else
                        compactprogressbar.Value = Math.Round _
                            ((QdirCountProgress / dirCountTotal * 100), 0)
                    End If
                Catch ex As Exception
                End Try

            End If


        End If


        If compressFinished = 1 Then                                                            'Hides and shows certain UI elements when compression is finished or if a compression status is being checked

            If isQueryMode And isQueryCalledByCompact = 0 Then
                progressPageLabel.Text = "This folder contains compressed items"
                progresspercent.Visible = False


            End If

            compressFinished = 0

            buttonRevert.Visible = True
            returnArrow.Visible = True
            CalculateSaving()
            QdirCountProgress = 0
        End If

        If uncompressFinished = 1 Then                                                          'Hides and shows certain UI elements when uncompression is finished 

            uncompressFinished = 0
            buttonCompress.Visible = True
            buttonRevert.Visible = False
            progressPageLabel.Text = "Folder Uncompressed."
            returnArrow.Visible = True


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




    'Set variables for the advanced compression checkboxes. 
    Dim workingDir As String = ""
    Dim recursiveScan As String = ""
    Dim hiddenFiles As String = ""
    Dim forceCompression As String = ""                                                         'Not actually implemented - yet


    'Set variables for minor security and error handling
    Dim overrideCompressFolderButton = 0
    Dim directorysizeexceptionCount = 0                                                         'Used in the DirectorySize() Function to ensure the error message only shows up once, even if multiple UnauthorizedAccessException errors get thrown


    Dim uncompressedfoldersize


    Private Sub SelectFolderToCompress _
        (sender As Object, e As EventArgs) Handles dirChooser.LinkClicked, chosenDirDisplay.Click

        overrideCompressFolderButton = 0

        Dim folderChoice As New VistaFolderBrowserDialog

        folderChoice.ShowDialog()

        SelectFolder(folderChoice.SelectedPath, "button")

        folderChoice.Dispose()

    End Sub

    Dim dirLabelResults As String = ""

    Private Sub SelectFolder(selectedDir As String, senderID As String)
        Dim wDString = selectedDir

        If wDString.Contains("C:\Windows") Then                                                 'Makes sure you're not trying to compact the Windows directory. I should Regex this to catch all possible drives hey?

            MsgBox("Compressing items in the Windows folder using this program " _
                    & "is not recommended. Please use the command line if you still " _
                    & "wish to compress the Windows folder")

        ElseIf wDString.EndsWith(":\") Then

            MsgBox("Compressing an entire drive with this tool is not allowed")

        Else

            If wDString.Length >= 3 Then                                                        'Makes sure the chosen folder isn't a null value or an exception


                Dim DIwDString = New DirectoryInfo(wDString)
                directorysizeexceptionCount = 0
                workingDir = wDString.ToString()
                chosenDirDisplay.Text = DIwDString.Parent.ToString + " ❯ " + DIwDString.Name.ToString
                uncompressedfoldersize = Math.Round(DirectorySize(DIwDString, True), 0)
                preSize.Text = "Uncompressed Size: " + GetOutputSize _
                    (Math.Round(DirectorySize(DIwDString, True), 0), True)

                dirLabelResults = DIwDString.Name.ToString

                preSize.Visible = True
                buttonQueryCompact.Visible = True

                Try
                    Dim directories() As String = System.IO.Directory.GetDirectories _
                        (wDString, "*", IO.SearchOption.AllDirectories)

                    dirCountTotal = directories.Length + 1

                    Dim numberOfFiles As Int64 = Directory.GetFiles _
                        (wDString, "*", IO.SearchOption.AllDirectories).Length

                    fileCountTotal = numberOfFiles

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
        CreateProcess("compact")
    End Sub
    Private Sub buttonQueryCompact_Click(sender As Object, e As EventArgs) Handles buttonQueryCompact.Click
        CreateProcess("query")
    End Sub




    Private Sub CreateProcess(passthrougharg As String)
        Try
            MyProcess.Kill()
        Catch ex As Exception
        End Try

        Try
            If passthrougharg = "compact" Then isQueryMode = 0
            If passthrougharg = "query" Then isQueryMode = 1

            progresspercent.Visible = True
            conOut.Items.Clear()

            MyProcess = New Process
            With MyProcess.StartInfo
                .FileName = "CMD.exe"
                .Arguments = ""
                .UseShellExecute = False
                .CreateNoWindow = True
                .StandardOutputEncoding = Encoding.Default
                .StandardErrorEncoding = Encoding.Default
                .WorkingDirectory = workingDir
                .RedirectStandardInput = True
                .RedirectStandardOutput = True
                .RedirectStandardError = True
            End With
            MyProcess.Start()
            MyProcess.StandardInput.WriteLine("chcp")
            MyProcess.StandardInput.Flush()
            Dim Res = MyProcess.StandardOutput.ReadLine()

            Dim i = 0
            Do Until i = 4
                Res = MyProcess.StandardOutput.ReadLine()
                i += 1
            Loop

            'MsgBox(Res.Split(" ").Reverse().ElementAt(0))
            Dim CP = Integer.Parse(Res.Split(" ").Reverse().ElementAt(0))
            MyProcess.StandardInput.WriteLine("exit")
            MyProcess.StandardInput.Flush()
            MyProcess.WaitForExit()




            MyProcess = New Process

            With MyProcess.StartInfo
                .FileName = "CMD.exe"
                .Arguments = ""
                .StandardOutputEncoding = Encoding.GetEncoding(CP)                             'Allow console output to use UTF-8. Otherwise it will translate to ASCII equivalents.
                .StandardErrorEncoding = Encoding.GetEncoding(CP)
                .WorkingDirectory = workingDir                                      'Set working directory via argument, allows UTF-8 to be passed directly.
                .UseShellExecute = False
                .CreateNoWindow = True
                .RedirectStandardInput = True
                .RedirectStandardOutput = True
                .RedirectStandardError = True
            End With

            MyProcess.Start()
            MyProcess.PriorityClass = ProcessPriorityClass.BelowNormal
            MyProcess.EnableRaisingEvents = True
            MyProcess.BeginErrorReadLine()
            MyProcess.BeginOutputReadLine()

            Try
                RunCompact(passthrougharg)

                If passthrougharg = "compact" Then progressPageLabel.Text = "Compressing, Please Wait"
                If passthrougharg = "query" Then progressPageLabel.Text = "Analyzing"
                TabControl1.SelectedTab = ProgressPage

            Catch ex As Exception
            End Try

        Catch ex As Exception
            Console.WriteLine(ex.Data)
        End Try

    End Sub


    Private Sub ButtonRevert_Click(sender As Object, e As EventArgs) Handles buttonRevert.Click             'Handles uncompressing. For now, uncompressing can only be done through the program only to revert a compression that's just been done.
        isQueryMode = 0
        fileCountProgress = 0
        dirCountProgress = 0
        progresspercent.Visible = True
        CompResultsPanel.Visible = False
        buttonRevert.Visible = False
        progressPageLabel.Text = "Reverting Changes, Please Wait"

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
        TabControl1.SelectedTab = InputPage
        dirCountProgress = 0
        fileCountProgress = 0
        isQueryCalledByCompact = 0
        MyProcess.Kill()
    End Sub




    Private Sub CheckShowConOut_CheckedChanged(sender As Object, e As EventArgs) Handles checkShowConOut.CheckedChanged     'Handles showing the embedded console
        If checkShowConOut.Checked Then
            conOut.Visible = True

        Else
            conOut.Visible = False
        End If
    End Sub





    Private Sub MyForm_Closing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If isActive = 1 Then

            If MessageBox.Show _
                ("Are you sure you want to exit?" & vbCrLf & vbCrLf & "Quitting while the Compact function is running is potentially dangerous." _
                 & "Continuing to close could lead to one of your files becoming stuck in a semi-compressed state." _
                 & vbCrLf & vbCrLf &
                 "If you do decide to force quit now, you can potentially fix any unreadable files by running Compact again," _
                 & "selecting the 'Force Compression' Checkbox and then running uncompress on the folder." & vbCrLf & "Click Yes to continue exiting the program.",
                 "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then

                e.Cancel = True

            Else
                Try
                    LetsKillStuff()
                Catch ex As Exception
                End Try
            End If
        Else
            Try
                LetsKillStuff()
            Catch ex As Exception
            End Try
        End If

    End Sub




    Private Sub dirChooser_MouseEnter(sender As Object, e As EventArgs) Handles dirChooser.MouseEnter
        dirChooser.LinkColor = Color.FromArgb(10, 80, 150)
    End Sub
    Private Sub dirChooser_MouseLeave(sender As Object, e As EventArgs) Handles dirChooser.MouseLeave
        dirChooser.LinkColor = Color.FromArgb(37, 110, 196)
    End Sub







    '/////////////FUNCTIONS//////////////

    Private Sub CalculateSaving()   'Calculations for all the relevant information after compression is completed. All the data is parsed from the console ouput using basic strings, but because that occurs on a different thread, information is stored to variables first (The Status Monitors at the top) then those values are used. 

        Dim numberFilesCompressed = 0
        Dim querySize As Int64 = 0


        'If isQueryMode = 0 Then querySize = Long.Parse(Regex.Replace(ARR_TOTALBYTES(CON_INDEX_TOTALBYTESNOTCOMPRESSED), "[^\d]", ""))


        Dim oldFolderSize = Long.Parse(Regex.Replace(ARR_TOTALBYTES(CON_INDEX_TOTALBYTESNOTCOMPRESSED), "[^\d]", ""))

        Dim newfoldersize = Long.Parse(Regex.Replace(ARR_TOTALBYTES(CON_INDEX_TOTALBYTESCOMPRESSED), "[^\d]", ""))

        Try
            numberFilesCompressed = Long.Parse(Regex.Replace(ARR_FILESCOMPRESSED(CON_INDEX_FILESCOMPRESSEDCOUNT), "[^\d]", ""))
        Catch ex As Exception
        End Try

        If GetOutputSize((oldFolderSize - newfoldersize), False) = "0" And isQueryMode = 1 Then

            progressPageLabel.Text = "Folder is not compressed"
            buttonRevert.Visible = False
            isQueryCalledByCompact = 0

        Else

            progressPageLabel.Text = "Folder is compressed"

            If isQueryMode = 1 And isQueryCalledByCompact = 0 Then
                origSizeLabel.Text = GetOutputSize(oldFolderSize, True)
            Else
                origSizeLabel.Text = GetOutputSize(uncompressedfoldersize, True)
            End If

            compressedSizeLabel.Text = GetOutputSize _
                (uncompressedfoldersize - (oldFolderSize - newfoldersize), True)

            compRatioLabel.Text = Math.Round _
                (oldFolderSize / newfoldersize, 1)

            compRatioLabel.Text = Math.Round _
                (uncompressedfoldersize / (uncompressedfoldersize - (oldFolderSize - newfoldersize)), 1)

            spaceSavedLabel.Text = GetOutputSize _
                ((oldFolderSize - newfoldersize), True) + " Saved"

            dirChosenLabel.Text = "❯ In " + dirLabelResults

            labelFilesCompressed.Text =
                numberFilesCompressed.ToString + " / " + fileCountTotal.ToString + " files compressed"

            Try

                compressedSizeVisual.Width = CInt(368 / compRatioLabel.Text)

                If hasqueryfinished = 1 Then
                    isQueryCalledByCompact = 0
                    isQueryMode = 0
                    buttonRevert.Visible = True
                End If

            Catch ex As System.OverflowException
                compressedSizeVisual.Width = 368
            End Try

            If isQueryCalledByCompact = 0 Then

                CompResultsPanel.Visible = True


            ElseIf isQueryCalledByCompact = 1 Then

                progressPageLabel.Text = "Analyzing..."

                buttonRevert.Visible = False
                CompResultsPanel.Visible = False


            End If

        End If

        If isQueryCalledByCompact = 1 Then Queryaftercompact()
        If isQueryCalledByCompact = 0 Then isQueryMode = 0

    End Sub




    Private Sub Queryaftercompact()
        isQueryMode = 1
        hasqueryfinished = 1
        RunCompact("query")
    End Sub




    Dim hasqueryfinished = 0


    Private Sub RunCompact(desiredfunction As String)

        If desiredfunction = "compact" Then

            isQueryCalledByCompact = 0
            compactArgs = "compact /C"

            If checkRecursiveScan.Checked = True Then
                compactArgs = compactArgs + " /S"
            End If
            If checkForceCompression.Checked = True Then
                compactArgs = compactArgs + " /F"
            End If
            If checkHiddenFiles.Checked = True Then
                compactArgs = compactArgs + " /A"
            End If
            If compressX4.Checked = True Then
                compactArgs = compactArgs + " /EXE:XPRESS4K"
            End If
            If compressX8.Checked = True Then
                compactArgs = compactArgs + " /EXE:XPRESS8K"
            End If
            If compressX16.Checked = True Then
                compactArgs = compactArgs + " /EXE:XPRESS16K"
            End If
            If compressLZX.Checked = True Then
                compactArgs = compactArgs + " /EXE:LZX"
            End If

            MyProcess.StandardInput.WriteLine(compactArgs)
            MyProcess.StandardInput.Flush()


            isQueryCalledByCompact = 1
            hasqueryfinished = 0
            isActive = 1

        ElseIf desiredfunction = "uncompact" Then

            isQueryCalledByCompact = 0

            compactArgs = "compact /U /S /EXE "

            If checkForceCompression.Checked = True Then
                compactArgs = compactArgs + " /F"
            End If
            If checkHiddenFiles.Checked = True Then
                compactArgs = compactArgs + " /A"
            End If

            MyProcess.StandardInput.WriteLine(compactArgs)
            MyProcess.StandardInput.Flush()

            isActive = 1


        ElseIf desiredfunction = "query" Then

            compactArgs = "compact /S /Q /EXE"

            MyProcess.StandardInput.WriteLine(compactArgs)
            MyProcess.StandardInput.Flush()

        End If

    End Sub




    Public Function GetOutputSize(ByVal inputsize As Long, Optional ByVal showSizeType As Boolean = False) As String            'Function for converting from Bytes into various units
        Dim sizeType As String = ""
        If inputsize < 1024 Then
            sizeType = " B"
        Else
            If inputsize < (1024 ^ 3) * 9 Then
                If inputsize < (1024 ^ 2) * 9 Then
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
                    If MessageBox.Show("This directory contains a subfolder that you do not have permission to access. Please try running the program again as an Administrator." _
                         & vbCrLf & vbCrLf & "If the problem persists, the subfolder is most likely protected by the System, and by design this program will refuse to let you proceed." _
                        & vbCrLf & vbCrLf & " Would you like to restart the program as an Administrator?", "Permission Error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) = DialogResult.Yes Then

                        RCMenu.RunAsAdmin()
                        Me.Close()

                    End If

                Else
                    MsgBox("This directory contains a subfolder that you do not have permission To access." _
                       & vbCrLf & vbCrLf & "The subfolder is most likely protected by the System, and by design this program will refuse to let you proceed.")
                End If

            End If

        Catch ex As Exception

        End Try

    End Function




    Private WithEvents KillProc As Process
    Private Sub LetsKillStuff()

        KillProc = New Process

        With KillProc.StartInfo
            .FileName = "powershell.exe"
            .Arguments = "taskkill /f /im compact.exe"
            .UseShellExecute = False
            .CreateNoWindow = True
        End With

        KillProc.Start()

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

    Private Sub ExecuteButton_Click _
        (ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click     'Code to run the manual testing input box


        Dim input As String

        input = TextBox1.Text

        Try

            MyProcess.StandardInput.WriteLine(input)
            MyProcess.StandardInput.Flush()
            conOut.Refresh()

            fileCountProgress = 0
            dirCountProgress = 0

        Catch ex As Exception

        End Try

        TextBox1.Text = ""


    End Sub




    Private Sub TestArguments(sender As Object, e As EventArgs) Handles testcompactargs.Click
        compactArgs = "compact /C"
        If checkRecursiveScan.Checked = True Then
            compactArgs = compactArgs + " /S"
        End If
        If checkForceCompression.Checked = True Then
            compactArgs = compactArgs + " /F"
        End If
        If checkHiddenFiles.Checked = True Then
            compactArgs = compactArgs + " /A"
        End If
        If compressX4.Checked = True Then
            compactArgs = compactArgs + " /EXE:XPRESS4K"
        End If
        If compressX8.Checked = True Then
            compactArgs = compactArgs + " /EXE:XPRESS8K"
        End If
        If compressX16.Checked = True Then
            compactArgs = compactArgs + " /EXE:XPRESS16K"
        End If
        If compressLZX.Checked = True Then
            compactArgs = compactArgs + " /EXE:LZX"
        End If
        'MsgBox(compactArgs)
    End Sub







End Class
