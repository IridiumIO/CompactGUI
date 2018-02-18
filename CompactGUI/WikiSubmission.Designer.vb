<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WikiSubmission
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WikiSubmission))
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.lbl_Title = New System.Windows.Forms.Label()
        Me.txtbox_Name = New System.Windows.Forms.TextBox()
        Me.lbl_GameorProgram = New System.Windows.Forms.Label()
        Me.lbl_SteamID = New System.Windows.Forms.Label()
        Me.lbl_optional = New System.Windows.Forms.Label()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.txtbox_SteamID = New System.Windows.Forms.NumericUpDown()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btn_NextPage = New System.Windows.Forms.Button()
        Me.btn_Cancel = New System.Windows.Forms.Button()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.Page1 = New System.Windows.Forms.TabPage()
        Me.Radio_Program = New System.Windows.Forms.RadioButton()
        Me.Radio_Game = New System.Windows.Forms.RadioButton()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Page2 = New System.Windows.Forms.TabPage()
        Me.panel_SteamID = New System.Windows.Forms.Panel()
        Me.Page3 = New System.Windows.Forms.TabPage()
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Panel1.SuspendLayout()
        CType(Me.txtbox_SteamID, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabControl1.SuspendLayout()
        Me.Page1.SuspendLayout()
        Me.Page2.SuspendLayout()
        Me.panel_SteamID.SuspendLayout()
        Me.Page3.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.Panel1.Controls.Add(Me.lbl_Title)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(590, 60)
        Me.Panel1.TabIndex = 0
        '
        'lbl_Title
        '
        Me.lbl_Title.AutoSize = True
        Me.lbl_Title.Font = New System.Drawing.Font("Segoe UI", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl_Title.ForeColor = System.Drawing.Color.White
        Me.lbl_Title.Location = New System.Drawing.Point(12, 18)
        Me.lbl_Title.Name = "lbl_Title"
        Me.lbl_Title.Size = New System.Drawing.Size(140, 30)
        Me.lbl_Title.TabIndex = 0
        Me.lbl_Title.Text = "Submit Result"
        '
        'txtbox_Name
        '
        Me.txtbox_Name.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.txtbox_Name.Location = New System.Drawing.Point(198, 47)
        Me.txtbox_Name.MaxLength = 100
        Me.txtbox_Name.Name = "txtbox_Name"
        Me.txtbox_Name.Size = New System.Drawing.Size(233, 25)
        Me.txtbox_Name.TabIndex = 1
        Me.ToolTip1.SetToolTip(Me.txtbox_Name, resources.GetString("txtbox_Name.ToolTip"))
        '
        'lbl_GameorProgram
        '
        Me.lbl_GameorProgram.AutoSize = True
        Me.lbl_GameorProgram.Font = New System.Drawing.Font("Segoe UI", 11.0!)
        Me.lbl_GameorProgram.Location = New System.Drawing.Point(37, 48)
        Me.lbl_GameorProgram.Name = "lbl_GameorProgram"
        Me.lbl_GameorProgram.Size = New System.Drawing.Size(95, 20)
        Me.lbl_GameorProgram.TabIndex = 2
        Me.lbl_GameorProgram.Text = "Game Name:"
        Me.ToolTip1.SetToolTip(Me.lbl_GameorProgram, resources.GetString("lbl_GameorProgram.ToolTip"))
        '
        'lbl_SteamID
        '
        Me.lbl_SteamID.AutoSize = True
        Me.lbl_SteamID.Font = New System.Drawing.Font("Segoe UI", 11.0!)
        Me.lbl_SteamID.Location = New System.Drawing.Point(41, 20)
        Me.lbl_SteamID.Name = "lbl_SteamID"
        Me.lbl_SteamID.Size = New System.Drawing.Size(73, 20)
        Me.lbl_SteamID.TabIndex = 2
        Me.lbl_SteamID.Text = "Steam ID:"
        Me.ToolTip1.SetToolTip(Me.lbl_SteamID, resources.GetString("lbl_SteamID.ToolTip"))
        '
        'lbl_optional
        '
        Me.lbl_optional.AutoSize = True
        Me.lbl_optional.Font = New System.Drawing.Font("Segoe UI", 8.0!)
        Me.lbl_optional.ForeColor = System.Drawing.Color.Maroon
        Me.lbl_optional.Location = New System.Drawing.Point(419, 25)
        Me.lbl_optional.Name = "lbl_optional"
        Me.lbl_optional.Size = New System.Drawing.Size(59, 13)
        Me.lbl_optional.TabIndex = 2
        Me.lbl_optional.Text = "(Optional)"
        Me.ToolTip1.SetToolTip(Me.lbl_optional, resources.GetString("lbl_optional.ToolTip"))
        '
        'ToolTip1
        '
        Me.ToolTip1.AutomaticDelay = 100
        Me.ToolTip1.AutoPopDelay = 10000
        Me.ToolTip1.BackColor = System.Drawing.Color.White
        Me.ToolTip1.ForeColor = System.Drawing.Color.Black
        Me.ToolTip1.InitialDelay = 100
        Me.ToolTip1.IsBalloon = True
        Me.ToolTip1.ReshowDelay = 20
        Me.ToolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info
        Me.ToolTip1.ToolTipTitle = "Help"
        '
        'txtbox_SteamID
        '
        Me.txtbox_SteamID.BackColor = System.Drawing.Color.White
        Me.txtbox_SteamID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtbox_SteamID.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.txtbox_SteamID.Location = New System.Drawing.Point(180, 20)
        Me.txtbox_SteamID.Maximum = New Decimal(New Integer() {10000000, 0, 0, 0})
        Me.txtbox_SteamID.Name = "txtbox_SteamID"
        Me.txtbox_SteamID.Size = New System.Drawing.Size(233, 25)
        Me.txtbox_SteamID.TabIndex = 4
        Me.ToolTip1.SetToolTip(Me.txtbox_SteamID, resources.GetString("txtbox_SteamID.ToolTip"))
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI", 11.0!)
        Me.Label2.Location = New System.Drawing.Point(31, 19)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(235, 20)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "The following data has been sent. "
        Me.ToolTip1.SetToolTip(Me.Label2, resources.GetString("Label2.ToolTip"))
        '
        'btn_NextPage
        '
        Me.btn_NextPage.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.btn_NextPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_NextPage.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.btn_NextPage.ForeColor = System.Drawing.Color.White
        Me.btn_NextPage.Location = New System.Drawing.Point(503, 5)
        Me.btn_NextPage.Name = "btn_NextPage"
        Me.btn_NextPage.Size = New System.Drawing.Size(75, 32)
        Me.btn_NextPage.TabIndex = 3
        Me.btn_NextPage.Text = "Next"
        Me.btn_NextPage.UseVisualStyleBackColor = False
        '
        'btn_Cancel
        '
        Me.btn_Cancel.BackColor = System.Drawing.Color.Maroon
        Me.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btn_Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btn_Cancel.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.btn_Cancel.ForeColor = System.Drawing.Color.White
        Me.btn_Cancel.Location = New System.Drawing.Point(420, 5)
        Me.btn_Cancel.Name = "btn_Cancel"
        Me.btn_Cancel.Size = New System.Drawing.Size(75, 32)
        Me.btn_Cancel.TabIndex = 3
        Me.btn_Cancel.Text = "Cancel"
        Me.btn_Cancel.UseVisualStyleBackColor = False
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.Page1)
        Me.TabControl1.Controls.Add(Me.Page2)
        Me.TabControl1.Controls.Add(Me.Page3)
        Me.TabControl1.Location = New System.Drawing.Point(-5, 37)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(602, 214)
        Me.TabControl1.TabIndex = 4
        '
        'Page1
        '
        Me.Page1.Controls.Add(Me.Radio_Program)
        Me.Page1.Controls.Add(Me.Radio_Game)
        Me.Page1.Controls.Add(Me.Label6)
        Me.Page1.Location = New System.Drawing.Point(4, 22)
        Me.Page1.Name = "Page1"
        Me.Page1.Padding = New System.Windows.Forms.Padding(3)
        Me.Page1.Size = New System.Drawing.Size(594, 188)
        Me.Page1.TabIndex = 2
        Me.Page1.Text = "Page1"
        Me.Page1.UseVisualStyleBackColor = True
        '
        'Radio_Program
        '
        Me.Radio_Program.AutoSize = True
        Me.Radio_Program.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Radio_Program.Location = New System.Drawing.Point(65, 117)
        Me.Radio_Program.Name = "Radio_Program"
        Me.Radio_Program.Size = New System.Drawing.Size(71, 19)
        Me.Radio_Program.TabIndex = 4
        Me.Radio_Program.TabStop = True
        Me.Radio_Program.Text = "Program"
        Me.Radio_Program.UseVisualStyleBackColor = True
        '
        'Radio_Game
        '
        Me.Radio_Game.AutoSize = True
        Me.Radio_Game.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Radio_Game.Location = New System.Drawing.Point(65, 85)
        Me.Radio_Game.Name = "Radio_Game"
        Me.Radio_Game.Size = New System.Drawing.Size(56, 19)
        Me.Radio_Game.TabIndex = 4
        Me.Radio_Game.TabStop = True
        Me.Radio_Game.Text = "Game"
        Me.Radio_Game.UseVisualStyleBackColor = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Segoe UI", 11.0!)
        Me.Label6.Location = New System.Drawing.Point(37, 48)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(288, 20)
        Me.Label6.TabIndex = 3
        Me.Label6.Text = "Are you submitting a game or a program?"
        '
        'Page2
        '
        Me.Page2.Controls.Add(Me.panel_SteamID)
        Me.Page2.Controls.Add(Me.txtbox_Name)
        Me.Page2.Controls.Add(Me.lbl_GameorProgram)
        Me.Page2.Location = New System.Drawing.Point(4, 22)
        Me.Page2.Name = "Page2"
        Me.Page2.Padding = New System.Windows.Forms.Padding(3)
        Me.Page2.Size = New System.Drawing.Size(594, 188)
        Me.Page2.TabIndex = 0
        Me.Page2.Text = "Page2"
        Me.Page2.UseVisualStyleBackColor = True
        '
        'panel_SteamID
        '
        Me.panel_SteamID.Controls.Add(Me.txtbox_SteamID)
        Me.panel_SteamID.Controls.Add(Me.lbl_SteamID)
        Me.panel_SteamID.Controls.Add(Me.lbl_optional)
        Me.panel_SteamID.Location = New System.Drawing.Point(18, 74)
        Me.panel_SteamID.Name = "panel_SteamID"
        Me.panel_SteamID.Size = New System.Drawing.Size(490, 60)
        Me.panel_SteamID.TabIndex = 3
        '
        'Page3
        '
        Me.Page3.Controls.Add(Me.ListView1)
        Me.Page3.Controls.Add(Me.Label2)
        Me.Page3.Location = New System.Drawing.Point(4, 22)
        Me.Page3.Name = "Page3"
        Me.Page3.Padding = New System.Windows.Forms.Padding(3)
        Me.Page3.Size = New System.Drawing.Size(594, 188)
        Me.Page3.TabIndex = 1
        Me.Page3.Text = "Page3"
        Me.Page3.UseVisualStyleBackColor = True
        '
        'ListView1
        '
        Me.ListView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2})
        Me.ListView1.Font = New System.Drawing.Font("Segoe UI", 7.0!)
        Me.ListView1.GridLines = True
        Me.ListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None
        Me.ListView1.Location = New System.Drawing.Point(35, 46)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(533, 136)
        Me.ListView1.TabIndex = 4
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Data"
        Me.ColumnHeader1.Width = 120
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Value"
        Me.ColumnHeader2.Width = 700
        '
        'Panel2
        '
        Me.Panel2.BackColor = System.Drawing.Color.White
        Me.Panel2.Controls.Add(Me.btn_NextPage)
        Me.Panel2.Controls.Add(Me.btn_Cancel)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel2.Location = New System.Drawing.Point(0, 244)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(590, 49)
        Me.Panel2.TabIndex = 5
        '
        'WikiSubmission
        '
        Me.AcceptButton = Me.btn_NextPage
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(47, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(83, Byte), Integer))
        Me.CancelButton = Me.btn_Cancel
        Me.ClientSize = New System.Drawing.Size(590, 293)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.TabControl1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "WikiSubmission"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "WikiSubmission"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.txtbox_SteamID, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabControl1.ResumeLayout(False)
        Me.Page1.ResumeLayout(False)
        Me.Page1.PerformLayout()
        Me.Page2.ResumeLayout(False)
        Me.Page2.PerformLayout()
        Me.panel_SteamID.ResumeLayout(False)
        Me.panel_SteamID.PerformLayout()
        Me.Page3.ResumeLayout(False)
        Me.Page3.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Panel1 As Panel
    Friend WithEvents lbl_Title As Label
    Friend WithEvents txtbox_Name As TextBox
    Friend WithEvents lbl_GameorProgram As Label
    Friend WithEvents lbl_SteamID As Label
    Friend WithEvents lbl_optional As Label
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents btn_NextPage As Button
    Friend WithEvents btn_Cancel As Button
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents Page2 As TabPage
    Friend WithEvents Page3 As TabPage
    Friend WithEvents Page1 As TabPage
    Friend WithEvents Radio_Program As RadioButton
    Friend WithEvents Radio_Game As RadioButton
    Friend WithEvents Label6 As Label
    Friend WithEvents Panel2 As Panel
    Friend WithEvents panel_SteamID As Panel
    Friend WithEvents txtbox_SteamID As NumericUpDown
    Friend WithEvents Label2 As Label
    Friend WithEvents ListView1 As ListView
    Friend WithEvents ColumnHeader1 As ColumnHeader
    Friend WithEvents ColumnHeader2 As ColumnHeader
End Class
