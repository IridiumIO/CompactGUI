Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Ookii.Dialogs                                                                          'Uses Ookii Dialogs for the non-archaic filebrowser dialog. http://www.ookii.org/Software/Dialogs
Imports System.Management


Public Class Compact
    Public Shared version = "2.6.2"
    Private WithEvents MyProcess As Process



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
    Public isQueryMode As Boolean = False
    Public isActive As Boolean = False

    Dim intervaltime As Decimal
    Dim outputbuffer As New ArrayList
    Dim newFolderSize As UInt64
    Dim oldFolderSize As UInt64
    Friend workingDir As String = ""


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

        For Each arg In My.Application.CommandLineArgs
            If Directory.Exists(arg) Then SelectFolder(arg, "cmdlineargs")
        Next

    End Sub




    'Set variables for minor security and error handling
    Dim overrideCompressFolderButton = 0
    Dim directorysizeexceptionCount = 0                                                         'Used in the DirectorySize() Function to ensure the error message only shows up once, even if multiple UnauthorizedAccessException errors get thrown




    Private Sub SelectFolderToCompress(sender As Object, e As EventArgs) Handles dirChooser.LinkClicked, dirChooser.Click
        If isActive = False And isQueryMode = False Then
            Dim folderChoice
            If My.Settings.ExperimentalBrowser = True Then
                folderChoice = New FileFolderDialog
            Else
                folderChoice = New VistaFolderBrowserDialog
            End If

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




    Private Sub SelectFolder(selectedDir As String, senderID As String)
        Cursor.Current = Cursors.WaitCursor

        overrideCompressFolderButton = 0

        If selectedDir.Contains("C:\Windows") Then : ThrowError(ERR_WINDOWSDIRNOTALLOWED)                                    'Makes sure you're not trying to compact the Windows directory. I should Regex this to catch all possible drives hey?
        ElseIf selectedDir.EndsWith(":\") Then : ThrowError(ERR_WHOLEDRIVENOTALLOWED)

        Else
            If selectedDir.Length >= 3 Then                                                                                    'Makes sure the chosen folder isn't a null value or an exception
                Dim DI_selectedDir = New DirectoryInfo(selectedDir)
                workingDir = DI_selectedDir.FullName.TrimEnd("\", "/")

                ListOfFiles.Clear()
                AllFiles.Clear()
                TreeData.Clear()
                SelectedFiles.Items.Clear()
                ExcludedFilesSizes = 0
                directorysizeexceptionCount = 0

                If DI_selectedDir.Name.Length > 0 Then _
                    sb_FolderName.Text = StrConv(DI_selectedDir.Name, VbStrConv.ProperCase)

                If Directory.GetParent(DI_selectedDir.Parent.FullName) IsNot Nothing Then
                    dirChooser.Text = "❯ " + DI_selectedDir.Parent.Parent.Name.Replace(":\", "") +
                            " ❯ " + DI_selectedDir.Parent.Name + " ❯ " + DI_selectedDir.Name

                ElseIf Directory.GetParent(DI_selectedDir.FullName) IsNot Nothing Then
                    dirChooser.Text = "❯ " + DI_selectedDir.Parent.Name.Replace(":\", " ❯ ") + DI_selectedDir.Name
                Else
                    dirChooser.Text = "❯ " + DI_selectedDir.Parent.Name.Replace(":\", " ❯ ") + DI_selectedDir.Name
                End If

                GetFilesToCompress(workingDir, ListOfFiles, My.Settings.SkipNonCompressable)

                For Each fileName In ListOfFiles
                    Dim fN_Listable = fileName.Replace(workingDir, "").Replace("\", " ❯ ")
                    If fN_Listable.Count(Function(x) x = "❯") = 1 Then fN_Listable = fN_Listable.Replace(" ❯ ", "")
                    SelectedFiles.Items.Add(fN_Listable)
                Next

                oldFolderSize = DirectorySize(DI_selectedDir, True) - ExcludedFilesSizes    'Do not count excluded files as part of "compressed"
                Dim oldFolderSize_Formatted = GetOutputSize(oldFolderSize, True)

                PrepareforCompact()

                UnfurlTransition.UnfurlControl(topbar_dirchooserContainer, topbar_dirchooserContainer.Width, Me.Width - sb_Panel.Width - 46, 100)
                WikiHandler.localFolderParse(DI_selectedDir, oldFolderSize_Formatted)

                With topbar_title
                    .Anchor -= AnchorStyles.Right
                    .AutoSize = True
                    .TextAlign = ContentAlignment.MiddleLeft
                    .Font = New Font(topbar_title.Font.Name, 15.75, FontStyle.Regular)
                    .Location = New Point(39, 20)
                End With

                If overrideCompressFolderButton <> 0 Then btnCompress.Enabled = False               'Used as a security measure to stop accidental compression of folders that should not be compressed - even though the compact.exe process will throw an error if you try, I'd prefer to catch it here anyway. 

            Else
                If senderID = "button" Then Console.Write("No folder selected")
            End If
        End If

    End Sub







    Public ListOfFiles As New List(Of String)
    Public ExcludedFilesSizes As Decimal = 0
    Dim FileIndex As Integer = 0

    Private Sub GetFilesToCompress(ByVal targetDirectory As String, targetOutputList As List(Of String), LimitSelectedFiles As Boolean)

        Dim NonCompressableSet As New List(Of String)(Regex.Replace(My.Settings.NonCompressableList, "\s+", "").Split(";"c))

        Dim fileEntries As String() = Directory.GetFiles(targetDirectory)
        Dim fileName As String                                                              ' Process the list of files found in the directory.

        For Each fileName In fileEntries

            If LimitSelectedFiles = True Then
                If Path.GetExtension(fileName) = "" OrElse NonCompressableSet.Contains(Path.GetExtension(fileName).TrimStart(".").ToLowerInvariant) = False Then
                    Dim fi As FileInfo = New FileInfo(fileName)
                    If fi.Length >= My.Settings.IgnoreFileSizeLimit Then
                        targetOutputList.Add(fileName)
                    Else
                        ExcludedFilesSizes += fi.Length
                    End If
                End If
            Else
                Dim fi As FileInfo = New FileInfo(fileName)
                If fi.Length >= My.Settings.IgnoreFileSizeLimit Then
                    targetOutputList.Add(fileName)
                Else
                    ExcludedFilesSizes += fi.Length
                End If
            End If
        Next fileName

        Dim subdirectoryEntries As String() = Directory.GetDirectories(targetDirectory)     ' Recurse into subdirectories
        Dim subdirectory As String
        For Each subdirectory In subdirectoryEntries
            GetFilesToCompress(subdirectory, targetOutputList, LimitSelectedFiles)
        Next subdirectory

    End Sub



    Public WorkingList = New List(Of String)

    Private Sub BtnAnalyze_Click(sender As Object, e As EventArgs) Handles btnAnalyze.Click
        conOut.Items.Clear()
        CalculateSaving()
    End Sub




    Private Sub BtnCompress_Click(sender As System.Object, e As System.EventArgs) Handles btnCompress.Click
        conOut.Items.Clear()
        WorkingList = New List(Of String)(ListOfFiles)
        If WorkingList.count > 0 Then
            CurrentMode = "compact"
            CreateProcess("C")
        Else
            MsgBox("There are no compressable files in this folder. Check the CompactGUI settings to see if you have chosen to ignore certain files from being compressed.")
        End If

    End Sub




    Private Sub BtnUncompress_Click(sender As Object, e As EventArgs) Handles btnUncompress.Click             'Handles uncompressing. For now, uncompressing can only be done through the program only to revert a compression that's just been done.
        ActionBegun("U")
        FileIndex = 0
        CurrentMode = "uncompact"
        WorkingList = New List(Of String)(AllFiles)
        RunCompact(WorkingList(0))
    End Sub




    Private Sub OutputBufferDelegate()
        If Math.Round(intervaltime + 0.1, 2) < Math.Round(Date.Now.TimeOfDay.TotalSeconds, 2) Then      'Buffers incoming strings, then outputs them to the listbox every 0.1s
            Invoke(Sub()
                       conOut.BeginUpdate()
                       For Each str As String In outputbuffer
                           AppendOutputText(str)
                       Next

                       outputbuffer.Clear()
                       intervaltime = Date.Now.TimeOfDay.TotalSeconds
                       conOut.EndUpdate()
                   End Sub)
        End If
    End Sub




    Private Sub ProcessExited(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyProcess.Exited

        OutputBufferDelegate()

        FileIndex += 1

        If FileIndex < WorkingList.Count And WorkingList.Count > 1 Then
            If WorkingList(FileIndex).ToString.Contains(workingDir) Then
                outputbuffer.Add("Compressing: " & vbTab & WorkingList(FileIndex))
                RunCompact(WorkingList(FileIndex))
            Else
                MsgBox("A file that is not contained within your selected folder has been queued for compression. To prevent damage to your system, the program will halt." _
                & vbCrLf & "The file in question is: " & WorkingList(FileIndex))
                isActive = False
                Invoke(Sub()
                           Me.Close()
                       End Sub)
            End If

        Else
                Console.WriteLine("Done")
            Threading.Thread.Sleep(100)
            outputbuffer.Add("Completed:" & vbTab & "Processed " & ListOfFiles.Count & " files")

            For Each str As String In outputbuffer
                AppendOutputText(str)
            Next
            outputbuffer.Clear()
            FileIndex = 0

            Invoke(Sub()
                       CompactProcessCompleted()
                   End Sub)
        End If
    End Sub




    Private Sub SxSCompactIterator(SxSCount As Integer)
        For v As Integer = 1 To SxSCount
            If FileIndex >= WorkingList.Count Then
                Exit For
            End If
            RunCompact(WorkingList(FileIndex))

            v += 1
        Next
    End Sub




    Private Sub AppendOutputText(ByVal text As String)                                           'Attach output to the embedded console

        Invoke(Sub()
                   conOut.Items.Insert(0, text)
                   sb_progressbar.Width = FileIndex / WorkingList.Count * 301
                   sb_progresspercent.Text = Math.Round(FileIndex / WorkingList.Count * 100, 0) & "%"
               End Sub)
    End Sub




    Dim CurrentMode As String


    Private Sub CompactProcessCompleted()
        If CurrentMode = "compact" Then
            ActionCompleted("C")
            CalculateSaving()

        ElseIf CurrentMode = "uncompact" Then
            ActionCompleted("U")

            sb_compressedSizeVisual.Height = 113
            wkPostSizeVal.Text = "?"
            wkPostSizeUnit.Text = ""

        End If

        If checkShutdownOnCompletion.Checked And Not isQueryMode Then
            ShutdownDialog.SDProcIntent.Text = comboChooseShutdown.Text
            FadeTransition.FadeForm(ShutdownDialog, 0, 0.98, 300, True)
        End If

    End Sub




    Private Sub ReturnArrow_Click(sender As Object, e As EventArgs) Handles returnArrow.Click                       'Returns you to the first screen and cleans up some stuff
        PrepareforCompact()
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
                If MyProcess IsNot Nothing AndAlso MyProcess.HasExited = False Then MyProcess.Kill()
            End If
        Else
            If MyProcess IsNot Nothing AndAlso MyProcess.HasExited = False Then MyProcess.Kill()
        End If

    End Sub




    '/////////////FUNCTIONS//////////////


    Const FourGB As Decimal = 2 ^ 32
    Public AllFiles As New List(Of String)

    Public TreeData As New List(Of String)


    Private Sub CalculateSaving()   'Calculations for all the relevant information after compression is completed. All the data is parsed from the console ouput using basic strings, but because that occurs on a different thread, information is stored to variables first (The Status Monitors at the top) then those values are used. 


        ActionBegun("A")
        Dim numberFilesCompressed = 0


        Dim SizeAfterCompression As UInt64
        Dim SizeBeforeCompression As UInt64 = oldFolderSize
        Dim progressVal As Decimal


        GetFilesToCompress(workingDir, AllFiles, False)
        Console.WriteLine("Total Files in Dir: " & AllFiles.Count)

        Dim conOutFileNamePadding As Integer

        For Each fl In AllFiles
            If fl.Length > conOutFileNamePadding Then
                conOutFileNamePadding = fl.Length
            End If
        Next



        conOut.Items.Insert(0, "File" & StrDup(conOutFileNamePadding - 4, " ") & vbTab & "Size" & StrDup(16, " ") & "Size on Disk")
        conOut.Items.Insert(1, "")

        Dim AnalyzedPoorlyCompressedFiles As New List(Of String)

        For Each fpath In AllFiles

            Application.DoEvents()

            Dim compval As UInt64 = GetFileSizeOnDisk(fpath)
            Dim rawval As UInt64 = New FileInfo(fpath).Length

            If rawval > FourGB Then
                Dim Mod4K As Integer = rawval \ FourGB

                If compval < 1.1 * (rawval - (Mod4K * FourGB)) Then   'Checks if the compressed file is smaller than the adjustment consideration @ 4GB (with a 10% leeway)
                    compval = (Mod4K * FourGB + compval)
                End If

            End If

            If compval < rawval Then
                numberFilesCompressed += 1
            Else
                AnalyzedPoorlyCompressedFiles.Add(New FileInfo(fpath).Extension)
            End If

            Dim fpath_Output As String = fpath & StrDup(conOutFileNamePadding - fpath.Length, " ")

            Dim compval_fmt As String = GetOutputSize(compval, True)
            Dim rawval_fmt As String = GetOutputSize(rawval, True)


            outputbuffer.Add(fpath_Output & vbTab & "Size: " & rawval_fmt & StrDup(14 - rawval_fmt.Length, " ") & "Size on Disk: " & compval_fmt)


            If Math.Round(intervaltime + 0.2, 2) < Math.Round(Date.Now.TimeOfDay.TotalSeconds, 2) Then      'Buffers incoming strings, then outputs them to the listbox every 0.1s
                Invoke(Sub()
                           conOut.BeginUpdate()

                           For Each str As String In outputbuffer
                               conOut.Items.Insert(2, str)
                           Next

                           outputbuffer.Clear()

                           intervaltime = Date.Now.TimeOfDay.TotalSeconds
                           conOut.EndUpdate()
                       End Sub)
            End If



            TreeData.Add(fpath & "|" & rawval & "|" & compval)


            SizeAfterCompression += compval

            progressVal += (1 / AllFiles.Count) * 100
            sb_progressbar.Width = progressVal * 3.01
            sb_progresspercent.Text = Math.Round(progressVal, 0) & "%"
        Next

        For Each str As String In outputbuffer
            conOut.Items.Insert(2, str)
        Next

        Dim groups = AnalyzedPoorlyCompressedFiles.GroupBy(Function(value) value)
        Console.WriteLine("Poorly Compressed Extensions:")
        For Each grp In groups
            Console.WriteLine(vbTab & grp(0) & ": " & grp.Count & "/" & AllFiles.Where(Function(value) New FileInfo(value).Extension = grp(0)).Count)
        Next

        newFolderSize = SizeAfterCompression

        If CLng(SizeBeforeCompression) - CLng(SizeAfterCompression) < 0 And isQueryMode = True Then 'Checks if the Folder is NOT compressed

            ActionCompleted("A", False)
        Else

            origSizeLabel.Text = GetOutputSize(SizeBeforeCompression, True)
            Dim PrintOutSize = GetOutputSize(CLng(SizeBeforeCompression) - (CLng(SizeBeforeCompression) - CLng(SizeAfterCompression)), True)

            compressedSizeLabel.Text = PrintOutSize
            wkPostSizeVal.Text = PrintOutSize.Split(" ")(0) 'SPLITTING ON A SPACE COULD LEAD TO FORMATTING BUGS WITH CULTURES
            wkPostSizeUnit.Text = PrintOutSize.Split(" ")(1)

            Dim wkPostSizeVal_Len = TextRenderer.MeasureText(wkPostSizeVal.Text, wkPostSizeVal.Font)
            wkPostSizeUnit.Location = New Point(wkPostSizeVal.Location.X + (wkPostSizeVal.Size.Width / 2) + (wkPostSizeVal_Len.Width / 2 - 8), wkPostSizeVal.Location.Y + 16)



            Dim compRatio = Math.Round(SizeBeforeCompression / SizeAfterCompression, 1)


            spaceSavedLabel.Text = GetOutputSize((SizeBeforeCompression - SizeAfterCompression), True) + " Saved"

            labelFilesCompressed.Text = numberFilesCompressed & " / " & AllFiles.Count & " files compressed"
            help_resultsFilesCompressed.Location = New Point(labelFilesCompressed.Location.X + labelFilesCompressed.Width + 2, labelFilesCompressed.Location.Y + 1)


            Try

                compressedSizeVisual.Width = CInt(320 / compRatio)
                sb_compressedSizeVisual.Height = CInt(113 / compRatio)
                sb_compressedSizeVisual.Location = New Point(sb_compressedSizeVisual.Location.X, 5 + 113 - sb_compressedSizeVisual.Height)

                Callpercent = (CDec(1 - (SizeAfterCompression / SizeBeforeCompression))) * 100
                If My.Settings.ShowNotifications Then _
                        TrayIcon.ShowBalloonTip(1, "Compressed: " & sb_FolderName.Text, vbCrLf & "▸ " & spaceSavedLabel.Text & vbCrLf & "▸ " & Math.Round(Callpercent, 1) & "% Smaller", ToolTipIcon.None)

            Catch ex As OverflowException
                compressedSizeVisual.Width = 320
                sb_compressedSizeVisual.Height = 113
            End Try
            outputbuffer.Clear()
            ActionCompleted("A", True)

            Callpercent = (CDec(1 - (SizeAfterCompression / SizeBeforeCompression))) * 100
            PaintPercentageTransition.PaintTarget(results_arc, Callpercent, 5)

        End If

        isQueryMode = False

    End Sub





    Shared Function GetFileSizeOnDisk(file As String) As Decimal
        Dim info As New FileInfo(file)
        Dim blockSize As UInt64 = 0
        Dim clusterSize As UInteger
        Dim searcher As New ManagementObjectSearcher(
          "select BlockSize,NumberOfBlocks from Win32_Volume WHERE DriveLetter = '" +
          info.Directory.Root.FullName.TrimEnd("\") +
          "'")

        For Each vi As ManagementObject In searcher.[Get]()
            blockSize = vi("BlockSize")
            Exit For
        Next
        searcher.Dispose()
        clusterSize = blockSize
        Dim hosize As UInteger
        Dim losize As UInteger = GetCompressedFileSizeW(file, hosize)
        Dim size As Long
        size = CLng(hosize) << 32 Or losize
        Dim bytes As Decimal = ((size + clusterSize - 1) / clusterSize) * clusterSize

        Return CDec(bytes)
    End Function

    <DllImport("kernel32.dll")>
    Private Shared Function GetCompressedFileSizeW(
        <[In](), MarshalAs(UnmanagedType.LPWStr)> lpFileName As String,
        <Out(), MarshalAs(UnmanagedType.U4)> ByRef lpFileSizeHigh As UInteger) _
        As UInteger
    End Function


    Shared Function GetOutputSize(ByVal byteCount As Long, Optional ByVal showSizeType As Boolean = False) As String            'Function for converting from Bytes into various units

        Dim suf As String() = {" B", " KB", " MB", " GB", " TB", " PB", " EB"}
        If byteCount = 0 Then Return "0" & suf(0)
        Dim bytes As Long = Math.Abs(byteCount)
        Dim place As Integer = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)))
        Dim num As Double = Math.Round(bytes / Math.Pow(1024, place), 1)

        If showSizeType = True Then
            Return (Math.Sign(byteCount) * num).ToString() & suf(place)
        Else
            Return (Math.Sign(byteCount) * num).ToString()
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

            File.AppendAllText(Application.StartupPath & "\CompactGUILog.txt", sb.ToString, Encoding.UTF8)

            MsgBox("Saved log to " & Application.StartupPath & "\CompactGUILog.txt")
            Process.Start(Application.StartupPath & "\CompactGUILog.txt")
        End If
    End Sub




    Private Sub submitToWiki_Click(sender As Object, e As EventArgs) Handles submitToWiki.Click
        WikiSubmission.Folder_Submit = New DirectoryInfo(workingDir).Name

        If compressX4.Checked = True Then WikiSubmission.CompMode_Submit = "X4"
        If compressX8.Checked = True Then WikiSubmission.CompMode_Submit = "X8"
        If compressX16.Checked = True Then WikiSubmission.CompMode_Submit = "X16"
        If compressLZX.Checked = True Then WikiSubmission.CompMode_Submit = "LZX"

        WikiSubmission.BeforeSize_Submit = oldFolderSize
        WikiSubmission.AfterSize_Submit = newFolderSize


        Console.WriteLine(WikiSubmission.Folder_Submit)
        Console.WriteLine(WikiSubmission.CompMode_Submit)
        Console.WriteLine(WikiSubmission.BeforeSize_Submit)
        Console.WriteLine(WikiSubmission.AfterSize_Submit)

        WikiSubmission.Show()
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




#Region "Move And Resize"

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

    Private Sub panel_topBar_MouseDown(sender As Object, e As MouseEventArgs) Handles panel_topBar.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Left And isMaximised = False Then MoveForm()
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
#End Region




#Region "Paint Events and Other Visuals"

    Private Sub buttonCompress_Paint(sender As Object, e As PaintEventArgs) Handles btnCompress.Paint
        Dim btn = DirectCast(sender, Button)
        Dim drawBrush = New SolidBrush(btn.ForeColor)
        Dim sf = New StringFormat() With {
            .Alignment = StringAlignment.Center,
            .LineAlignment = StringAlignment.Center
        }
        btnCompress.Text = String.Empty
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

        Dim x As Integer = updateBanner.Width
        Dim y As Integer = updateBanner.Height
        e.Graphics.FillPolygon(New SolidBrush(Color.FromArgb(255, 47, 66, 83)), New PointF() {New Point(0, 0), New Point(0, y), New Point(y, y)})
        e.Graphics.FillPolygon(New SolidBrush(Color.FromArgb(255, 47, 66, 83)), New PointF() {New Point(x, 0), New Point(x, y), New Point(x - y, y)})

    End Sub




    Private Sub ListBox1_DrawItem(ByVal sender As Object, ByVal e As DrawItemEventArgs) Handles SelectedFiles.DrawItem
        e.DrawBackground()
        e.Graphics.DrawString(SelectedFiles.Items(e.Index).ToString, SelectedFiles.Font, Brushes.Gray, e.Bounds.Left, ((e.Bounds.Height - SelectedFiles.Font.Height) \ 2) + e.Bounds.Top)
    End Sub

    Private Sub ListBox1_MeasureItem(ByVal sender As Object, ByVal e As MeasureItemEventArgs) Handles SelectedFiles.MeasureItem
        e.ItemHeight = 22
    End Sub

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint
        Dim p As New Pen(Brushes.Silver, 1)
        e.Graphics.DrawLine(p, New Point(15, 0), New Point(Panel1.Width, 0))
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




    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles comboChooseShutdown.SelectedIndexChanged, comboChooseShutdown.MouseLeave
        InputPage.Focus()
    End Sub

    Private Sub buttonCompress_EnabledChanged(sender As Object, e As EventArgs) Handles btnCompress.EnabledChanged
        Dim btn = DirectCast(sender, Button)
        btn.ForeColor = If(btn.Enabled, Color.White, Color.Silver)
    End Sub





#End Region





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

