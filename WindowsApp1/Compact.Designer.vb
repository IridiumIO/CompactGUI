<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Compact
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Compact))
        Me.OldconOut = New System.Windows.Forms.RichTextBox()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.buttonCompress = New System.Windows.Forms.Button()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.checkRecursiveScan = New System.Windows.Forms.CheckBox()
        Me.checkHiddenFiles = New System.Windows.Forms.CheckBox()
        Me.checkForceCompression = New System.Windows.Forms.CheckBox()
        Me.compressX4 = New System.Windows.Forms.RadioButton()
        Me.compressX8 = New System.Windows.Forms.RadioButton()
        Me.compressX16 = New System.Windows.Forms.RadioButton()
        Me.compressLZX = New System.Windows.Forms.RadioButton()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.showinfopopup = New System.Windows.Forms.Label()
        Me.dirChooser = New System.Windows.Forms.LinkLabel()
        Me.chosenDirDisplay = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.preSize = New System.Windows.Forms.Label()
        Me.progressTimer = New System.Windows.Forms.Timer(Me.components)
        Me.progresspercent = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.compactprogressbar = New System.Windows.Forms.ProgressBar()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.origSizeLabel = New System.Windows.Forms.Label()
        Me.compressedSizeLabel = New System.Windows.Forms.Label()
        Me.compRatioLabel = New System.Windows.Forms.Label()
        Me.spaceSavedLabel = New System.Windows.Forms.Label()
        Me.testcompactargs = New System.Windows.Forms.Button()
        Me.buttonRevert = New System.Windows.Forms.Button()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.InputPage = New System.Windows.Forms.TabPage()
        Me.buttonQueryCompact = New System.Windows.Forms.Button()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.ProgressPage = New System.Windows.Forms.TabPage()
        Me.returnArrow = New System.Windows.Forms.Label()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.saveconlog = New System.Windows.Forms.Button()
        Me.checkShowConOut = New System.Windows.Forms.CheckBox()
        Me.conOut = New System.Windows.Forms.ListBox()
        Me.CompResultsPanel = New System.Windows.Forms.Panel()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.dirChosenLabel = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.labelFilesCompressed = New System.Windows.Forms.Label()
        Me.compressedSizeVisual = New System.Windows.Forms.Panel()
        Me.Panel5 = New System.Windows.Forms.Panel()
        Me.progressPageLabel = New System.Windows.Forms.Label()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.testFileArgs = New System.Windows.Forms.Button()
        Me.ToolTipFilesCompressed = New System.Windows.Forms.ToolTip(Me.components)
        Me.Panel1.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.InputPage.SuspendLayout()
        Me.FlowLayoutPanel1.SuspendLayout()
        Me.Panel4.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.ProgressPage.SuspendLayout()
        Me.TableLayoutPanel2.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.CompResultsPanel.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.compressedSizeVisual.SuspendLayout()
        Me.Panel5.SuspendLayout()
        Me.TabPage3.SuspendLayout()
        Me.SuspendLayout()
        '
        'OldconOut
        '
        Me.OldconOut.BackColor = System.Drawing.Color.White
        Me.OldconOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.OldconOut.ForeColor = System.Drawing.Color.DimGray
        Me.OldconOut.Location = New System.Drawing.Point(32, 308)
        Me.OldconOut.Name = "OldconOut"
        Me.OldconOut.ReadOnly = True
        Me.OldconOut.Size = New System.Drawing.Size(375, 161)
        Me.OldconOut.TabIndex = 0
        Me.OldconOut.Text = ""
        Me.OldconOut.Visible = False
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(32, 243)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(340, 20)
        Me.TextBox1.TabIndex = 1
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(378, 243)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 2
        Me.Button1.Text = "SendCMD"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'buttonCompress
        '
        Me.buttonCompress.BackColor = System.Drawing.Color.Gainsboro
        Me.buttonCompress.Enabled = False
        Me.buttonCompress.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.buttonCompress.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonCompress.ForeColor = System.Drawing.Color.DarkGray
        Me.buttonCompress.Location = New System.Drawing.Point(8, 302)
        Me.buttonCompress.Margin = New System.Windows.Forms.Padding(8, 3, 3, 3)
        Me.buttonCompress.Name = "buttonCompress"
        Me.buttonCompress.Size = New System.Drawing.Size(390, 54)
        Me.buttonCompress.TabIndex = 3
        Me.buttonCompress.Text = "Compress Folder"
        Me.buttonCompress.UseVisualStyleBackColor = False
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'checkRecursiveScan
        '
        Me.checkRecursiveScan.AutoSize = True
        Me.checkRecursiveScan.Checked = True
        Me.checkRecursiveScan.CheckState = System.Windows.Forms.CheckState.Checked
        Me.checkRecursiveScan.ForeColor = System.Drawing.Color.DimGray
        Me.checkRecursiveScan.Location = New System.Drawing.Point(35, 48)
        Me.checkRecursiveScan.Name = "checkRecursiveScan"
        Me.checkRecursiveScan.Size = New System.Drawing.Size(190, 17)
        Me.checkRecursiveScan.TabIndex = 5
        Me.checkRecursiveScan.Text = "Compress files in subfolders as well"
        Me.checkRecursiveScan.UseVisualStyleBackColor = True
        '
        'checkHiddenFiles
        '
        Me.checkHiddenFiles.AutoSize = True
        Me.checkHiddenFiles.ForeColor = System.Drawing.Color.DimGray
        Me.checkHiddenFiles.Location = New System.Drawing.Point(35, 94)
        Me.checkHiddenFiles.Name = "checkHiddenFiles"
        Me.checkHiddenFiles.Size = New System.Drawing.Size(287, 17)
        Me.checkHiddenFiles.TabIndex = 6
        Me.checkHiddenFiles.Text = "Compress / uncompress hidden and system files as well"
        Me.checkHiddenFiles.UseVisualStyleBackColor = True
        '
        'checkForceCompression
        '
        Me.checkForceCompression.AutoSize = True
        Me.checkForceCompression.ForeColor = System.Drawing.Color.DimGray
        Me.checkForceCompression.Location = New System.Drawing.Point(35, 71)
        Me.checkForceCompression.Name = "checkForceCompression"
        Me.checkForceCompression.Size = New System.Drawing.Size(246, 17)
        Me.checkForceCompression.TabIndex = 7
        Me.checkForceCompression.Text = "Force compression / uncompression on all files"
        Me.checkForceCompression.UseVisualStyleBackColor = True
        '
        'compressX4
        '
        Me.compressX4.AutoSize = True
        Me.compressX4.ForeColor = System.Drawing.Color.DimGray
        Me.compressX4.Location = New System.Drawing.Point(35, 40)
        Me.compressX4.Name = "compressX4"
        Me.compressX4.Size = New System.Drawing.Size(81, 17)
        Me.compressX4.TabIndex = 9
        Me.compressX4.Text = "XPRESS4K"
        Me.compressX4.UseVisualStyleBackColor = True
        '
        'compressX8
        '
        Me.compressX8.AutoSize = True
        Me.compressX8.Checked = True
        Me.compressX8.ForeColor = System.Drawing.Color.DimGray
        Me.compressX8.Location = New System.Drawing.Point(35, 63)
        Me.compressX8.Name = "compressX8"
        Me.compressX8.Size = New System.Drawing.Size(81, 17)
        Me.compressX8.TabIndex = 10
        Me.compressX8.TabStop = True
        Me.compressX8.Text = "XPRESS8K"
        Me.compressX8.UseVisualStyleBackColor = True
        '
        'compressX16
        '
        Me.compressX16.AutoSize = True
        Me.compressX16.ForeColor = System.Drawing.Color.DimGray
        Me.compressX16.Location = New System.Drawing.Point(35, 86)
        Me.compressX16.Name = "compressX16"
        Me.compressX16.Size = New System.Drawing.Size(87, 17)
        Me.compressX16.TabIndex = 11
        Me.compressX16.Text = "XPRESS16K"
        Me.compressX16.UseVisualStyleBackColor = True
        '
        'compressLZX
        '
        Me.compressLZX.AutoSize = True
        Me.compressLZX.ForeColor = System.Drawing.Color.DimGray
        Me.compressLZX.Location = New System.Drawing.Point(35, 111)
        Me.compressLZX.Name = "compressLZX"
        Me.compressLZX.Size = New System.Drawing.Size(48, 17)
        Me.compressLZX.TabIndex = 12
        Me.compressLZX.Text = "LZX "
        Me.compressLZX.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(28, 20)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(136, 30)
        Me.Label1.TabIndex = 13
        Me.Label1.Text = "Compact GUI"
        '
        'Panel1
        '
        Me.Panel1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Controls.Add(Me.showinfopopup)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Location = New System.Drawing.Point(-3, -12)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(491, 56)
        Me.Panel1.TabIndex = 14
        '
        'showinfopopup
        '
        Me.showinfopopup.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.showinfopopup.AutoSize = True
        Me.showinfopopup.ForeColor = System.Drawing.Color.DarkGray
        Me.showinfopopup.Location = New System.Drawing.Point(439, 33)
        Me.showinfopopup.Name = "showinfopopup"
        Me.showinfopopup.Size = New System.Drawing.Size(25, 13)
        Me.showinfopopup.TabIndex = 14
        Me.showinfopopup.Text = "Info"
        '
        'dirChooser
        '
        Me.dirChooser.ActiveLinkColor = System.Drawing.Color.Red
        Me.dirChooser.AutoSize = True
        Me.dirChooser.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.dirChooser.LinkColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.dirChooser.Location = New System.Drawing.Point(58, 39)
        Me.dirChooser.Name = "dirChooser"
        Me.dirChooser.Size = New System.Drawing.Size(202, 21)
        Me.dirChooser.TabIndex = 15
        Me.dirChooser.TabStop = True
        Me.dirChooser.Text = "Choose Folder to Compress"
        Me.dirChooser.VisitedLinkColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        '
        'chosenDirDisplay
        '
        Me.chosenDirDisplay.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.chosenDirDisplay.AutoEllipsis = True
        Me.chosenDirDisplay.BackColor = System.Drawing.Color.WhiteSmoke
        Me.chosenDirDisplay.Cursor = System.Windows.Forms.Cursors.Hand
        Me.chosenDirDisplay.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chosenDirDisplay.Location = New System.Drawing.Point(59, 70)
        Me.chosenDirDisplay.MaximumSize = New System.Drawing.Size(0, 27)
        Me.chosenDirDisplay.MinimumSize = New System.Drawing.Size(374, 27)
        Me.chosenDirDisplay.Name = "chosenDirDisplay"
        Me.chosenDirDisplay.Padding = New System.Windows.Forms.Padding(5)
        Me.chosenDirDisplay.Size = New System.Drawing.Size(374, 27)
        Me.chosenDirDisplay.TabIndex = 16
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.Label2.ForeColor = System.Drawing.Color.DimGray
        Me.Label2.Location = New System.Drawing.Point(20, 5)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(205, 21)
        Me.Label2.TabIndex = 18
        Me.Label2.Text = "Select Compression Method"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.ForeColor = System.Drawing.Color.DimGray
        Me.Label3.Location = New System.Drawing.Point(141, 42)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(50, 13)
        Me.Label3.TabIndex = 19
        Me.Label3.Text = "( fastest )"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.ForeColor = System.Drawing.Color.DimGray
        Me.Label4.Location = New System.Drawing.Point(141, 65)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(108, 13)
        Me.Label4.TabIndex = 19
        Me.Label4.Text = "( better compression )"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.ForeColor = System.Drawing.Color.DimGray
        Me.Label5.Location = New System.Drawing.Point(141, 90)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(101, 13)
        Me.Label5.TabIndex = 19
        Me.Label5.Text = "( best compression )"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.ForeColor = System.Drawing.Color.DimGray
        Me.Label6.Location = New System.Drawing.Point(141, 113)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(85, 13)
        Me.Label6.TabIndex = 19
        Me.Label6.Text = "( most compact )"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.ForeColor = System.Drawing.Color.SteelBlue
        Me.Label7.Location = New System.Drawing.Point(32, 42)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(25, 18)
        Me.Label7.TabIndex = 20
        Me.Label7.Text = "→ "
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.Label8.ForeColor = System.Drawing.Color.DimGray
        Me.Label8.Location = New System.Drawing.Point(20, 11)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(207, 21)
        Me.Label8.TabIndex = 18
        Me.Label8.Text = "Select Additional Arguments"
        '
        'preSize
        '
        Me.preSize.AutoSize = True
        Me.preSize.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.preSize.ForeColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.preSize.Location = New System.Drawing.Point(64, 112)
        Me.preSize.Name = "preSize"
        Me.preSize.Size = New System.Drawing.Size(60, 13)
        Me.preSize.TabIndex = 21
        Me.preSize.Text = "FolderSize"
        Me.preSize.Visible = False
        '
        'progressTimer
        '
        Me.progressTimer.Interval = 20
        '
        'progresspercent
        '
        Me.progresspercent.BackColor = System.Drawing.Color.Transparent
        Me.progresspercent.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.progresspercent.Location = New System.Drawing.Point(356, 39)
        Me.progresspercent.Name = "progresspercent"
        Me.progresspercent.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.progresspercent.Size = New System.Drawing.Size(77, 21)
        Me.progresspercent.TabIndex = 22
        Me.progresspercent.Text = "0%"
        Me.progresspercent.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.ForeColor = System.Drawing.SystemColors.HotTrack
        Me.Label10.Location = New System.Drawing.Point(10, 24)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(171, 25)
        Me.Label10.TabIndex = 23
        Me.Label10.Text = "Testing Grounds"
        '
        'compactprogressbar
        '
        Me.compactprogressbar.BackColor = System.Drawing.Color.WhiteSmoke
        Me.compactprogressbar.Location = New System.Drawing.Point(59, 70)
        Me.compactprogressbar.MaximumSize = New System.Drawing.Size(374, 27)
        Me.compactprogressbar.Name = "compactprogressbar"
        Me.compactprogressbar.Size = New System.Drawing.Size(374, 27)
        Me.compactprogressbar.Step = 2
        Me.compactprogressbar.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        Me.compactprogressbar.TabIndex = 24
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.ForeColor = System.Drawing.Color.DimGray
        Me.Label9.Location = New System.Drawing.Point(0, 120)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(68, 13)
        Me.Label9.TabIndex = 25
        Me.Label9.Text = "Original Size:"
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.ForeColor = System.Drawing.Color.DimGray
        Me.Label11.Location = New System.Drawing.Point(0, 178)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(91, 13)
        Me.Label11.TabIndex = 26
        Me.Label11.Text = "Compressed Size:"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(29, 156)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(35, 13)
        Me.Label12.TabIndex = 26
        Me.Label12.Text = "Ratio:"
        Me.Label12.Visible = False
        '
        'origSizeLabel
        '
        Me.origSizeLabel.AutoSize = True
        Me.origSizeLabel.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.origSizeLabel.ForeColor = System.Drawing.Color.White
        Me.origSizeLabel.Location = New System.Drawing.Point(1, 4)
        Me.origSizeLabel.Name = "origSizeLabel"
        Me.origSizeLabel.Size = New System.Drawing.Size(64, 21)
        Me.origSizeLabel.TabIndex = 27
        Me.origSizeLabel.Text = "300 MB"
        '
        'compressedSizeLabel
        '
        Me.compressedSizeLabel.AutoSize = True
        Me.compressedSizeLabel.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.compressedSizeLabel.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.compressedSizeLabel.Location = New System.Drawing.Point(1, 4)
        Me.compressedSizeLabel.Name = "compressedSizeLabel"
        Me.compressedSizeLabel.Size = New System.Drawing.Size(64, 21)
        Me.compressedSizeLabel.TabIndex = 27
        Me.compressedSizeLabel.Text = "220 MB"
        '
        'compRatioLabel
        '
        Me.compRatioLabel.AutoSize = True
        Me.compRatioLabel.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.compRatioLabel.Location = New System.Drawing.Point(28, 178)
        Me.compRatioLabel.Name = "compRatioLabel"
        Me.compRatioLabel.Size = New System.Drawing.Size(85, 21)
        Me.compRatioLabel.TabIndex = 27
        Me.compRatioLabel.Text = "compRatio"
        Me.compRatioLabel.Visible = False
        '
        'spaceSavedLabel
        '
        Me.spaceSavedLabel.AutoSize = True
        Me.spaceSavedLabel.Font = New System.Drawing.Font("Segoe UI", 16.0!)
        Me.spaceSavedLabel.ForeColor = System.Drawing.Color.DimGray
        Me.spaceSavedLabel.Location = New System.Drawing.Point(-4, 10)
        Me.spaceSavedLabel.Name = "spaceSavedLabel"
        Me.spaceSavedLabel.Size = New System.Drawing.Size(147, 30)
        Me.spaceSavedLabel.TabIndex = 27
        Me.spaceSavedLabel.Text = "700MB Saved"
        '
        'testcompactargs
        '
        Me.testcompactargs.Location = New System.Drawing.Point(32, 75)
        Me.testcompactargs.Name = "testcompactargs"
        Me.testcompactargs.Size = New System.Drawing.Size(113, 23)
        Me.testcompactargs.TabIndex = 28
        Me.testcompactargs.Text = "Test CompactArgs"
        Me.testcompactargs.UseVisualStyleBackColor = True
        '
        'buttonRevert
        '
        Me.buttonRevert.BackColor = System.Drawing.Color.Gainsboro
        Me.buttonRevert.ForeColor = System.Drawing.Color.Black
        Me.buttonRevert.Location = New System.Drawing.Point(3, 3)
        Me.buttonRevert.Name = "buttonRevert"
        Me.buttonRevert.Size = New System.Drawing.Size(375, 39)
        Me.buttonRevert.TabIndex = 29
        Me.buttonRevert.Text = "Uncompress"
        Me.buttonRevert.UseVisualStyleBackColor = False
        Me.buttonRevert.Visible = False
        '
        'TabControl1
        '
        Me.TabControl1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TabControl1.Controls.Add(Me.InputPage)
        Me.TabControl1.Controls.Add(Me.ProgressPage)
        Me.TabControl1.Controls.Add(Me.TabPage3)
        Me.TabControl1.Location = New System.Drawing.Point(-5, 1)
        Me.TabControl1.MinimumSize = New System.Drawing.Size(503, 624)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(503, 634)
        Me.TabControl1.TabIndex = 30
        '
        'InputPage
        '
        Me.InputPage.BackColor = System.Drawing.Color.White
        Me.InputPage.Controls.Add(Me.buttonQueryCompact)
        Me.InputPage.Controls.Add(Me.FlowLayoutPanel1)
        Me.InputPage.Controls.Add(Me.dirChooser)
        Me.InputPage.Controls.Add(Me.preSize)
        Me.InputPage.Controls.Add(Me.chosenDirDisplay)
        Me.InputPage.Controls.Add(Me.Label7)
        Me.InputPage.Location = New System.Drawing.Point(4, 22)
        Me.InputPage.Name = "InputPage"
        Me.InputPage.Padding = New System.Windows.Forms.Padding(3)
        Me.InputPage.Size = New System.Drawing.Size(495, 608)
        Me.InputPage.TabIndex = 0
        Me.InputPage.Text = "InputPage"
        '
        'buttonQueryCompact
        '
        Me.buttonQueryCompact.BackColor = System.Drawing.Color.Gainsboro
        Me.buttonQueryCompact.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue
        Me.buttonQueryCompact.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.buttonQueryCompact.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonQueryCompact.ForeColor = System.Drawing.Color.DarkGray
        Me.buttonQueryCompact.Location = New System.Drawing.Point(289, 105)
        Me.buttonQueryCompact.Name = "buttonQueryCompact"
        Me.buttonQueryCompact.Size = New System.Drawing.Size(144, 27)
        Me.buttonQueryCompact.TabIndex = 23
        Me.buttonQueryCompact.Text = "Analyse Folder"
        Me.buttonQueryCompact.UseVisualStyleBackColor = False
        Me.buttonQueryCompact.Visible = False
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.FlowLayoutPanel1.AutoSize = True
        Me.FlowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.FlowLayoutPanel1.Controls.Add(Me.Panel4)
        Me.FlowLayoutPanel1.Controls.Add(Me.Panel3)
        Me.FlowLayoutPanel1.Controls.Add(Me.buttonCompress)
        Me.FlowLayoutPanel1.Location = New System.Drawing.Point(35, 160)
        Me.FlowLayoutPanel1.MaximumSize = New System.Drawing.Size(700, 0)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        Me.FlowLayoutPanel1.Size = New System.Drawing.Size(423, 359)
        Me.FlowLayoutPanel1.TabIndex = 22
        '
        'Panel4
        '
        Me.Panel4.Controls.Add(Me.Label2)
        Me.Panel4.Controls.Add(Me.Label6)
        Me.Panel4.Controls.Add(Me.Label5)
        Me.Panel4.Controls.Add(Me.Label4)
        Me.Panel4.Controls.Add(Me.Label3)
        Me.Panel4.Controls.Add(Me.compressX4)
        Me.Panel4.Controls.Add(Me.compressLZX)
        Me.Panel4.Controls.Add(Me.compressX8)
        Me.Panel4.Controls.Add(Me.compressX16)
        Me.Panel4.Location = New System.Drawing.Point(3, 3)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(262, 137)
        Me.Panel4.TabIndex = 24
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.Label8)
        Me.Panel3.Controls.Add(Me.checkForceCompression)
        Me.Panel3.Controls.Add(Me.Label14)
        Me.Panel3.Controls.Add(Me.checkHiddenFiles)
        Me.Panel3.Controls.Add(Me.checkRecursiveScan)
        Me.Panel3.Location = New System.Drawing.Point(3, 146)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(417, 150)
        Me.Panel3.TabIndex = 23
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label14.ForeColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.Label14.Location = New System.Drawing.Point(278, 69)
        Me.Label14.Margin = New System.Windows.Forms.Padding(0, 1, 0, 0)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(22, 17)
        Me.Label14.TabIndex = 32
        Me.Label14.Text = "(?)"
        Me.ToolTipFilesCompressed.SetToolTip(Me.Label14, resources.GetString("Label14.ToolTip"))
        '
        'ProgressPage
        '
        Me.ProgressPage.BackColor = System.Drawing.Color.White
        Me.ProgressPage.Controls.Add(Me.returnArrow)
        Me.ProgressPage.Controls.Add(Me.TableLayoutPanel2)
        Me.ProgressPage.Controls.Add(Me.progressPageLabel)
        Me.ProgressPage.Controls.Add(Me.compactprogressbar)
        Me.ProgressPage.Controls.Add(Me.progresspercent)
        Me.ProgressPage.Location = New System.Drawing.Point(4, 22)
        Me.ProgressPage.Name = "ProgressPage"
        Me.ProgressPage.Padding = New System.Windows.Forms.Padding(3)
        Me.ProgressPage.Size = New System.Drawing.Size(495, 608)
        Me.ProgressPage.TabIndex = 1
        Me.ProgressPage.Text = "ProgressPage"
        '
        'returnArrow
        '
        Me.returnArrow.AutoSize = True
        Me.returnArrow.Font = New System.Drawing.Font("Calibri", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.returnArrow.ForeColor = System.Drawing.Color.SteelBlue
        Me.returnArrow.Location = New System.Drawing.Point(32, 42)
        Me.returnArrow.Name = "returnArrow"
        Me.returnArrow.Size = New System.Drawing.Size(25, 18)
        Me.returnArrow.TabIndex = 22
        Me.returnArrow.Text = "← "
        Me.returnArrow.Visible = False
        '
        'TableLayoutPanel2
        '
        Me.TableLayoutPanel2.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TableLayoutPanel2.ColumnCount = 1
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel2.Controls.Add(Me.Panel2, 0, 2)
        Me.TableLayoutPanel2.Controls.Add(Me.CompResultsPanel, 0, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.buttonRevert, 0, 0)
        Me.TableLayoutPanel2.Location = New System.Drawing.Point(55, 102)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        Me.TableLayoutPanel2.RowCount = 3
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel2.Size = New System.Drawing.Size(383, 484)
        Me.TableLayoutPanel2.TabIndex = 31
        '
        'Panel2
        '
        Me.Panel2.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel2.Controls.Add(Me.saveconlog)
        Me.Panel2.Controls.Add(Me.checkShowConOut)
        Me.Panel2.Controls.Add(Me.conOut)
        Me.Panel2.Location = New System.Drawing.Point(3, 285)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(377, 196)
        Me.Panel2.TabIndex = 33
        '
        'saveconlog
        '
        Me.saveconlog.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.saveconlog.BackColor = System.Drawing.Color.WhiteSmoke
        Me.saveconlog.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.saveconlog.Location = New System.Drawing.Point(299, 162)
        Me.saveconlog.Name = "saveconlog"
        Me.saveconlog.Size = New System.Drawing.Size(75, 23)
        Me.saveconlog.TabIndex = 33
        Me.saveconlog.Text = "Save Log"
        Me.saveconlog.UseVisualStyleBackColor = False
        Me.saveconlog.Visible = False
        '
        'checkShowConOut
        '
        Me.checkShowConOut.AutoSize = True
        Me.checkShowConOut.ForeColor = System.Drawing.Color.DimGray
        Me.checkShowConOut.Location = New System.Drawing.Point(1, 3)
        Me.checkShowConOut.Name = "checkShowConOut"
        Me.checkShowConOut.Size = New System.Drawing.Size(139, 17)
        Me.checkShowConOut.TabIndex = 32
        Me.checkShowConOut.Text = "Show Detailed Progress"
        Me.checkShowConOut.UseVisualStyleBackColor = True
        '
        'conOut
        '
        Me.conOut.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.conOut.BackColor = System.Drawing.Color.White
        Me.conOut.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.conOut.ForeColor = System.Drawing.Color.DimGray
        Me.conOut.FormattingEnabled = True
        Me.conOut.Location = New System.Drawing.Point(0, 26)
        Me.conOut.Name = "conOut"
        Me.conOut.Size = New System.Drawing.Size(374, 130)
        Me.conOut.TabIndex = 30
        Me.conOut.Visible = False
        '
        'CompResultsPanel
        '
        Me.CompResultsPanel.Controls.Add(Me.Label15)
        Me.CompResultsPanel.Controls.Add(Me.dirChosenLabel)
        Me.CompResultsPanel.Controls.Add(Me.TableLayoutPanel1)
        Me.CompResultsPanel.Controls.Add(Me.compressedSizeVisual)
        Me.CompResultsPanel.Controls.Add(Me.Panel5)
        Me.CompResultsPanel.Controls.Add(Me.Label9)
        Me.CompResultsPanel.Controls.Add(Me.Label11)
        Me.CompResultsPanel.Controls.Add(Me.spaceSavedLabel)
        Me.CompResultsPanel.Location = New System.Drawing.Point(3, 48)
        Me.CompResultsPanel.Name = "CompResultsPanel"
        Me.CompResultsPanel.Size = New System.Drawing.Size(375, 231)
        Me.CompResultsPanel.TabIndex = 31
        Me.CompResultsPanel.Visible = False
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label15.ForeColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.Label15.Location = New System.Drawing.Point(237, 0)
        Me.Label15.Name = "Label15"
        Me.Label15.Padding = New System.Windows.Forms.Padding(3, 5, 3, 5)
        Me.Label15.Size = New System.Drawing.Size(138, 23)
        Me.Label15.TabIndex = 33
        Me.Label15.Text = "❯ Submit Results to Wiki"
        '
        'dirChosenLabel
        '
        Me.dirChosenLabel.AutoSize = True
        Me.dirChosenLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.dirChosenLabel.ForeColor = System.Drawing.Color.DarkGray
        Me.dirChosenLabel.Location = New System.Drawing.Point(2, 79)
        Me.dirChosenLabel.Name = "dirChosenLabel"
        Me.dirChosenLabel.Size = New System.Drawing.Size(87, 15)
        Me.dirChosenLabel.TabIndex = 32
        Me.dirChosenLabel.Text = "dirchosenLabel"
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.AutoSize = True
        Me.TableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel1.Controls.Add(Me.Label13, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.labelFilesCompressed, 0, 0)
        Me.TableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(1, 50)
        Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 1
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(172, 20)
        Me.TableLayoutPanel1.TabIndex = 31
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label13.ForeColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.Label13.Location = New System.Drawing.Point(150, 1)
        Me.Label13.Margin = New System.Windows.Forms.Padding(0, 1, 0, 0)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(22, 17)
        Me.Label13.TabIndex = 31
        Me.Label13.Text = "(?)"
        Me.ToolTipFilesCompressed.SetToolTip(Me.Label13, resources.GetString("Label13.ToolTip"))
        '
        'labelFilesCompressed
        '
        Me.labelFilesCompressed.AutoSize = True
        Me.labelFilesCompressed.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelFilesCompressed.ForeColor = System.Drawing.Color.DimGray
        Me.labelFilesCompressed.Location = New System.Drawing.Point(0, 0)
        Me.labelFilesCompressed.Margin = New System.Windows.Forms.Padding(0)
        Me.labelFilesCompressed.MaximumSize = New System.Drawing.Size(0, 20)
        Me.labelFilesCompressed.MinimumSize = New System.Drawing.Size(150, 20)
        Me.labelFilesCompressed.Name = "labelFilesCompressed"
        Me.labelFilesCompressed.Size = New System.Drawing.Size(150, 20)
        Me.labelFilesCompressed.TabIndex = 30
        Me.labelFilesCompressed.Text = "n/n Files Compressed"
        '
        'compressedSizeVisual
        '
        Me.compressedSizeVisual.BackColor = System.Drawing.Color.FromArgb(CType(CType(39, Byte), Integer), CType(CType(174, Byte), Integer), CType(CType(96, Byte), Integer))
        Me.compressedSizeVisual.Controls.Add(Me.compressedSizeLabel)
        Me.compressedSizeVisual.Location = New System.Drawing.Point(2, 194)
        Me.compressedSizeVisual.Name = "compressedSizeVisual"
        Me.compressedSizeVisual.Size = New System.Drawing.Size(368, 30)
        Me.compressedSizeVisual.TabIndex = 29
        '
        'Panel5
        '
        Me.Panel5.BackColor = System.Drawing.Color.FromArgb(CType(CType(211, Byte), Integer), CType(CType(84, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.Panel5.Controls.Add(Me.origSizeLabel)
        Me.Panel5.Location = New System.Drawing.Point(2, 136)
        Me.Panel5.Name = "Panel5"
        Me.Panel5.Size = New System.Drawing.Size(368, 30)
        Me.Panel5.TabIndex = 28
        '
        'progressPageLabel
        '
        Me.progressPageLabel.AutoSize = True
        Me.progressPageLabel.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.progressPageLabel.ForeColor = System.Drawing.Color.DimGray
        Me.progressPageLabel.Location = New System.Drawing.Point(58, 39)
        Me.progressPageLabel.Name = "progressPageLabel"
        Me.progressPageLabel.Size = New System.Drawing.Size(188, 21)
        Me.progressPageLabel.TabIndex = 30
        Me.progressPageLabel.Text = "Compressing, Please Wait"
        '
        'TabPage3
        '
        Me.TabPage3.Controls.Add(Me.testFileArgs)
        Me.TabPage3.Controls.Add(Me.Label10)
        Me.TabPage3.Controls.Add(Me.OldconOut)
        Me.TabPage3.Controls.Add(Me.testcompactargs)
        Me.TabPage3.Controls.Add(Me.compRatioLabel)
        Me.TabPage3.Controls.Add(Me.Button1)
        Me.TabPage3.Controls.Add(Me.Label12)
        Me.TabPage3.Controls.Add(Me.TextBox1)
        Me.TabPage3.Location = New System.Drawing.Point(4, 22)
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage3.Size = New System.Drawing.Size(495, 608)
        Me.TabPage3.TabIndex = 2
        Me.TabPage3.Text = "TabPage3"
        Me.TabPage3.UseVisualStyleBackColor = True
        '
        'testFileArgs
        '
        Me.testFileArgs.Location = New System.Drawing.Point(278, 74)
        Me.testFileArgs.Name = "testFileArgs"
        Me.testFileArgs.Size = New System.Drawing.Size(75, 23)
        Me.testFileArgs.TabIndex = 29
        Me.testFileArgs.Text = "Test FileArgs"
        Me.testFileArgs.UseVisualStyleBackColor = True
        '
        'ToolTipFilesCompressed
        '
        Me.ToolTipFilesCompressed.AutoPopDelay = 12000
        Me.ToolTipFilesCompressed.BackColor = System.Drawing.Color.White
        Me.ToolTipFilesCompressed.ForeColor = System.Drawing.SystemColors.WindowFrame
        Me.ToolTipFilesCompressed.InitialDelay = 200
        Me.ToolTipFilesCompressed.IsBalloon = True
        Me.ToolTipFilesCompressed.ReshowDelay = 100
        Me.ToolTipFilesCompressed.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info
        Me.ToolTipFilesCompressed.ToolTipTitle = "Information"
        '
        'Compact
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(474, 621)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.Panel1)
        Me.DoubleBuffered = True
        Me.ForeColor = System.Drawing.Color.DimGray
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(490, 660)
        Me.Name = "Compact"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.InputPage.ResumeLayout(False)
        Me.InputPage.PerformLayout()
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.Panel4.ResumeLayout(False)
        Me.Panel4.PerformLayout()
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.ProgressPage.ResumeLayout(False)
        Me.ProgressPage.PerformLayout()
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.CompResultsPanel.ResumeLayout(False)
        Me.CompResultsPanel.PerformLayout()
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.compressedSizeVisual.ResumeLayout(False)
        Me.compressedSizeVisual.PerformLayout()
        Me.Panel5.ResumeLayout(False)
        Me.Panel5.PerformLayout()
        Me.TabPage3.ResumeLayout(False)
        Me.TabPage3.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents OldconOut As RichTextBox
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents buttonCompress As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents checkRecursiveScan As CheckBox
    Friend WithEvents checkHiddenFiles As CheckBox
    Friend WithEvents checkForceCompression As CheckBox
    Friend WithEvents compressX4 As RadioButton
    Friend WithEvents compressX8 As RadioButton
    Friend WithEvents compressX16 As RadioButton
    Friend WithEvents compressLZX As RadioButton
    Friend WithEvents Label1 As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents dirChooser As LinkLabel
    Friend WithEvents chosenDirDisplay As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents preSize As Label
    Friend WithEvents progressTimer As Timer
    Friend WithEvents progresspercent As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents compactprogressbar As ProgressBar
    Friend WithEvents Label9 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents Label12 As Label
    Friend WithEvents origSizeLabel As Label
    Friend WithEvents compressedSizeLabel As Label
    Friend WithEvents compRatioLabel As Label
    Friend WithEvents spaceSavedLabel As Label
    Friend WithEvents testcompactargs As Button
    Friend WithEvents buttonRevert As Button
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents InputPage As TabPage
    Friend WithEvents ProgressPage As TabPage
    Friend WithEvents TabPage3 As TabPage
    Friend WithEvents progressPageLabel As Label
    Friend WithEvents returnArrow As Label
    Friend WithEvents CompResultsPanel As Panel
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents Panel3 As Panel
    Friend WithEvents Panel4 As Panel
    Friend WithEvents Panel5 As Panel
    Friend WithEvents compressedSizeVisual As Panel
    Friend WithEvents checkShowConOut As CheckBox
    Friend WithEvents showinfopopup As Label
    Friend WithEvents testFileArgs As Button
    Friend WithEvents buttonQueryCompact As Button
    Friend WithEvents labelFilesCompressed As Label
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents Label13 As Label
    Friend WithEvents ToolTipFilesCompressed As ToolTip
    Friend WithEvents dirChosenLabel As Label
    Friend WithEvents Label14 As Label
    Friend WithEvents conOut As ListBox
    Friend WithEvents Panel2 As Panel
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents Label15 As Label
    Friend WithEvents saveconlog As Button
End Class
