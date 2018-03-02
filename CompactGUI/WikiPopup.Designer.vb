<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WikiPopup
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WikiPopup))
        Me.GamesTable = New System.Windows.Forms.TableLayoutPanel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.ImportantToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'GamesTable
        '
        Me.GamesTable.AutoSize = True
        Me.GamesTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.GamesTable.BackColor = System.Drawing.Color.Transparent
        Me.GamesTable.ColumnCount = 5
        Me.GamesTable.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.GamesTable.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80.0!))
        Me.GamesTable.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80.0!))
        Me.GamesTable.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70.0!))
        Me.GamesTable.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110.0!))
        Me.GamesTable.ForeColor = System.Drawing.Color.White
        Me.GamesTable.Location = New System.Drawing.Point(16, 69)
        Me.GamesTable.Name = "GamesTable"
        Me.GamesTable.RowCount = 1
        Me.GamesTable.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35.0!))
        Me.GamesTable.Size = New System.Drawing.Size(340, 35)
        Me.GamesTable.TabIndex = 7
        Me.GamesTable.Visible = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.White
        Me.Label1.Location = New System.Drawing.Point(11, 27)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(131, 25)
        Me.Label1.TabIndex = 8
        Me.Label1.Text = "Online Results"
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.Transparent
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.GamesTable)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.ForeColor = System.Drawing.Color.Black
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(430, 375)
        Me.Panel1.TabIndex = 9
        '
        'ImportantToolTip
        '
        Me.ImportantToolTip.AutomaticDelay = 100
        Me.ImportantToolTip.AutoPopDelay = 3000
        Me.ImportantToolTip.BackColor = System.Drawing.Color.White
        Me.ImportantToolTip.InitialDelay = 100
        Me.ImportantToolTip.ReshowDelay = 20
        Me.ImportantToolTip.ShowAlways = True
        Me.ImportantToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info
        '
        'WikiPopup
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(43, Byte), Integer), CType(CType(60, Byte), Integer), CType(CType(75, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(430, 375)
        Me.Controls.Add(Me.Panel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "WikiPopup"
        Me.Opacity = 0R
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "Form2"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents GamesTable As TableLayoutPanel
    Friend WithEvents Label1 As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents ImportantToolTip As ToolTip
End Class
