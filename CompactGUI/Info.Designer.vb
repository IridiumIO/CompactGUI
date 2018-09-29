<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Info
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Info))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.RichTextBox1 = New System.Windows.Forms.RichTextBox()
        Me.semVersion = New System.Windows.Forms.Label()
        Me.lbl_CheckUpdates = New System.Windows.Forms.LinkLabel()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.checkMinimisetoTray = New System.Windows.Forms.CheckBox()
        Me.checkEnableRCMenu = New System.Windows.Forms.CheckBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.panel_header = New System.Windows.Forms.Panel()
        Me.btn_Mainexit = New System.Windows.Forms.Button()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.link_Github = New System.Windows.Forms.LinkLabel()
        Me.InfoTabControl = New System.Windows.Forms.TabControl()
        Me.Tab_Features = New System.Windows.Forms.TabPage()
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.NumericUpDown1 = New System.Windows.Forms.NumericUpDown()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.btnDefaultNonCompressable = New System.Windows.Forms.Button()
        Me.btnSaveNonCompressable = New System.Windows.Forms.Button()
        Me.TxtBoxNonCompressable = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.checkShowNotifications = New System.Windows.Forms.CheckBox()
        Me.checkExperimentalBrowser = New System.Windows.Forms.CheckBox()
        Me.checkEnableNonCompressable = New System.Windows.Forms.CheckBox()
        Me.Tab_Licenses = New System.Windows.Forms.TabPage()
        Me.Tab_Help = New System.Windows.Forms.TabPage()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.btn_options = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.btn_help = New System.Windows.Forms.Button()
        Me.btn_licenses = New System.Windows.Forms.Button()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.CheckBoxPlaySound = New System.Windows.Forms.CheckBox()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.panel_header.SuspendLayout()
        Me.InfoTabControl.SuspendLayout()
        Me.Tab_Features.SuspendLayout()
        CType(Me.NumericUpDown1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Tab_Licenses.SuspendLayout()
        Me.Tab_Help.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI Semilight", 10.0!)
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.Label1.Location = New System.Drawing.Point(61, 70)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(164, 23)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "EXTERNAL LICENSES"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI Semibold", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.Black
        Me.Label2.Location = New System.Drawing.Point(92, 119)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(111, 23)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Ookii Dialogs"
        '
        'RichTextBox1
        '
        Me.RichTextBox1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.RichTextBox1.BackColor = System.Drawing.Color.White
        Me.RichTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.RichTextBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RichTextBox1.ForeColor = System.Drawing.SystemColors.WindowFrame
        Me.RichTextBox1.Location = New System.Drawing.Point(96, 154)
        Me.RichTextBox1.Margin = New System.Windows.Forms.Padding(0)
        Me.RichTextBox1.Name = "RichTextBox1"
        Me.RichTextBox1.ReadOnly = True
        Me.RichTextBox1.Size = New System.Drawing.Size(781, 254)
        Me.RichTextBox1.TabIndex = 2
        Me.RichTextBox1.TabStop = False
        Me.RichTextBox1.Text = resources.GetString("RichTextBox1.Text")
        '
        'semVersion
        '
        Me.semVersion.AutoSize = True
        Me.semVersion.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.semVersion.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.semVersion.ForeColor = System.Drawing.Color.Gainsboro
        Me.semVersion.Location = New System.Drawing.Point(4, 0)
        Me.semVersion.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.semVersion.Name = "semVersion"
        Me.semVersion.Size = New System.Drawing.Size(70, 28)
        Me.semVersion.TabIndex = 3
        Me.semVersion.Text = "V 1.2.2"
        '
        'lbl_CheckUpdates
        '
        Me.lbl_CheckUpdates.AutoSize = True
        Me.lbl_CheckUpdates.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.lbl_CheckUpdates.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_CheckUpdates.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.lbl_CheckUpdates.LinkColor = System.Drawing.SystemColors.MenuHighlight
        Me.lbl_CheckUpdates.Location = New System.Drawing.Point(82, 6)
        Me.lbl_CheckUpdates.Margin = New System.Windows.Forms.Padding(4, 6, 4, 0)
        Me.lbl_CheckUpdates.Name = "lbl_CheckUpdates"
        Me.lbl_CheckUpdates.Size = New System.Drawing.Size(125, 19)
        Me.lbl_CheckUpdates.TabIndex = 4
        Me.lbl_CheckUpdates.TabStop = True
        Me.lbl_CheckUpdates.Text = "Check For Updates"
        Me.lbl_CheckUpdates.Visible = False
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.AutoSize = True
        Me.TableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.TableLayoutPanel1.Controls.Add(Me.semVersion, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.lbl_CheckUpdates, 1, 0)
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(196, 39)
        Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(4)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 1
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(211, 28)
        Me.TableLayoutPanel1.TabIndex = 5
        '
        'checkMinimisetoTray
        '
        Me.checkMinimisetoTray.AutoSize = True
        Me.checkMinimisetoTray.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.checkMinimisetoTray.ForeColor = System.Drawing.Color.DimGray
        Me.checkMinimisetoTray.Location = New System.Drawing.Point(68, 375)
        Me.checkMinimisetoTray.Margin = New System.Windows.Forms.Padding(4)
        Me.checkMinimisetoTray.Name = "checkMinimisetoTray"
        Me.checkMinimisetoTray.Size = New System.Drawing.Size(197, 24)
        Me.checkMinimisetoTray.TabIndex = 2
        Me.checkMinimisetoTray.Text = "Hide to Tray on Minimise"
        Me.checkMinimisetoTray.UseVisualStyleBackColor = True
        '
        'checkEnableRCMenu
        '
        Me.checkEnableRCMenu.AutoSize = True
        Me.checkEnableRCMenu.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.checkEnableRCMenu.ForeColor = System.Drawing.Color.DimGray
        Me.checkEnableRCMenu.Location = New System.Drawing.Point(68, 447)
        Me.checkEnableRCMenu.Margin = New System.Windows.Forms.Padding(4)
        Me.checkEnableRCMenu.Name = "checkEnableRCMenu"
        Me.checkEnableRCMenu.Size = New System.Drawing.Size(251, 24)
        Me.checkEnableRCMenu.TabIndex = 2
        Me.checkEnableRCMenu.Text = "Add to Explorer Right-click Menu"
        Me.checkEnableRCMenu.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Segoe UI Semilight", 10.0!)
        Me.Label3.ForeColor = System.Drawing.Color.Black
        Me.Label3.Location = New System.Drawing.Point(61, 302)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(83, 23)
        Me.Label3.TabIndex = 1
        Me.Label3.Text = "SETTINGS"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.Label4.Font = New System.Drawing.Font("Segoe UI", 16.0!)
        Me.Label4.ForeColor = System.Drawing.Color.White
        Me.Label4.Location = New System.Drawing.Point(15, 31)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(169, 37)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "CompactGUI"
        '
        'panel_header
        '
        Me.panel_header.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.panel_header.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.panel_header.Controls.Add(Me.btn_Mainexit)
        Me.panel_header.Controls.Add(Me.Label4)
        Me.panel_header.Controls.Add(Me.TableLayoutPanel1)
        Me.panel_header.Location = New System.Drawing.Point(0, 0)
        Me.panel_header.Margin = New System.Windows.Forms.Padding(4)
        Me.panel_header.Name = "panel_header"
        Me.panel_header.Size = New System.Drawing.Size(1120, 94)
        Me.panel_header.TabIndex = 9
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
        Me.btn_Mainexit.Location = New System.Drawing.Point(1059, 0)
        Me.btn_Mainexit.Margin = New System.Windows.Forms.Padding(0)
        Me.btn_Mainexit.Name = "btn_Mainexit"
        Me.btn_Mainexit.Size = New System.Drawing.Size(60, 37)
        Me.btn_Mainexit.TabIndex = 16
        Me.btn_Mainexit.TabStop = False
        Me.btn_Mainexit.Text = "✕"
        Me.btn_Mainexit.UseVisualStyleBackColor = False
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(92, 119)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(304, 23)
        Me.Label5.TabIndex = 6
        Me.Label5.Text = "Help and information can be found on"
        '
        'link_Github
        '
        Me.link_Github.AutoSize = True
        Me.link_Github.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.link_Github.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.link_Github.LinkColor = System.Drawing.SystemColors.MenuHighlight
        Me.link_Github.Location = New System.Drawing.Point(92, 149)
        Me.link_Github.Margin = New System.Windows.Forms.Padding(4, 6, 4, 0)
        Me.link_Github.Name = "link_Github"
        Me.link_Github.Size = New System.Drawing.Size(81, 23)
        Me.link_Github.TabIndex = 5
        Me.link_Github.TabStop = True
        Me.link_Github.Text = "❯  Github"
        '
        'InfoTabControl
        '
        Me.InfoTabControl.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.InfoTabControl.Controls.Add(Me.Tab_Features)
        Me.InfoTabControl.Controls.Add(Me.Tab_Licenses)
        Me.InfoTabControl.Controls.Add(Me.Tab_Help)
        Me.InfoTabControl.Location = New System.Drawing.Point(172, 58)
        Me.InfoTabControl.Margin = New System.Windows.Forms.Padding(4)
        Me.InfoTabControl.Name = "InfoTabControl"
        Me.InfoTabControl.SelectedIndex = 0
        Me.InfoTabControl.Size = New System.Drawing.Size(952, 630)
        Me.InfoTabControl.TabIndex = 10
        '
        'Tab_Features
        '
        Me.Tab_Features.BackColor = System.Drawing.Color.White
        Me.Tab_Features.Controls.Add(Me.CheckBoxPlaySound)
        Me.Tab_Features.Controls.Add(Me.ComboBox1)
        Me.Tab_Features.Controls.Add(Me.Label10)
        Me.Tab_Features.Controls.Add(Me.NumericUpDown1)
        Me.Tab_Features.Controls.Add(Me.Label9)
        Me.Tab_Features.Controls.Add(Me.Label8)
        Me.Tab_Features.Controls.Add(Me.btnDefaultNonCompressable)
        Me.Tab_Features.Controls.Add(Me.btnSaveNonCompressable)
        Me.Tab_Features.Controls.Add(Me.TxtBoxNonCompressable)
        Me.Tab_Features.Controls.Add(Me.Label7)
        Me.Tab_Features.Controls.Add(Me.Label3)
        Me.Tab_Features.Controls.Add(Me.checkShowNotifications)
        Me.Tab_Features.Controls.Add(Me.checkMinimisetoTray)
        Me.Tab_Features.Controls.Add(Me.checkExperimentalBrowser)
        Me.Tab_Features.Controls.Add(Me.checkEnableNonCompressable)
        Me.Tab_Features.Controls.Add(Me.checkEnableRCMenu)
        Me.Tab_Features.Location = New System.Drawing.Point(4, 25)
        Me.Tab_Features.Margin = New System.Windows.Forms.Padding(4)
        Me.Tab_Features.Name = "Tab_Features"
        Me.Tab_Features.Padding = New System.Windows.Forms.Padding(4)
        Me.Tab_Features.Size = New System.Drawing.Size(944, 601)
        Me.Tab_Features.TabIndex = 0
        Me.Tab_Features.Text = "Features"
        '
        'ComboBox1
        '
        Me.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ComboBox1.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Items.AddRange(New Object() {"B", "KB", "MB", "GB"})
        Me.ComboBox1.Location = New System.Drawing.Point(760, 411)
        Me.ComboBox1.Name = "ComboBox1"
        Me.ComboBox1.Size = New System.Drawing.Size(60, 28)
        Me.ComboBox1.TabIndex = 9
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Label10.ForeColor = System.Drawing.Color.DimGray
        Me.Label10.Location = New System.Drawing.Point(524, 375)
        Me.Label10.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(168, 20)
        Me.Label10.TabIndex = 8
        Me.Label10.Text = "Ignore files smaller than"
        '
        'NumericUpDown1
        '
        Me.NumericUpDown1.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.NumericUpDown1.Location = New System.Drawing.Point(527, 411)
        Me.NumericUpDown1.Maximum = New Decimal(New Integer() {-1, -1, -1, 0})
        Me.NumericUpDown1.Name = "NumericUpDown1"
        Me.NumericUpDown1.Size = New System.Drawing.Size(227, 27)
        Me.NumericUpDown1.TabIndex = 7
        Me.NumericUpDown1.Tag = ""
        Me.NumericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.NumericUpDown1.ThousandsSeparator = True
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Segoe UI Semilight", 10.0!)
        Me.Label9.ForeColor = System.Drawing.Color.Black
        Me.Label9.Location = New System.Drawing.Point(61, 70)
        Me.Label9.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(261, 23)
        Me.Label9.TabIndex = 6
        Me.Label9.Text = "POORLY COMPRESSED FILETYPES"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Label8.ForeColor = System.Drawing.Color.DimGray
        Me.Label8.Location = New System.Drawing.Point(61, 528)
        Me.Label8.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(299, 20)
        Me.Label8.TabIndex = 5
        Me.Label8.Text = "* Requires a restart of CompactGUI to apply"
        '
        'btnDefaultNonCompressable
        '
        Me.btnDefaultNonCompressable.BackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnDefaultNonCompressable.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.btnDefaultNonCompressable.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDefaultNonCompressable.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.btnDefaultNonCompressable.ForeColor = System.Drawing.Color.White
        Me.btnDefaultNonCompressable.Location = New System.Drawing.Point(721, 106)
        Me.btnDefaultNonCompressable.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDefaultNonCompressable.Name = "btnDefaultNonCompressable"
        Me.btnDefaultNonCompressable.Size = New System.Drawing.Size(100, 37)
        Me.btnDefaultNonCompressable.TabIndex = 4
        Me.btnDefaultNonCompressable.Text = "Default"
        Me.btnDefaultNonCompressable.UseVisualStyleBackColor = False
        '
        'btnSaveNonCompressable
        '
        Me.btnSaveNonCompressable.BackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btnSaveNonCompressable.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.btnSaveNonCompressable.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSaveNonCompressable.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.btnSaveNonCompressable.ForeColor = System.Drawing.Color.White
        Me.btnSaveNonCompressable.Location = New System.Drawing.Point(721, 202)
        Me.btnSaveNonCompressable.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSaveNonCompressable.Name = "btnSaveNonCompressable"
        Me.btnSaveNonCompressable.Size = New System.Drawing.Size(100, 37)
        Me.btnSaveNonCompressable.TabIndex = 4
        Me.btnSaveNonCompressable.Text = "Save"
        Me.btnSaveNonCompressable.UseVisualStyleBackColor = False
        '
        'TxtBoxNonCompressable
        '
        Me.TxtBoxNonCompressable.AcceptsTab = True
        Me.TxtBoxNonCompressable.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TxtBoxNonCompressable.ForeColor = System.Drawing.Color.DimGray
        Me.TxtBoxNonCompressable.Location = New System.Drawing.Point(65, 106)
        Me.TxtBoxNonCompressable.Margin = New System.Windows.Forms.Padding(4)
        Me.TxtBoxNonCompressable.Multiline = True
        Me.TxtBoxNonCompressable.Name = "TxtBoxNonCompressable"
        Me.TxtBoxNonCompressable.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TxtBoxNonCompressable.Size = New System.Drawing.Size(605, 132)
        Me.TxtBoxNonCompressable.TabIndex = 3
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Segoe UI Semilight", 10.0!)
        Me.Label7.ForeColor = System.Drawing.Color.Black
        Me.Label7.Location = New System.Drawing.Point(523, 302)
        Me.Label7.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(124, 23)
        Me.Label7.TabIndex = 1
        Me.Label7.Text = "EXPERIMENTAL"
        '
        'checkShowNotifications
        '
        Me.checkShowNotifications.AutoSize = True
        Me.checkShowNotifications.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.checkShowNotifications.ForeColor = System.Drawing.Color.DimGray
        Me.checkShowNotifications.Location = New System.Drawing.Point(68, 411)
        Me.checkShowNotifications.Margin = New System.Windows.Forms.Padding(4)
        Me.checkShowNotifications.Name = "checkShowNotifications"
        Me.checkShowNotifications.Size = New System.Drawing.Size(267, 24)
        Me.checkShowNotifications.TabIndex = 2
        Me.checkShowNotifications.Text = "Show Notification when Completed"
        Me.checkShowNotifications.UseVisualStyleBackColor = True
        '
        'checkExperimentalBrowser
        '
        Me.checkExperimentalBrowser.AutoSize = True
        Me.checkExperimentalBrowser.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.checkExperimentalBrowser.ForeColor = System.Drawing.Color.DimGray
        Me.checkExperimentalBrowser.Location = New System.Drawing.Point(528, 340)
        Me.checkExperimentalBrowser.Margin = New System.Windows.Forms.Padding(4)
        Me.checkExperimentalBrowser.Name = "checkExperimentalBrowser"
        Me.checkExperimentalBrowser.Size = New System.Drawing.Size(278, 24)
        Me.checkExperimentalBrowser.TabIndex = 2
        Me.checkExperimentalBrowser.Text = "Use Experimental File/Folder Browser"
        Me.checkExperimentalBrowser.UseVisualStyleBackColor = True
        '
        'checkEnableNonCompressable
        '
        Me.checkEnableNonCompressable.AutoSize = True
        Me.checkEnableNonCompressable.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.checkEnableNonCompressable.ForeColor = System.Drawing.Color.DimGray
        Me.checkEnableNonCompressable.Location = New System.Drawing.Point(68, 340)
        Me.checkEnableNonCompressable.Margin = New System.Windows.Forms.Padding(4)
        Me.checkEnableNonCompressable.Name = "checkEnableNonCompressable"
        Me.checkEnableNonCompressable.Size = New System.Drawing.Size(288, 24)
        Me.checkEnableNonCompressable.TabIndex = 2
        Me.checkEnableNonCompressable.Text = "Skip Files that are Poorly Compressed *"
        Me.checkEnableNonCompressable.UseVisualStyleBackColor = True
        '
        'Tab_Licenses
        '
        Me.Tab_Licenses.BackColor = System.Drawing.Color.White
        Me.Tab_Licenses.Controls.Add(Me.RichTextBox1)
        Me.Tab_Licenses.Controls.Add(Me.Label1)
        Me.Tab_Licenses.Controls.Add(Me.Label2)
        Me.Tab_Licenses.Location = New System.Drawing.Point(4, 25)
        Me.Tab_Licenses.Margin = New System.Windows.Forms.Padding(4)
        Me.Tab_Licenses.Name = "Tab_Licenses"
        Me.Tab_Licenses.Padding = New System.Windows.Forms.Padding(4)
        Me.Tab_Licenses.Size = New System.Drawing.Size(944, 601)
        Me.Tab_Licenses.TabIndex = 1
        Me.Tab_Licenses.Text = "Licenses"
        '
        'Tab_Help
        '
        Me.Tab_Help.Controls.Add(Me.Label6)
        Me.Tab_Help.Controls.Add(Me.link_Github)
        Me.Tab_Help.Controls.Add(Me.Label5)
        Me.Tab_Help.Location = New System.Drawing.Point(4, 25)
        Me.Tab_Help.Margin = New System.Windows.Forms.Padding(4)
        Me.Tab_Help.Name = "Tab_Help"
        Me.Tab_Help.Padding = New System.Windows.Forms.Padding(4)
        Me.Tab_Help.Size = New System.Drawing.Size(944, 601)
        Me.Tab_Help.TabIndex = 2
        Me.Tab_Help.Text = "Help"
        Me.Tab_Help.UseVisualStyleBackColor = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Segoe UI Semilight", 10.0!)
        Me.Label6.ForeColor = System.Drawing.Color.FromArgb(CType(CType(48, Byte), Integer), CType(CType(67, Byte), Integer), CType(CType(84, Byte), Integer))
        Me.Label6.Location = New System.Drawing.Point(61, 70)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(45, 23)
        Me.Label6.TabIndex = 1
        Me.Label6.Text = "Help"
        '
        'btn_options
        '
        Me.btn_options.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_options.BackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btn_options.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.btn_options.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.btn_options.FlatAppearance.BorderSize = 0
        Me.btn_options.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btn_options.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btn_options.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_options.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.btn_options.ForeColor = System.Drawing.Color.White
        Me.btn_options.Location = New System.Drawing.Point(0, 0)
        Me.btn_options.Margin = New System.Windows.Forms.Padding(40, 4, 4, 4)
        Me.btn_options.Name = "btn_options"
        Me.btn_options.Padding = New System.Windows.Forms.Padding(7, 0, 0, 0)
        Me.btn_options.Size = New System.Drawing.Size(193, 48)
        Me.btn_options.TabIndex = 24
        Me.btn_options.Text = "Options"
        Me.btn_options.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btn_options.UseVisualStyleBackColor = False
        '
        'Panel1
        '
        Me.Panel1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Panel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(43, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(75, Byte), Integer))
        Me.Panel1.Controls.Add(Me.btn_help)
        Me.Panel1.Controls.Add(Me.btn_licenses)
        Me.Panel1.Controls.Add(Me.btn_options)
        Me.Panel1.Location = New System.Drawing.Point(0, 92)
        Me.Panel1.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(191, 641)
        Me.Panel1.TabIndex = 11
        '
        'btn_help
        '
        Me.btn_help.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_help.BackColor = System.Drawing.Color.FromArgb(CType(CType(43, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(75, Byte), Integer))
        Me.btn_help.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.btn_help.FlatAppearance.BorderSize = 0
        Me.btn_help.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btn_help.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btn_help.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_help.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.btn_help.ForeColor = System.Drawing.Color.White
        Me.btn_help.Location = New System.Drawing.Point(0, 98)
        Me.btn_help.Margin = New System.Windows.Forms.Padding(40, 4, 4, 4)
        Me.btn_help.Name = "btn_help"
        Me.btn_help.Padding = New System.Windows.Forms.Padding(7, 0, 0, 0)
        Me.btn_help.Size = New System.Drawing.Size(193, 48)
        Me.btn_help.TabIndex = 24
        Me.btn_help.Text = "Help"
        Me.btn_help.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btn_help.UseVisualStyleBackColor = False
        '
        'btn_licenses
        '
        Me.btn_licenses.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btn_licenses.BackColor = System.Drawing.Color.FromArgb(CType(CType(43, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(75, Byte), Integer))
        Me.btn_licenses.FlatAppearance.BorderColor = System.Drawing.Color.White
        Me.btn_licenses.FlatAppearance.BorderSize = 0
        Me.btn_licenses.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btn_licenses.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(121, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.btn_licenses.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_licenses.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.btn_licenses.ForeColor = System.Drawing.Color.White
        Me.btn_licenses.Location = New System.Drawing.Point(0, 49)
        Me.btn_licenses.Margin = New System.Windows.Forms.Padding(40, 4, 4, 4)
        Me.btn_licenses.Name = "btn_licenses"
        Me.btn_licenses.Padding = New System.Windows.Forms.Padding(7, 0, 0, 0)
        Me.btn_licenses.Size = New System.Drawing.Size(193, 48)
        Me.btn_licenses.TabIndex = 24
        Me.btn_licenses.Text = "External Licenses"
        Me.btn_licenses.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btn_licenses.UseVisualStyleBackColor = False
        '
        'Panel2
        '
        Me.Panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel2.Controls.Add(Me.panel_header)
        Me.Panel2.Controls.Add(Me.Panel1)
        Me.Panel2.Controls.Add(Me.InfoTabControl)
        Me.Panel2.Cursor = System.Windows.Forms.Cursors.Default
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel2.Location = New System.Drawing.Point(0, 0)
        Me.Panel2.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(1121, 663)
        Me.Panel2.TabIndex = 12
        '
        'CheckBoxPlaySound
        '
        Me.CheckBoxPlaySound.AutoSize = True
        Me.CheckBoxPlaySound.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.CheckBoxPlaySound.ForeColor = System.Drawing.Color.DimGray
        Me.CheckBoxPlaySound.Location = New System.Drawing.Point(528, 447)
        Me.CheckBoxPlaySound.Margin = New System.Windows.Forms.Padding(4)
        Me.CheckBoxPlaySound.Name = "CheckBoxPlaySound"
        Me.CheckBoxPlaySound.Size = New System.Drawing.Size(254, 24)
        Me.CheckBoxPlaySound.TabIndex = 10
        Me.CheckBoxPlaySound.Text = "Play Sound on Action Completion"
        Me.CheckBoxPlaySound.UseVisualStyleBackColor = True
        '
        'Info
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(1121, 663)
        Me.Controls.Add(Me.Panel2)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MinimumSize = New System.Drawing.Size(533, 369)
        Me.Name = "Info"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.panel_header.ResumeLayout(False)
        Me.panel_header.PerformLayout()
        Me.InfoTabControl.ResumeLayout(False)
        Me.Tab_Features.ResumeLayout(False)
        Me.Tab_Features.PerformLayout()
        CType(Me.NumericUpDown1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Tab_Licenses.ResumeLayout(False)
        Me.Tab_Licenses.PerformLayout()
        Me.Tab_Help.ResumeLayout(False)
        Me.Tab_Help.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents RichTextBox1 As RichTextBox
    Friend WithEvents semVersion As Label
    Friend WithEvents lbl_CheckUpdates As LinkLabel
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents checkEnableRCMenu As CheckBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents panel_header As Panel
    Friend WithEvents Label5 As Label
    Friend WithEvents link_Github As LinkLabel
    Friend WithEvents checkMinimisetoTray As CheckBox
    Friend WithEvents InfoTabControl As TabControl
    Friend WithEvents Tab_Features As TabPage
    Friend WithEvents Tab_Licenses As TabPage
    Friend WithEvents Tab_Help As TabPage
    Friend WithEvents Label6 As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents btn_options As Button
    Friend WithEvents btn_help As Button
    Friend WithEvents btn_licenses As Button
    Friend WithEvents Panel2 As Panel
    Friend WithEvents btn_Mainexit As Button
    Friend WithEvents checkShowNotifications As CheckBox
    Friend WithEvents Label7 As Label
    Friend WithEvents checkExperimentalBrowser As CheckBox
    Friend WithEvents TxtBoxNonCompressable As TextBox
    Friend WithEvents checkEnableNonCompressable As CheckBox
    Friend WithEvents btnDefaultNonCompressable As Button
    Friend WithEvents btnSaveNonCompressable As Button
    Friend WithEvents Label8 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents Label10 As Label
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents CheckBoxPlaySound As CheckBox
End Class
