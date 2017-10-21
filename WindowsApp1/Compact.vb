Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Ookii.Dialogs                                                                          'Uses Ookii Dialogs for the non-archaic filebrowser dialog. http://www.ookii.org/Software/Dialogs
Imports System.Management

Public Class Compact
    Dim version = "1.4.0_rc1"
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




    Private Sub MyProcess_OutputDataReceived _
        (ByVal sender As Object, ByVal e As System.Diagnostics.DataReceivedEventArgs) _
        Handles MyProcess.OutputDataReceived

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
                If uncompactPass2 = 0 Then
                    dirCountProgress = 0
                    fileCountProgress = 0
                    uncompactPass2 = 1
                    uncompressFinished = 2
                ElseIf uncompactPass2 = 1 Then
                    dirCountProgress = 0
                    fileCountProgress = fileCountTotal
                    uncompressFinished = 1
                    isActive = 0
                    uncompactPass2 = 0
                End If

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
                    If compactprogressbar.Value > 100 Then                                         'Avoids a /r/softwaregore scenario
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
                    If compactprogressbar.Value > 100 Then                                         'Avoids a /r/softwaregore scenario
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
        ElseIf uncompressFinished = 2 Then
            uncompressFinished = 0
            RunCompact("uncompact")

        End If



    End Sub




    Private Sub AppendOutputText(ByVal text As String)                                           'Attach output to the embedded console
        Try
            If conOut.InvokeRequired Then
                Dim serverOutDelegate As New AppendOutputTextDelegate(AddressOf AppendOutputText)
                Me.Invoke(serverOutDelegate, text.Replace("ÿ", " "))
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

            MsgBox("Compressing items in the Windows folder using this program 
                    is not recommended. Please use the command line if you still 
                    wish to compress the Windows folder")

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

                    fileCountTotal = numberOfFiles '- (dirCountTotal + 1) '                        'Windows seems to do a funny thing where it counts "files" as the number of files + folders

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
                .StandardOutputEncoding = Encoding.UTF8                             'Allow console output to use UTF-8. Otherwise it will translate to ASCII equivalents.
                .StandardErrorEncoding = Encoding.UTF8
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
                MyProcess.StandardInput.WriteLine("chcp 65001")                    'UTF-8 codepage in console output. Otherwise it will translate to ASCII equivalents.
                MyProcess.StandardInput.Flush()

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

    Private Sub checkMarkFolder_CheckedChanged(sender As Object, e As EventArgs) Handles checkMarkFolder.CheckedChanged
        If checkMarkFolder.Checked = False Then

            markFolder = 0
        End If
    End Sub




    Private Sub MyForm_Closing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If isActive = 1 Then

            If MessageBox.Show _
                ("Quitting while the Compact function is running is potentially dangerous." _
                 & "Continuing to close could lead to one of your files becoming stuck in a semi-compressed state." _
                 & vbCrLf & vbCrLf &
                 "If you do decide to force quit now, you can potentially fix any unreadable files by running Compact again," _
                 & "selecting the 'Force Compression' Checkbox and then running uncompress on the folder.",
                 "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) <> DialogResult.Yes Then

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


        If isQueryMode = 0 Then querySize = Long.Parse(Regex.Replace(byteComparisonRaw.Substring _
          (0, byteComparisonRaw.IndexOf("t")), "[^\d]", ""))


        Dim oldFolderSize = Long.Parse(Regex.Replace(byteComparisonRaw.Substring _
           (0, byteComparisonRaw.IndexOf("t")), "[^\d]", ""))

        Dim newFolderSizem1 = byteComparisonRaw.Substring _
            (byteComparisonRaw.LastIndexOf("n"c) + 1)

        Dim newfoldersize = Long.Parse(Regex.Replace(newFolderSizem1.Substring _
            (0, newFolderSizem1.Length - 7), "[^\d]", ""))

        Try
            numberFilesCompressed = Long.Parse(Regex.Replace(byteComparisonRawFilesCompressed.Substring _
             (0, byteComparisonRawFilesCompressed.IndexOf("a")), "[^\d]", ""))
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
                If checkMarkFolder.Checked = True Then
                    progressPageLabel.Text = "Marking Folder..."
                Else
                    progressPageLabel.Text = "Analyzing..."
                End If
                buttonRevert.Visible = False
                CompResultsPanel.Visible = False


            End If

        End If

        If isQueryCalledByCompact = 1 Then
            If checkMarkFolder.Checked = True Then
                markFolder = 1
                RunCompact("compact")
            End If

            Queryaftercompact()
        End If
        If isQueryCalledByCompact = 0 Then isQueryMode = 0

    End Sub




    Private Sub Queryaftercompact()
        isQueryMode = 1
        hasqueryfinished = 1
        RunCompact("query")
    End Sub




    Dim hasqueryfinished = 0
    Dim markFolder = 0
    Dim isUncompacting = 0
    Dim uncompactPass2 = 0

    Private Sub RunCompact(desiredfunction As String)

        If desiredfunction = "compact" Then
            Dim isUncompacting = 0
            isQueryCalledByCompact = 0
            compactArgs = "compact /C"

            If markFolder = 1 Then
                compactArgs = compactArgs + " /S *.*"
                MyProcess.StandardInput.WriteLine(compactArgs)
                MyProcess.StandardInput.Flush()

                markFolder = 0
            Else
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

            End If

            isQueryCalledByCompact = 1
            hasqueryfinished = 0
            isActive = 1

        ElseIf desiredfunction = "uncompact" Then
            If uncompactPass2 = 1 Then
                isQueryMode = 0


                progressPageLabel.Text = "Setting Attributes..."
                MyProcess.StandardInput.WriteLine("compact /U /S *.*")
                MyProcess.StandardInput.Flush()

            ElseIf uncompactPass2 = 0 Then

                isQueryCalledByCompact = 0
                isUncompacting = 1
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


            End If


        ElseIf desiredfunction = "query" Then
            Dim isUncompacting = 0
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
