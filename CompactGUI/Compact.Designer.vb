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
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.topbar_title = New System.Windows.Forms.Label()
        Me.panel_topBar = New System.Windows.Forms.Panel()
        Me.showinfopopup = New System.Windows.Forms.Label()
        Me.updateBanner = New System.Windows.Forms.Panel()
        Me.dlUpdateLink = New System.Windows.Forms.LinkLabel()
        Me.btnAnalyze = New System.Windows.Forms.Button()
        Me.topbar_dirchooserContainer = New System.Windows.Forms.Panel()
        Me.dirChooser = New System.Windows.Forms.LinkLabel()
        Me.btn_Mainmin = New System.Windows.Forms.Button()
        Me.btn_Mainmax = New System.Windows.Forms.Button()
        Me.btn_Mainexit = New System.Windows.Forms.Button()
        Me.topbar_icon = New System.Windows.Forms.PictureBox()
        Me.progressTimer = New System.Windows.Forms.Timer(Me.components)
        Me.seecompest = New System.Windows.Forms.Label()
        Me.ToolTipFilesCompressed = New System.Windows.Forms.ToolTip(Me.components)
        Me.help_resultsFilesCompressed = New System.Windows.Forms.Label()
        Me.sb_lblGameIssues = New System.Windows.Forms.Label()
        Me.compressX4 = New System.Windows.Forms.RadioButton()
        Me.compressLZX = New System.Windows.Forms.RadioButton()
        Me.compressX8 = New System.Windows.Forms.RadioButton()
        Me.compressX16 = New System.Windows.Forms.RadioButton()
        Me.checkForceCompression = New System.Windows.Forms.CheckBox()
        Me.checkHiddenFiles = New System.Windows.Forms.CheckBox()
        Me.checkRecursiveScan = New System.Windows.Forms.CheckBox()
        Me.panel_MainWindow = New System.Windows.Forms.Panel()
        Me.sb_Panel = New System.Windows.Forms.Panel()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.sb_ResultsPanel = New System.Windows.Forms.Panel()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.sb_compressedSizeVisual = New System.Windows.Forms.Panel()
        Me.Panel6 = New System.Windows.Forms.Panel()
        Me.sb_AnalysisPanel = New System.Windows.Forms.Panel()
        Me.sb_progresslabel = New System.Windows.Forms.Label()
        Me.sb_progressbar = New System.Windows.Forms.Panel()
        Me.sb_progresspercent = New System.Windows.Forms.Label()
        Me.wkPostSizeUnit = New System.Windows.Forms.Label()
        Me.wkPostSizeVal = New System.Windows.Forms.Label()
        Me.wkPreSizeUnit = New System.Windows.Forms.Label()
        Me.TableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.sb_labelCompressed = New System.Windows.Forms.Label()
        Me.sb_FolderName = New System.Windows.Forms.Label()
        Me.vis_dropshadowmain2 = New System.Windows.Forms.Panel()
        Me.wkPreSizeVal = New System.Windows.Forms.Label()
        Me.btnCompress = New System.Windows.Forms.Button()
        Me.btnUncompress = New System.Windows.Forms.Button()
        Me.vis_dropshadowMain = New System.Windows.Forms.Panel()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.InputPage = New System.Windows.Forms.TabPage()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SelectedFiles = New System.Windows.Forms.ListBox()
        Me.FlowPanel_CompressionOptions = New System.Windows.Forms.FlowLayoutPanel()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.comboChooseShutdown = New System.Windows.Forms.ComboBox()
        Me.checkShutdownOnCompletion = New System.Windows.Forms.CheckBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.ProgressPage = New System.Windows.Forms.TabPage()
        Me.TableLayoutPanel4 = New System.Windows.Forms.TableLayoutPanel()
        Me.CompResultsPanel = New System.Windows.Forms.Panel()
        Me.results_arc = New CompactGUI.GraphicsPanel()
        Me.labelFilesCompressed = New System.Windows.Forms.Label()
        Me.dirChosenLabel = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.compressedSizeVisual = New System.Windows.Forms.Panel()
        Me.compressedSizeLabel = New System.Windows.Forms.Label()
        Me.Panel5 = New System.Windows.Forms.Panel()
        Me.origSizeLabel = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.panel_console = New System.Windows.Forms.Panel()
        Me.saveconlog = New System.Windows.Forms.Button()
        Me.conOut = New System.Windows.Forms.ListBox()
        Me.returnArrow = New System.Windows.Forms.Label()
        Me.submitToWiki = New System.Windows.Forms.Label()
        Me.spaceSavedLabel = New System.Windows.Forms.Label()
        Me.TrayIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.TrayMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.Tray_ShowMain = New System.Windows.Forms.ToolStripMenuItem()
        Me.panel_topBar.SuspendLayout()
        Me.updateBanner.SuspendLayout()
        Me.topbar_dirchooserContainer.SuspendLayout()
        CType(Me.topbar_icon, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.panel_MainWindow.SuspendLayout()
        Me.sb_Panel.SuspendLayout()
        Me.TableLayoutPanel2.SuspendLayout()
        Me.sb_ResultsPanel.SuspendLayout()
        Me.sb_AnalysisPanel.SuspendLayout()
        Me.TableLayoutPanel3.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.InputPage.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.FlowPanel_CompressionOptions.SuspendLayout()
        Me.Panel4.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.ProgressPage.SuspendLayout()
        Me.TableLayoutPanel4.SuspendLayout()
        Me.CompResultsPanel.SuspendLayout()
        Me.compressedSizeVisual.SuspendLayout()
        Me.Panel5.SuspendLayout()
        Me.panel_console.SuspendLayout()
        Me.TrayMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'topbar_title
        '
        Me.topbar_title.AutoSize = True
        Me.topbar_title.Font = New System.Drawing.Font("Segoe UI Semilight", 15.75!)
        Me.topbar_title.ForeColor = System.Drawing.Color.White
        Me.topbar_title.Location = New System.Drawing.Point(39, 20)
        Me.topbar_title.Name = "topbar_title"
        Me.topbar_title.Size = New System.Drawing.Size(136, 30)
        Me.topbar_title.TabIndex = 13
        Me.topbar_title.Text = "CompactGUI²"
        '
        'panel_topBar
        '
        Me.panel_topBar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.panel_topBar.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.panel_topBar.Controls.Add(Me.showinfopopup)
        Me.panel_topBar.Controls.Add(Me.updateBanner)
        Me.panel_topBar.Controls.Add(Me.btnAnalyze)
        Me.panel_topBar.Controls.Add(Me.topbar_dirchooserContainer)
        Me.panel_topBar.Controls.Add(Me.btn_Mainmin)
        Me.panel_topBar.Controls.Add(Me.btn_Mainmax)
        Me.panel_topBar.Controls.Add(Me.btn_Mainexit)
        Me.panel_topBar.Controls.Add(Me.topbar_icon)
        Me.panel_topBar.Controls.Add(Me.topbar_title)
        Me.panel_topBar.Location = New System.Drawing.Point(0, 0)
        Me.panel_topBar.Name = "panel_topBar"
        Me.panel_topBar.Size = New System.Drawing.Size(1000, 135)
        Me.panel_topBar.TabIndex = 14
        '
        'showinfopopup
        '
        Me.showinfopopup.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.showinfopopup.AutoSize = True
        Me.showinfopopup.Font = New System.Drawing.Font("Segoe MDL2 Assets", 11.0!)
        Me.showinfopopup.ForeColor = System.Drawing.Color.White
        Me.showinfopopup.Location = New System.Drawing.Point(798, 15)
        Me.showinfopopup.Name = "showinfopopup"
        Me.showinfopopup.Size = New System.Drawing.Size(22, 15)
        Me.showinfopopup.TabIndex = 14
        Me.showinfopopup.Text = ""
        '
        'updateBanner
        '
        Me.updateBanner.Anchor = System.Windows.Forms.AnchorStyles.Top
        Me.updateBanner.BackColor = System.Drawing.Color.FromArgb(CType(CType(39, Byte), Integer), CType(CType(174, Byte), Integer), CType(CType(96, Byte), Integer))
        Me.updateBanner.Controls.Add(Me.dlUpdateLink)
        Me.updateBanner.Location = New System.Drawing.Point(310, 0)
        Me.updateBanner.Name = "updateBanner"
        Me.updateBanner.Size = New System.Drawing.Size(380, 19)
        Me.updateBanner.TabIndex = 24
        Me.updateBanner.Visible = False
        '
        'dlUpdateLink
        '
        Me.dlUpdateLink.BackColor = System.Drawing.Color.Transparent
        Me.dlUpdateLink.Font = New System.Drawing.Font("Segoe UI", 8.0!)
        Me.dlUpdateLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.dlUpdateLink.LinkColor = System.Drawing.Color.AliceBlue
        Me.dlUpdateLink.Location = New System.Drawing.Point(0, 0)
        Me.dlUpdateLink.Margin = New System.Windows.Forms.Padding(0)
        Me.dlUpdateLink.Name = "dlUpdateLink"
        Me.dlUpdateLink.Size = New System.Drawing.Size(380, 18)
        Me.dlUpdateLink.TabIndex = 1
        Me.dlUpdateLink.TabStop = True
        Me.dlUpdateLink.Text = "Update Available: Click to download V2.X"
        Me.dlUpdateLink.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnAnalyze
        '
        Me.btnAnalyze.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnAnalyze.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.btnAnalyze.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnAnalyze.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(72, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnAnalyze.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnAnalyze.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnAnalyze.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnAnalyze.ForeColor = System.Drawing.Color.White
        Me.btnAnalyze.Location = New System.Drawing.Point(742, 71)
        Me.btnAnalyze.Margin = New System.Windows.Forms.Padding(30, 3, 3, 3)
        Me.btnAnalyze.Name = "btnAnalyze"
        Me.btnAnalyze.Size = New System.Drawing.Size(165, 39)
        Me.btnAnalyze.TabIndex = 23
        Me.btnAnalyze.Text = "Analyse Folder"
        Me.btnAnalyze.UseVisualStyleBackColor = False
        Me.btnAnalyze.Visible = False
        '
        'topbar_dirchooserContainer
        '
        Me.topbar_dirchooserContainer.AllowDrop = True
        Me.topbar_dirchooserContainer.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.topbar_dirchooserContainer.BackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.topbar_dirchooserContainer.Controls.Add(Me.dirChooser)
        Me.topbar_dirchooserContainer.Location = New System.Drawing.Point(44, 69)
        Me.topbar_dirchooserContainer.MinimumSize = New System.Drawing.Size(270, 0)
        Me.topbar_dirchooserContainer.Name = "topbar_dirchooserContainer"
        Me.topbar_dirchooserContainer.Size = New System.Drawing.Size(912, 43)
        Me.topbar_dirchooserContainer.TabIndex = 15
        '
        'dirChooser
        '
        Me.dirChooser.ActiveLinkColor = System.Drawing.Color.White
        Me.dirChooser.AllowDrop = True
        Me.dirChooser.AutoEllipsis = True
        Me.dirChooser.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dirChooser.Font = New System.Drawing.Font("Segoe UI", 11.25!)
        Me.dirChooser.ForeColor = System.Drawing.Color.White
        Me.dirChooser.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline
        Me.dirChooser.LinkColor = System.Drawing.Color.White
        Me.dirChooser.Location = New System.Drawing.Point(0, 0)
        Me.dirChooser.Name = "dirChooser"
        Me.dirChooser.Padding = New System.Windows.Forms.Padding(19, 0, 0, 0)
        Me.dirChooser.Size = New System.Drawing.Size(912, 43)
        Me.dirChooser.TabIndex = 15
        Me.dirChooser.TabStop = True
        Me.dirChooser.Text = "❯   Select Target Folder"
        Me.dirChooser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.dirChooser.VisitedLinkColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        '
        'btn_Mainmin
        '
        Me.btn_Mainmin.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_Mainmin.BackColor = System.Drawing.Color.Transparent
        Me.btn_Mainmin.FlatAppearance.BorderSize = 0
        Me.btn_Mainmin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent
        Me.btn_Mainmin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent
        Me.btn_Mainmin.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_Mainmin.Font = New System.Drawing.Font("Segoe UI Symbol", 11.25!)
        Me.btn_Mainmin.ForeColor = System.Drawing.Color.White
        Me.btn_Mainmin.Location = New System.Drawing.Point(841, 0)
        Me.btn_Mainmin.Margin = New System.Windows.Forms.Padding(0)
        Me.btn_Mainmin.Name = "btn_Mainmin"
        Me.btn_Mainmin.Size = New System.Drawing.Size(45, 42)
        Me.btn_Mainmin.TabIndex = 17
        Me.btn_Mainmin.TabStop = False
        Me.btn_Mainmin.Text = "—"
        Me.btn_Mainmin.UseVisualStyleBackColor = False
        '
        'btn_Mainmax
        '
        Me.btn_Mainmax.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_Mainmax.BackColor = System.Drawing.Color.Transparent
        Me.btn_Mainmax.FlatAppearance.BorderSize = 0
        Me.btn_Mainmax.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent
        Me.btn_Mainmax.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent
        Me.btn_Mainmax.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_Mainmax.Font = New System.Drawing.Font("Segoe UI Symbol", 11.25!)
        Me.btn_Mainmax.ForeColor = System.Drawing.Color.White
        Me.btn_Mainmax.Location = New System.Drawing.Point(899, 0)
        Me.btn_Mainmax.Margin = New System.Windows.Forms.Padding(0)
        Me.btn_Mainmax.Name = "btn_Mainmax"
        Me.btn_Mainmax.Size = New System.Drawing.Size(44, 42)
        Me.btn_Mainmax.TabIndex = 16
        Me.btn_Mainmax.TabStop = False
        Me.btn_Mainmax.Text = "☐"
        Me.btn_Mainmax.UseVisualStyleBackColor = False
        '
        'btn_Mainexit
        '
        Me.btn_Mainexit.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_Mainexit.BackColor = System.Drawing.Color.Transparent
        Me.btn_Mainexit.FlatAppearance.BorderSize = 0
        Me.btn_Mainexit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Maroon
        Me.btn_Mainexit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btn_Mainexit.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_Mainexit.Font = New System.Drawing.Font("Segoe UI Symbol", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btn_Mainexit.ForeColor = System.Drawing.Color.White
        Me.btn_Mainexit.Location = New System.Drawing.Point(943, 0)
        Me.btn_Mainexit.Margin = New System.Windows.Forms.Padding(0)
        Me.btn_Mainexit.Name = "btn_Mainexit"
        Me.btn_Mainexit.Size = New System.Drawing.Size(57, 42)
        Me.btn_Mainexit.TabIndex = 15
        Me.btn_Mainexit.TabStop = False
        Me.btn_Mainexit.Text = "✕"
        Me.btn_Mainexit.UseVisualStyleBackColor = False
        '
        'topbar_icon
        '
        Me.topbar_icon.Image = Global.CompactGUI.My.Resources.Resources.iconbright
        Me.topbar_icon.Location = New System.Drawing.Point(11, 25)
        Me.topbar_icon.Name = "topbar_icon"
        Me.topbar_icon.Size = New System.Drawing.Size(25, 25)
        Me.topbar_icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.topbar_icon.TabIndex = 15
        Me.topbar_icon.TabStop = False
        '
        'progressTimer
        '
        Me.progressTimer.Interval = 20
        '
        'seecompest
        '
        Me.seecompest.AutoSize = True
        Me.seecompest.Font = New System.Drawing.Font("Segoe UI Symbol", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.seecompest.ForeColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.seecompest.Location = New System.Drawing.Point(316, 96)
        Me.seecompest.Name = "seecompest"
        Me.seecompest.Size = New System.Drawing.Size(16, 13)
        Me.seecompest.TabIndex = 21
        Me.seecompest.Text = "❯ "
        Me.seecompest.Visible = False
        '
        'ToolTipFilesCompressed
        '
        Me.ToolTipFilesCompressed.AutoPopDelay = 12000
        Me.ToolTipFilesCompressed.BackColor = System.Drawing.Color.White
        Me.ToolTipFilesCompressed.ForeColor = System.Drawing.SystemColors.WindowFrame
        Me.ToolTipFilesCompressed.InitialDelay = 200
        Me.ToolTipFilesCompressed.IsBalloon = True
        Me.ToolTipFilesCompressed.ReshowDelay = 100
        Me.ToolTipFilesCompressed.ShowAlways = True
        Me.ToolTipFilesCompressed.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info
        Me.ToolTipFilesCompressed.ToolTipTitle = "Information"
        '
        'help_resultsFilesCompressed
        '
        Me.help_resultsFilesCompressed.AutoSize = True
        Me.help_resultsFilesCompressed.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.help_resultsFilesCompressed.ForeColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.help_resultsFilesCompressed.Location = New System.Drawing.Point(480, 168)
        Me.help_resultsFilesCompressed.Margin = New System.Windows.Forms.Padding(0, 1, 0, 0)
        Me.help_resultsFilesCompressed.Name = "help_resultsFilesCompressed"
        Me.help_resultsFilesCompressed.Size = New System.Drawing.Size(22, 17)
        Me.help_resultsFilesCompressed.TabIndex = 31
        Me.help_resultsFilesCompressed.Text = "(?)"
        Me.ToolTipFilesCompressed.SetToolTip(Me.help_resultsFilesCompressed, resources.GetString("help_resultsFilesCompressed.ToolTip"))
        '
        'sb_lblGameIssues
        '
        Me.sb_lblGameIssues.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.sb_lblGameIssues.ForeColor = System.Drawing.Color.FromArgb(CType(CType(220, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.sb_lblGameIssues.Location = New System.Drawing.Point(189, 153)
        Me.sb_lblGameIssues.Name = "sb_lblGameIssues"
        Me.sb_lblGameIssues.Size = New System.Drawing.Size(132, 20)
        Me.sb_lblGameIssues.TabIndex = 23
        Me.sb_lblGameIssues.Text = "! Game Has Issues"
        Me.sb_lblGameIssues.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.ToolTipFilesCompressed.SetToolTip(Me.sb_lblGameIssues, "This game has issues and compression is not recommended. Click to go to the Wiki " &
        "for details. ")
        Me.sb_lblGameIssues.Visible = False
        '
        'compressX4
        '
        Me.compressX4.AutoSize = True
        Me.compressX4.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.compressX4.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.compressX4.Location = New System.Drawing.Point(37, 38)
        Me.compressX4.Name = "compressX4"
        Me.compressX4.Size = New System.Drawing.Size(77, 17)
        Me.compressX4.TabIndex = 9
        Me.compressX4.Text = "XPRESS 4K"
        Me.ToolTipFilesCompressed.SetToolTip(Me.compressX4, "Fastest, Low Compression")
        Me.compressX4.UseVisualStyleBackColor = True
        '
        'compressLZX
        '
        Me.compressLZX.AutoSize = True
        Me.compressLZX.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.compressLZX.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.compressLZX.Location = New System.Drawing.Point(37, 128)
        Me.compressLZX.Name = "compressLZX"
        Me.compressLZX.Size = New System.Drawing.Size(45, 17)
        Me.compressLZX.TabIndex = 12
        Me.compressLZX.Text = "LZX "
        Me.ToolTipFilesCompressed.SetToolTip(Me.compressLZX, "Slowest, Very High Compression (Not Recommended for Games/Programs)")
        Me.compressLZX.UseVisualStyleBackColor = True
        '
        'compressX8
        '
        Me.compressX8.AutoSize = True
        Me.compressX8.Checked = True
        Me.compressX8.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.compressX8.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.compressX8.Location = New System.Drawing.Point(37, 68)
        Me.compressX8.Name = "compressX8"
        Me.compressX8.Size = New System.Drawing.Size(77, 17)
        Me.compressX8.TabIndex = 10
        Me.compressX8.TabStop = True
        Me.compressX8.Text = "XPRESS 8K"
        Me.ToolTipFilesCompressed.SetToolTip(Me.compressX8, "Fast, Medium Compression (Recommended)")
        Me.compressX8.UseVisualStyleBackColor = True
        '
        'compressX16
        '
        Me.compressX16.AutoSize = True
        Me.compressX16.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.compressX16.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.compressX16.Location = New System.Drawing.Point(37, 98)
        Me.compressX16.Name = "compressX16"
        Me.compressX16.Size = New System.Drawing.Size(83, 17)
        Me.compressX16.TabIndex = 11
        Me.compressX16.Text = "XPRESS 16K"
        Me.ToolTipFilesCompressed.SetToolTip(Me.compressX16, "Slow, High Compression")
        Me.compressX16.UseVisualStyleBackColor = True
        '
        'checkForceCompression
        '
        Me.checkForceCompression.AutoSize = True
        Me.checkForceCompression.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.checkForceCompression.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.checkForceCompression.Location = New System.Drawing.Point(37, 68)
        Me.checkForceCompression.Name = "checkForceCompression"
        Me.checkForceCompression.Size = New System.Drawing.Size(133, 17)
        Me.checkForceCompression.TabIndex = 7
        Me.checkForceCompression.Text = "Force Action on Files"
        Me.ToolTipFilesCompressed.SetToolTip(Me.checkForceCompression, resources.GetString("checkForceCompression.ToolTip"))
        Me.checkForceCompression.UseVisualStyleBackColor = True
        '
        'checkHiddenFiles
        '
        Me.checkHiddenFiles.AutoSize = True
        Me.checkHiddenFiles.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.checkHiddenFiles.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.checkHiddenFiles.Location = New System.Drawing.Point(37, 98)
        Me.checkHiddenFiles.Name = "checkHiddenFiles"
        Me.checkHiddenFiles.Size = New System.Drawing.Size(192, 17)
        Me.checkHiddenFiles.TabIndex = 6
        Me.checkHiddenFiles.Text = "Process Hidden and System Files"
        Me.ToolTipFilesCompressed.SetToolTip(Me.checkHiddenFiles, resources.GetString("checkHiddenFiles.ToolTip"))
        Me.checkHiddenFiles.UseVisualStyleBackColor = True
        '
        'checkRecursiveScan
        '
        Me.checkRecursiveScan.AutoSize = True
        Me.checkRecursiveScan.Checked = True
        Me.checkRecursiveScan.CheckState = System.Windows.Forms.CheckState.Checked
        Me.checkRecursiveScan.Enabled = False
        Me.checkRecursiveScan.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.checkRecursiveScan.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.checkRecursiveScan.Location = New System.Drawing.Point(37, 38)
        Me.checkRecursiveScan.Name = "checkRecursiveScan"
        Me.checkRecursiveScan.Size = New System.Drawing.Size(135, 17)
        Me.checkRecursiveScan.TabIndex = 5
        Me.checkRecursiveScan.Text = "Compress Subfolders"
        Me.ToolTipFilesCompressed.SetToolTip(Me.checkRecursiveScan, "This option is now checked by default and cannot be changed. ")
        Me.checkRecursiveScan.UseVisualStyleBackColor = True
        '
        'panel_MainWindow
        '
        Me.panel_MainWindow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.panel_MainWindow.Controls.Add(Me.panel_topBar)
        Me.panel_MainWindow.Controls.Add(Me.sb_Panel)
        Me.panel_MainWindow.Controls.Add(Me.vis_dropshadowMain)
        Me.panel_MainWindow.Controls.Add(Me.TabControl1)
        Me.panel_MainWindow.Dock = System.Windows.Forms.DockStyle.Fill
        Me.panel_MainWindow.Location = New System.Drawing.Point(0, 0)
        Me.panel_MainWindow.Name = "panel_MainWindow"
        Me.panel_MainWindow.Size = New System.Drawing.Size(1002, 652)
        Me.panel_MainWindow.TabIndex = 31
        '
        'sb_Panel
        '
        Me.sb_Panel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.sb_Panel.BackColor = System.Drawing.Color.FromArgb(CType(CType(43, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(75, Byte), Integer))
        Me.sb_Panel.Controls.Add(Me.TableLayoutPanel2)
        Me.sb_Panel.Controls.Add(Me.sb_lblGameIssues)
        Me.sb_Panel.Controls.Add(Me.seecompest)
        Me.sb_Panel.Controls.Add(Me.wkPostSizeUnit)
        Me.sb_Panel.Controls.Add(Me.wkPostSizeVal)
        Me.sb_Panel.Controls.Add(Me.wkPreSizeUnit)
        Me.sb_Panel.Controls.Add(Me.TableLayoutPanel3)
        Me.sb_Panel.Controls.Add(Me.sb_FolderName)
        Me.sb_Panel.Controls.Add(Me.vis_dropshadowmain2)
        Me.sb_Panel.Controls.Add(Me.wkPreSizeVal)
        Me.sb_Panel.Controls.Add(Me.btnCompress)
        Me.sb_Panel.Controls.Add(Me.btnUncompress)
        Me.sb_Panel.Location = New System.Drawing.Point(647, 135)
        Me.sb_Panel.Margin = New System.Windows.Forms.Padding(0)
        Me.sb_Panel.Name = "sb_Panel"
        Me.sb_Panel.Padding = New System.Windows.Forms.Padding(20, 0, 20, 0)
        Me.sb_Panel.Size = New System.Drawing.Size(353, 515)
        Me.sb_Panel.TabIndex = 15
        Me.sb_Panel.Visible = False
        '
        'TableLayoutPanel2
        '
        Me.TableLayoutPanel2.ColumnCount = 1
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel2.Controls.Add(Me.sb_ResultsPanel, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.sb_AnalysisPanel, 0, 1)
        Me.TableLayoutPanel2.Location = New System.Drawing.Point(7, 173)
        Me.TableLayoutPanel2.Margin = New System.Windows.Forms.Padding(0)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        Me.TableLayoutPanel2.RowCount = 2
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel2.Size = New System.Drawing.Size(342, 251)
        Me.TableLayoutPanel2.TabIndex = 23
        '
        'sb_ResultsPanel
        '
        Me.sb_ResultsPanel.Controls.Add(Me.Label4)
        Me.sb_ResultsPanel.Controls.Add(Me.Label3)
        Me.sb_ResultsPanel.Controls.Add(Me.sb_compressedSizeVisual)
        Me.sb_ResultsPanel.Controls.Add(Me.Panel6)
        Me.sb_ResultsPanel.Location = New System.Drawing.Point(3, 3)
        Me.sb_ResultsPanel.Name = "sb_ResultsPanel"
        Me.sb_ResultsPanel.Padding = New System.Windows.Forms.Padding(40, 0, 40, 0)
        Me.sb_ResultsPanel.Size = New System.Drawing.Size(332, 141)
        Me.sb_ResultsPanel.TabIndex = 25
        Me.sb_ResultsPanel.Visible = False
        '
        'Label4
        '
        Me.Label4.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.Silver
        Me.Label4.Location = New System.Drawing.Point(201, 126)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(93, 13)
        Me.Label4.TabIndex = 26
        Me.Label4.Text = "Compressed Size"
        Me.Label4.Visible = False
        '
        'Label3
        '
        Me.Label3.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.Silver
        Me.Label3.Location = New System.Drawing.Point(50, 126)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(72, 13)
        Me.Label3.TabIndex = 26
        Me.Label3.Text = "Original Size"
        Me.Label3.Visible = False
        '
        'sb_compressedSizeVisual
        '
        Me.sb_compressedSizeVisual.BackColor = System.Drawing.Color.FromArgb(CType(CType(39, Byte), Integer), CType(CType(174, Byte), Integer), CType(CType(96, Byte), Integer))
        Me.sb_compressedSizeVisual.Location = New System.Drawing.Point(225, 5)
        Me.sb_compressedSizeVisual.Name = "sb_compressedSizeVisual"
        Me.sb_compressedSizeVisual.Size = New System.Drawing.Size(40, 113)
        Me.sb_compressedSizeVisual.TabIndex = 0
        '
        'Panel6
        '
        Me.Panel6.BackColor = System.Drawing.Color.FromArgb(CType(CType(211, Byte), Integer), CType(CType(84, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.Panel6.Location = New System.Drawing.Point(67, 5)
        Me.Panel6.Name = "Panel6"
        Me.Panel6.Size = New System.Drawing.Size(40, 113)
        Me.Panel6.TabIndex = 0
        '
        'sb_AnalysisPanel
        '
        Me.sb_AnalysisPanel.Controls.Add(Me.sb_progresslabel)
        Me.sb_AnalysisPanel.Controls.Add(Me.sb_progressbar)
        Me.sb_AnalysisPanel.Controls.Add(Me.sb_progresspercent)
        Me.sb_AnalysisPanel.Location = New System.Drawing.Point(3, 150)
        Me.sb_AnalysisPanel.Name = "sb_AnalysisPanel"
        Me.sb_AnalysisPanel.Size = New System.Drawing.Size(332, 96)
        Me.sb_AnalysisPanel.TabIndex = 24
        Me.sb_AnalysisPanel.Visible = False
        '
        'sb_progresslabel
        '
        Me.sb_progresslabel.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.sb_progresslabel.ForeColor = System.Drawing.Color.White
        Me.sb_progresslabel.Location = New System.Drawing.Point(19, 32)
        Me.sb_progresslabel.Name = "sb_progresslabel"
        Me.sb_progresslabel.Size = New System.Drawing.Size(301, 20)
        Me.sb_progresslabel.TabIndex = 11
        Me.sb_progresslabel.Text = "Analysing..."
        Me.sb_progresslabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'sb_progressbar
        '
        Me.sb_progressbar.BackColor = System.Drawing.Color.FromArgb(CType(CType(39, Byte), Integer), CType(CType(174, Byte), Integer), CType(CType(96, Byte), Integer))
        Me.sb_progressbar.Location = New System.Drawing.Point(19, 55)
        Me.sb_progressbar.Name = "sb_progressbar"
        Me.sb_progressbar.Size = New System.Drawing.Size(301, 14)
        Me.sb_progressbar.TabIndex = 10
        '
        'sb_progresspercent
        '
        Me.sb_progresspercent.BackColor = System.Drawing.Color.Transparent
        Me.sb_progresspercent.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.sb_progresspercent.ForeColor = System.Drawing.Color.White
        Me.sb_progresspercent.Location = New System.Drawing.Point(107, 72)
        Me.sb_progresspercent.Name = "sb_progresspercent"
        Me.sb_progresspercent.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.sb_progresspercent.Size = New System.Drawing.Size(122, 14)
        Me.sb_progresspercent.TabIndex = 22
        Me.sb_progresspercent.Text = "0%"
        Me.sb_progresspercent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'wkPostSizeUnit
        '
        Me.wkPostSizeUnit.AutoSize = True
        Me.wkPostSizeUnit.BackColor = System.Drawing.Color.Transparent
        Me.wkPostSizeUnit.Font = New System.Drawing.Font("Segoe UI", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.wkPostSizeUnit.ForeColor = System.Drawing.Color.White
        Me.wkPostSizeUnit.Location = New System.Drawing.Point(273, 128)
        Me.wkPostSizeUnit.Name = "wkPostSizeUnit"
        Me.wkPostSizeUnit.Size = New System.Drawing.Size(16, 12)
        Me.wkPostSizeUnit.TabIndex = 9
        Me.wkPostSizeUnit.Text = "GB"
        Me.wkPostSizeUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'wkPostSizeVal
        '
        Me.wkPostSizeVal.BackColor = System.Drawing.Color.Transparent
        Me.wkPostSizeVal.Font = New System.Drawing.Font("Segoe UI", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.wkPostSizeVal.ForeColor = System.Drawing.Color.White
        Me.wkPostSizeVal.Location = New System.Drawing.Point(184, 112)
        Me.wkPostSizeVal.Name = "wkPostSizeVal"
        Me.wkPostSizeVal.Size = New System.Drawing.Size(143, 50)
        Me.wkPostSizeVal.TabIndex = 6
        Me.wkPostSizeVal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'wkPreSizeUnit
        '
        Me.wkPreSizeUnit.AutoSize = True
        Me.wkPreSizeUnit.BackColor = System.Drawing.Color.Transparent
        Me.wkPreSizeUnit.Font = New System.Drawing.Font("Segoe UI", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.wkPreSizeUnit.ForeColor = System.Drawing.Color.White
        Me.wkPreSizeUnit.Location = New System.Drawing.Point(115, 128)
        Me.wkPreSizeUnit.Name = "wkPreSizeUnit"
        Me.wkPreSizeUnit.Size = New System.Drawing.Size(16, 12)
        Me.wkPreSizeUnit.TabIndex = 8
        Me.wkPreSizeUnit.Text = "GB"
        Me.wkPreSizeUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TableLayoutPanel3
        '
        Me.TableLayoutPanel3.ColumnCount = 3
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 149.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150.0!))
        Me.TableLayoutPanel3.Controls.Add(Me.Label19, 0, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.sb_labelCompressed, 2, 0)
        Me.TableLayoutPanel3.Location = New System.Drawing.Point(23, 92)
        Me.TableLayoutPanel3.Name = "TableLayoutPanel3"
        Me.TableLayoutPanel3.RowCount = 1
        Me.TableLayoutPanel3.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel3.Size = New System.Drawing.Size(307, 20)
        Me.TableLayoutPanel3.TabIndex = 7
        '
        'Label19
        '
        Me.Label19.BackColor = System.Drawing.Color.Transparent
        Me.Label19.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label19.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label19.ForeColor = System.Drawing.Color.FromArgb(CType(CType(149, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(154, Byte), Integer))
        Me.Label19.Location = New System.Drawing.Point(0, 0)
        Me.Label19.Margin = New System.Windows.Forms.Padding(0)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(149, 20)
        Me.Label19.TabIndex = 8
        Me.Label19.Text = "Uncompressed"
        Me.Label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'sb_labelCompressed
        '
        Me.sb_labelCompressed.BackColor = System.Drawing.Color.Transparent
        Me.sb_labelCompressed.Dock = System.Windows.Forms.DockStyle.Fill
        Me.sb_labelCompressed.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.sb_labelCompressed.ForeColor = System.Drawing.Color.FromArgb(CType(CType(149, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(154, Byte), Integer))
        Me.sb_labelCompressed.Location = New System.Drawing.Point(157, 0)
        Me.sb_labelCompressed.Margin = New System.Windows.Forms.Padding(0)
        Me.sb_labelCompressed.Name = "sb_labelCompressed"
        Me.sb_labelCompressed.Size = New System.Drawing.Size(150, 20)
        Me.sb_labelCompressed.TabIndex = 8
        Me.sb_labelCompressed.Text = "Estimated Compressed"
        Me.sb_labelCompressed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'sb_FolderName
        '
        Me.sb_FolderName.BackColor = System.Drawing.Color.Transparent
        Me.sb_FolderName.Font = New System.Drawing.Font("Segoe UI", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.sb_FolderName.ForeColor = System.Drawing.Color.White
        Me.sb_FolderName.Location = New System.Drawing.Point(0, 17)
        Me.sb_FolderName.Name = "sb_FolderName"
        Me.sb_FolderName.Padding = New System.Windows.Forms.Padding(10, 0, 10, 0)
        Me.sb_FolderName.Size = New System.Drawing.Size(354, 60)
        Me.sb_FolderName.TabIndex = 6
        Me.sb_FolderName.Text = "Portal 2"
        Me.sb_FolderName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'vis_dropshadowmain2
        '
        Me.vis_dropshadowmain2.BackColor = System.Drawing.Color.Transparent
        Me.vis_dropshadowmain2.BackgroundImage = Global.CompactGUI.My.Resources.Resources.dsmask
        Me.vis_dropshadowmain2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.vis_dropshadowmain2.Location = New System.Drawing.Point(0, 0)
        Me.vis_dropshadowmain2.Name = "vis_dropshadowmain2"
        Me.vis_dropshadowmain2.Size = New System.Drawing.Size(353, 6)
        Me.vis_dropshadowmain2.TabIndex = 2
        '
        'wkPreSizeVal
        '
        Me.wkPreSizeVal.BackColor = System.Drawing.Color.Transparent
        Me.wkPreSizeVal.Font = New System.Drawing.Font("Segoe UI", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.wkPreSizeVal.ForeColor = System.Drawing.Color.White
        Me.wkPreSizeVal.Location = New System.Drawing.Point(23, 112)
        Me.wkPreSizeVal.Name = "wkPreSizeVal"
        Me.wkPreSizeVal.Size = New System.Drawing.Size(149, 50)
        Me.wkPreSizeVal.TabIndex = 6
        Me.wkPreSizeVal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnCompress
        '
        Me.btnCompress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnCompress.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.btnCompress.Enabled = False
        Me.btnCompress.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnCompress.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(72, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnCompress.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnCompress.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnCompress.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnCompress.ForeColor = System.Drawing.Color.Silver
        Me.btnCompress.Location = New System.Drawing.Point(71, 446)
        Me.btnCompress.Margin = New System.Windows.Forms.Padding(8, 3, 3, 3)
        Me.btnCompress.Name = "btnCompress"
        Me.btnCompress.Size = New System.Drawing.Size(208, 39)
        Me.btnCompress.TabIndex = 3
        Me.btnCompress.Text = "Compress Folder"
        Me.btnCompress.UseVisualStyleBackColor = False
        '
        'btnUncompress
        '
        Me.btnUncompress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnUncompress.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.btnUncompress.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnUncompress.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(72, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnUncompress.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnUncompress.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnUncompress.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.btnUncompress.ForeColor = System.Drawing.Color.White
        Me.btnUncompress.Location = New System.Drawing.Point(71, 446)
        Me.btnUncompress.Name = "btnUncompress"
        Me.btnUncompress.Size = New System.Drawing.Size(208, 39)
        Me.btnUncompress.TabIndex = 29
        Me.btnUncompress.Text = "Uncompress Folder"
        Me.btnUncompress.UseVisualStyleBackColor = False
        Me.btnUncompress.Visible = False
        '
        'vis_dropshadowMain
        '
        Me.vis_dropshadowMain.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.vis_dropshadowMain.BackColor = System.Drawing.Color.Transparent
        Me.vis_dropshadowMain.BackgroundImage = Global.CompactGUI.My.Resources.Resources.dsmask
        Me.vis_dropshadowMain.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.vis_dropshadowMain.Location = New System.Drawing.Point(0, 135)
        Me.vis_dropshadowMain.Name = "vis_dropshadowMain"
        Me.vis_dropshadowMain.Size = New System.Drawing.Size(998, 6)
        Me.vis_dropshadowMain.TabIndex = 16
        '
        'TabControl1
        '
        Me.TabControl1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TabControl1.Controls.Add(Me.InputPage)
        Me.TabControl1.Controls.Add(Me.ProgressPage)
        Me.TabControl1.Location = New System.Drawing.Point(0, 116)
        Me.TabControl1.MinimumSize = New System.Drawing.Size(503, 200)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(1003, 535)
        Me.TabControl1.TabIndex = 30
        '
        'InputPage
        '
        Me.InputPage.BackColor = System.Drawing.Color.White
        Me.InputPage.Controls.Add(Me.Panel1)
        Me.InputPage.Controls.Add(Me.FlowPanel_CompressionOptions)
        Me.InputPage.Location = New System.Drawing.Point(4, 22)
        Me.InputPage.Name = "InputPage"
        Me.InputPage.Padding = New System.Windows.Forms.Padding(3)
        Me.InputPage.Size = New System.Drawing.Size(995, 509)
        Me.InputPage.TabIndex = 0
        Me.InputPage.Text = "InputPage"
        '
        'Panel1
        '
        Me.Panel1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.SelectedFiles)
        Me.Panel1.Location = New System.Drawing.Point(44, 214)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(574, 287)
        Me.Panel1.TabIndex = 24
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 11.25!)
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.Label1.Location = New System.Drawing.Point(15, 35)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(99, 20)
        Me.Label1.TabIndex = 18
        Me.Label1.Text = "Selected Files"
        '
        'SelectedFiles
        '
        Me.SelectedFiles.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.SelectedFiles.BackColor = System.Drawing.Color.White
        Me.SelectedFiles.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.SelectedFiles.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable
        Me.SelectedFiles.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.SelectedFiles.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.SelectedFiles.FormattingEnabled = True
        Me.SelectedFiles.ItemHeight = 25
        Me.SelectedFiles.Location = New System.Drawing.Point(19, 59)
        Me.SelectedFiles.Name = "SelectedFiles"
        Me.SelectedFiles.Size = New System.Drawing.Size(555, 200)
        Me.SelectedFiles.TabIndex = 23
        Me.SelectedFiles.TabStop = False
        '
        'FlowPanel_CompressionOptions
        '
        Me.FlowPanel_CompressionOptions.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.FlowPanel_CompressionOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.FlowPanel_CompressionOptions.Controls.Add(Me.Panel4)
        Me.FlowPanel_CompressionOptions.Controls.Add(Me.Panel3)
        Me.FlowPanel_CompressionOptions.Location = New System.Drawing.Point(44, 21)
        Me.FlowPanel_CompressionOptions.Name = "FlowPanel_CompressionOptions"
        Me.FlowPanel_CompressionOptions.Size = New System.Drawing.Size(574, 174)
        Me.FlowPanel_CompressionOptions.TabIndex = 22
        '
        'Panel4
        '
        Me.Panel4.Controls.Add(Me.Label2)
        Me.Panel4.Controls.Add(Me.compressX4)
        Me.Panel4.Controls.Add(Me.compressLZX)
        Me.Panel4.Controls.Add(Me.compressX8)
        Me.Panel4.Controls.Add(Me.compressX16)
        Me.Panel4.Location = New System.Drawing.Point(3, 3)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(266, 160)
        Me.Panel4.TabIndex = 24
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI", 11.25!)
        Me.Label2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.Label2.Location = New System.Drawing.Point(12, 7)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(210, 20)
        Me.Label2.TabIndex = 18
        Me.Label2.Text = "Select Compression Algorithm"
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.comboChooseShutdown)
        Me.Panel3.Controls.Add(Me.checkShutdownOnCompletion)
        Me.Panel3.Controls.Add(Me.Label8)
        Me.Panel3.Controls.Add(Me.checkForceCompression)
        Me.Panel3.Controls.Add(Me.checkHiddenFiles)
        Me.Panel3.Controls.Add(Me.checkRecursiveScan)
        Me.FlowPanel_CompressionOptions.SetFlowBreak(Me.Panel3, True)
        Me.Panel3.Location = New System.Drawing.Point(275, 3)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(290, 160)
        Me.Panel3.TabIndex = 23
        '
        'comboChooseShutdown
        '
        Me.comboChooseShutdown.BackColor = System.Drawing.Color.White
        Me.comboChooseShutdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboChooseShutdown.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.comboChooseShutdown.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.comboChooseShutdown.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.comboChooseShutdown.FormattingEnabled = True
        Me.comboChooseShutdown.Items.AddRange(New Object() {"Shutdown", "Restart", "Sleep"})
        Me.comboChooseShutdown.Location = New System.Drawing.Point(52, 125)
        Me.comboChooseShutdown.Margin = New System.Windows.Forms.Padding(0)
        Me.comboChooseShutdown.Name = "comboChooseShutdown"
        Me.comboChooseShutdown.Size = New System.Drawing.Size(78, 21)
        Me.comboChooseShutdown.TabIndex = 34
        '
        'checkShutdownOnCompletion
        '
        Me.checkShutdownOnCompletion.AutoSize = True
        Me.checkShutdownOnCompletion.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.checkShutdownOnCompletion.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.checkShutdownOnCompletion.Location = New System.Drawing.Point(37, 128)
        Me.checkShutdownOnCompletion.Name = "checkShutdownOnCompletion"
        Me.checkShutdownOnCompletion.Size = New System.Drawing.Size(168, 17)
        Me.checkShutdownOnCompletion.TabIndex = 33
        Me.checkShutdownOnCompletion.Text = "                          PC on Finish"
        Me.checkShutdownOnCompletion.UseVisualStyleBackColor = True
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Segoe UI", 11.25!)
        Me.Label8.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.Label8.Location = New System.Drawing.Point(12, 7)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(199, 20)
        Me.Label8.TabIndex = 18
        Me.Label8.Text = "Select Additional Arguments"
        '
        'ProgressPage
        '
        Me.ProgressPage.BackColor = System.Drawing.Color.White
        Me.ProgressPage.Controls.Add(Me.TableLayoutPanel4)
        Me.ProgressPage.Controls.Add(Me.returnArrow)
        Me.ProgressPage.Controls.Add(Me.submitToWiki)
        Me.ProgressPage.Location = New System.Drawing.Point(4, 22)
        Me.ProgressPage.Name = "ProgressPage"
        Me.ProgressPage.Padding = New System.Windows.Forms.Padding(3)
        Me.ProgressPage.Size = New System.Drawing.Size(995, 509)
        Me.ProgressPage.TabIndex = 1
        Me.ProgressPage.Text = "ProgressPage"
        '
        'TableLayoutPanel4
        '
        Me.TableLayoutPanel4.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TableLayoutPanel4.AutoSize = True
        Me.TableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.TableLayoutPanel4.ColumnCount = 1
        Me.TableLayoutPanel4.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel4.Controls.Add(Me.CompResultsPanel, 0, 0)
        Me.TableLayoutPanel4.Controls.Add(Me.panel_console, 0, 1)
        Me.TableLayoutPanel4.Location = New System.Drawing.Point(7, 54)
        Me.TableLayoutPanel4.Name = "TableLayoutPanel4"
        Me.TableLayoutPanel4.RowCount = 2
        Me.TableLayoutPanel4.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel4.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel4.Size = New System.Drawing.Size(630, 447)
        Me.TableLayoutPanel4.TabIndex = 34
        '
        'CompResultsPanel
        '
        Me.CompResultsPanel.Controls.Add(Me.results_arc)
        Me.CompResultsPanel.Controls.Add(Me.help_resultsFilesCompressed)
        Me.CompResultsPanel.Controls.Add(Me.labelFilesCompressed)
        Me.CompResultsPanel.Controls.Add(Me.dirChosenLabel)
        Me.CompResultsPanel.Controls.Add(Me.TableLayoutPanel1)
        Me.CompResultsPanel.Controls.Add(Me.compressedSizeVisual)
        Me.CompResultsPanel.Controls.Add(Me.Panel5)
        Me.CompResultsPanel.Controls.Add(Me.Label9)
        Me.CompResultsPanel.Controls.Add(Me.Label11)
        Me.CompResultsPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.CompResultsPanel.Location = New System.Drawing.Point(3, 3)
        Me.CompResultsPanel.Name = "CompResultsPanel"
        Me.CompResultsPanel.Size = New System.Drawing.Size(624, 219)
        Me.CompResultsPanel.TabIndex = 31
        Me.CompResultsPanel.Visible = False
        '
        'results_arc
        '
        Me.results_arc.Location = New System.Drawing.Point(13, 63)
        Me.results_arc.Name = "results_arc"
        Me.results_arc.Size = New System.Drawing.Size(265, 122)
        Me.results_arc.TabIndex = 35
        '
        'labelFilesCompressed
        '
        Me.labelFilesCompressed.AutoSize = True
        Me.labelFilesCompressed.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelFilesCompressed.ForeColor = System.Drawing.Color.DimGray
        Me.labelFilesCompressed.Location = New System.Drawing.Point(284, 167)
        Me.labelFilesCompressed.Margin = New System.Windows.Forms.Padding(0)
        Me.labelFilesCompressed.MaximumSize = New System.Drawing.Size(0, 20)
        Me.labelFilesCompressed.MinimumSize = New System.Drawing.Size(150, 20)
        Me.labelFilesCompressed.Name = "labelFilesCompressed"
        Me.labelFilesCompressed.Size = New System.Drawing.Size(150, 20)
        Me.labelFilesCompressed.TabIndex = 30
        Me.labelFilesCompressed.Text = "n/n Files Compressed"
        '
        'dirChosenLabel
        '
        Me.dirChosenLabel.AutoSize = True
        Me.dirChosenLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.dirChosenLabel.ForeColor = System.Drawing.Color.DarkGray
        Me.dirChosenLabel.Location = New System.Drawing.Point(290, 196)
        Me.dirChosenLabel.Name = "dirChosenLabel"
        Me.dirChosenLabel.Size = New System.Drawing.Size(87, 15)
        Me.dirChosenLabel.TabIndex = 32
        Me.dirChosenLabel.Text = "dirchosenLabel"
        Me.dirChosenLabel.Visible = False
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.AutoSize = True
        Me.TableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(228, 50)
        Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 1
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(0, 0)
        Me.TableLayoutPanel1.TabIndex = 31
        '
        'compressedSizeVisual
        '
        Me.compressedSizeVisual.BackColor = System.Drawing.Color.FromArgb(CType(CType(95, Byte), Integer), CType(CType(190, Byte), Integer), CType(CType(123, Byte), Integer))
        Me.compressedSizeVisual.Controls.Add(Me.compressedSizeLabel)
        Me.compressedSizeVisual.Location = New System.Drawing.Point(288, 125)
        Me.compressedSizeVisual.Name = "compressedSizeVisual"
        Me.compressedSizeVisual.Size = New System.Drawing.Size(320, 25)
        Me.compressedSizeVisual.TabIndex = 29
        '
        'compressedSizeLabel
        '
        Me.compressedSizeLabel.AutoSize = True
        Me.compressedSizeLabel.Font = New System.Drawing.Font("Segoe UI", 10.5!)
        Me.compressedSizeLabel.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.compressedSizeLabel.Location = New System.Drawing.Point(1, 3)
        Me.compressedSizeLabel.Name = "compressedSizeLabel"
        Me.compressedSizeLabel.Size = New System.Drawing.Size(58, 19)
        Me.compressedSizeLabel.TabIndex = 27
        Me.compressedSizeLabel.Text = "220 MB"
        '
        'Panel5
        '
        Me.Panel5.BackColor = System.Drawing.Color.FromArgb(CType(CType(226, Byte), Integer), CType(CType(127, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.Panel5.Controls.Add(Me.origSizeLabel)
        Me.Panel5.Location = New System.Drawing.Point(288, 62)
        Me.Panel5.Name = "Panel5"
        Me.Panel5.Size = New System.Drawing.Size(320, 25)
        Me.Panel5.TabIndex = 28
        '
        'origSizeLabel
        '
        Me.origSizeLabel.AutoSize = True
        Me.origSizeLabel.Font = New System.Drawing.Font("Segoe UI", 10.5!)
        Me.origSizeLabel.ForeColor = System.Drawing.Color.White
        Me.origSizeLabel.Location = New System.Drawing.Point(1, 3)
        Me.origSizeLabel.Name = "origSizeLabel"
        Me.origSizeLabel.Size = New System.Drawing.Size(58, 19)
        Me.origSizeLabel.TabIndex = 27
        Me.origSizeLabel.Text = "300 MB"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.ForeColor = System.Drawing.Color.FromArgb(CType(CType(42, Byte), Integer), CType(CType(58, Byte), Integer), CType(CType(73, Byte), Integer))
        Me.Label9.Location = New System.Drawing.Point(284, 34)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(93, 20)
        Me.Label9.TabIndex = 25
        Me.Label9.Text = "Original Size"
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Font = New System.Drawing.Font("Segoe UI", 11.25!)
        Me.Label11.ForeColor = System.Drawing.Color.FromArgb(CType(CType(42, Byte), Integer), CType(CType(58, Byte), Integer), CType(CType(73, Byte), Integer))
        Me.Label11.Location = New System.Drawing.Point(284, 97)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(125, 20)
        Me.Label11.TabIndex = 26
        Me.Label11.Text = "Compressed Size:"
        '
        'panel_console
        '
        Me.panel_console.Controls.Add(Me.saveconlog)
        Me.panel_console.Controls.Add(Me.conOut)
        Me.panel_console.Dock = System.Windows.Forms.DockStyle.Fill
        Me.panel_console.Location = New System.Drawing.Point(3, 228)
        Me.panel_console.Name = "panel_console"
        Me.panel_console.Size = New System.Drawing.Size(624, 216)
        Me.panel_console.TabIndex = 33
        '
        'saveconlog
        '
        Me.saveconlog.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.saveconlog.BackColor = System.Drawing.Color.WhiteSmoke
        Me.saveconlog.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.saveconlog.Location = New System.Drawing.Point(533, 188)
        Me.saveconlog.Name = "saveconlog"
        Me.saveconlog.Size = New System.Drawing.Size(75, 23)
        Me.saveconlog.TabIndex = 33
        Me.saveconlog.Text = "Save Log"
        Me.saveconlog.UseVisualStyleBackColor = False
        '
        'conOut
        '
        Me.conOut.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.conOut.BackColor = System.Drawing.Color.White
        Me.conOut.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.conOut.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.conOut.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.conOut.FormattingEnabled = True
        Me.conOut.HorizontalScrollbar = True
        Me.conOut.ItemHeight = 15
        Me.conOut.Location = New System.Drawing.Point(13, 4)
        Me.conOut.Name = "conOut"
        Me.conOut.Size = New System.Drawing.Size(595, 165)
        Me.conOut.TabIndex = 30
        '
        'returnArrow
        '
        Me.returnArrow.AutoSize = True
        Me.returnArrow.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.returnArrow.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.returnArrow.Location = New System.Drawing.Point(11, 24)
        Me.returnArrow.Name = "returnArrow"
        Me.returnArrow.Size = New System.Drawing.Size(205, 20)
        Me.returnArrow.TabIndex = 22
        Me.returnArrow.Text = "❮   Return To Selection Screen"
        Me.returnArrow.Visible = False
        '
        'submitToWiki
        '
        Me.submitToWiki.AutoSize = True
        Me.submitToWiki.BackColor = System.Drawing.Color.WhiteSmoke
        Me.submitToWiki.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.submitToWiki.ForeColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.submitToWiki.Location = New System.Drawing.Point(439, 19)
        Me.submitToWiki.Name = "submitToWiki"
        Me.submitToWiki.Padding = New System.Windows.Forms.Padding(3, 5, 3, 5)
        Me.submitToWiki.Size = New System.Drawing.Size(179, 30)
        Me.submitToWiki.TabIndex = 33
        Me.submitToWiki.Text = "❯  Submit Results to Wiki"
        Me.submitToWiki.Visible = False
        '
        'spaceSavedLabel
        '
        Me.spaceSavedLabel.AutoSize = True
        Me.spaceSavedLabel.Font = New System.Drawing.Font("Segoe UI", 16.0!)
        Me.spaceSavedLabel.ForeColor = System.Drawing.Color.DimGray
        Me.spaceSavedLabel.Location = New System.Drawing.Point(1042, 298)
        Me.spaceSavedLabel.Name = "spaceSavedLabel"
        Me.spaceSavedLabel.Size = New System.Drawing.Size(147, 30)
        Me.spaceSavedLabel.TabIndex = 27
        Me.spaceSavedLabel.Text = "700MB Saved"
        '
        'TrayIcon
        '
        Me.TrayIcon.ContextMenuStrip = Me.TrayMenu
        Me.TrayIcon.Icon = CType(resources.GetObject("TrayIcon.Icon"), System.Drawing.Icon)
        Me.TrayIcon.Text = "CompactGUI"
        Me.TrayIcon.Visible = True
        '
        'TrayMenu
        '
        Me.TrayMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Tray_ShowMain})
        Me.TrayMenu.Name = "TrayMenu"
        Me.TrayMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional
        Me.TrayMenu.ShowImageMargin = False
        Me.TrayMenu.Size = New System.Drawing.Size(79, 26)
        '
        'Tray_ShowMain
        '
        Me.Tray_ShowMain.BackColor = System.Drawing.Color.White
        Me.Tray_ShowMain.Name = "Tray_ShowMain"
        Me.Tray_ShowMain.Size = New System.Drawing.Size(78, 22)
        Me.Tray_ShowMain.Text = "Show"
        '
        'Compact
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(1002, 652)
        Me.Controls.Add(Me.panel_MainWindow)
        Me.Controls.Add(Me.spaceSavedLabel)
        Me.DoubleBuffered = True
        Me.ForeColor = System.Drawing.Color.DimGray
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(600, 652)
        Me.Name = "Compact"
        Me.Text = "CompactGUI"
        Me.panel_topBar.ResumeLayout(False)
        Me.panel_topBar.PerformLayout()
        Me.updateBanner.ResumeLayout(False)
        Me.topbar_dirchooserContainer.ResumeLayout(False)
        CType(Me.topbar_icon, System.ComponentModel.ISupportInitialize).EndInit()
        Me.panel_MainWindow.ResumeLayout(False)
        Me.sb_Panel.ResumeLayout(False)
        Me.sb_Panel.PerformLayout()
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.sb_ResultsPanel.ResumeLayout(False)
        Me.sb_ResultsPanel.PerformLayout()
        Me.sb_AnalysisPanel.ResumeLayout(False)
        Me.TableLayoutPanel3.ResumeLayout(False)
        Me.TabControl1.ResumeLayout(False)
        Me.InputPage.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.FlowPanel_CompressionOptions.ResumeLayout(False)
        Me.Panel4.ResumeLayout(False)
        Me.Panel4.PerformLayout()
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.ProgressPage.ResumeLayout(False)
        Me.ProgressPage.PerformLayout()
        Me.TableLayoutPanel4.ResumeLayout(False)
        Me.CompResultsPanel.ResumeLayout(False)
        Me.CompResultsPanel.PerformLayout()
        Me.compressedSizeVisual.ResumeLayout(False)
        Me.compressedSizeVisual.PerformLayout()
        Me.Panel5.ResumeLayout(False)
        Me.Panel5.PerformLayout()
        Me.panel_console.ResumeLayout(False)
        Me.TrayMenu.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents topbar_title As Label
    Friend WithEvents panel_topBar As Panel
    Friend WithEvents dirChooser As LinkLabel
    Friend WithEvents progressTimer As Timer
    Friend WithEvents showinfopopup As Label
    Friend WithEvents btnAnalyze As Button
    Friend WithEvents ToolTipFilesCompressed As ToolTip
    Friend WithEvents seecompest As Label
    Friend WithEvents topbar_icon As PictureBox
    Friend WithEvents panel_MainWindow As Panel
    Friend WithEvents topbar_dirchooserContainer As Panel
    Friend WithEvents btn_Mainmin As Button
    Friend WithEvents btn_Mainmax As Button
    Friend WithEvents btn_Mainexit As Button
    Friend WithEvents sb_Panel As Panel
    Friend WithEvents sb_progresslabel As Label
    Friend WithEvents sb_progressbar As Panel
    Friend WithEvents wkPostSizeUnit As Label
    Friend WithEvents wkPostSizeVal As Label
    Friend WithEvents wkPreSizeUnit As Label
    Friend WithEvents TableLayoutPanel3 As TableLayoutPanel
    Friend WithEvents Label19 As Label
    Friend WithEvents sb_labelCompressed As Label
    Friend WithEvents sb_FolderName As Label
    Friend WithEvents vis_dropshadowmain2 As Panel
    Friend WithEvents vis_dropshadowMain As Panel
    Friend WithEvents wkPreSizeVal As Label
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents InputPage As TabPage
    Friend WithEvents FlowPanel_CompressionOptions As FlowLayoutPanel
    Friend WithEvents Panel4 As Panel
    Friend WithEvents Label2 As Label
    Friend WithEvents compressX4 As RadioButton
    Friend WithEvents compressLZX As RadioButton
    Friend WithEvents compressX8 As RadioButton
    Friend WithEvents compressX16 As RadioButton
    Friend WithEvents Panel3 As Panel
    Friend WithEvents comboChooseShutdown As ComboBox
    Friend WithEvents checkShutdownOnCompletion As CheckBox
    Friend WithEvents Label8 As Label
    Friend WithEvents checkForceCompression As CheckBox
    Friend WithEvents checkHiddenFiles As CheckBox
    Friend WithEvents checkRecursiveScan As CheckBox
    Friend WithEvents btnCompress As Button
    Friend WithEvents ProgressPage As TabPage
    Friend WithEvents returnArrow As Label
    Friend WithEvents panel_console As Panel
    Friend WithEvents saveconlog As Button
    Friend WithEvents conOut As ListBox
    Friend WithEvents CompResultsPanel As Panel
    Friend WithEvents submitToWiki As Label
    Friend WithEvents dirChosenLabel As Label
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents help_resultsFilesCompressed As Label
    Friend WithEvents labelFilesCompressed As Label
    Friend WithEvents compressedSizeVisual As Panel
    Friend WithEvents compressedSizeLabel As Label
    Friend WithEvents Panel5 As Panel
    Friend WithEvents origSizeLabel As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents spaceSavedLabel As Label
    Friend WithEvents btnUncompress As Button
    Friend WithEvents sb_progresspercent As Label
    Friend WithEvents sb_lblGameIssues As Label
    Friend WithEvents sb_AnalysisPanel As Panel
    Friend WithEvents sb_ResultsPanel As Panel
    Friend WithEvents Panel6 As Panel
    Friend WithEvents Label4 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents sb_compressedSizeVisual As Panel
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents TableLayoutPanel4 As TableLayoutPanel
    Friend WithEvents updateBanner As Panel
    Friend WithEvents dlUpdateLink As LinkLabel
    Friend WithEvents results_arc As GraphicsPanel
    Friend WithEvents TrayIcon As NotifyIcon
    Friend WithEvents TrayMenu As ContextMenuStrip
    Friend WithEvents Tray_ShowMain As ToolStripMenuItem
    Friend WithEvents SelectedFiles As ListBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Panel1 As Panel
End Class
