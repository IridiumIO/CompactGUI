<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form2
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.GamesTable = New System.Windows.Forms.TableLayoutPanel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Panel5 = New System.Windows.Forms.Panel()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.wkPostSizeUnit = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.wkPreSizeUnit = New System.Windows.Forms.Label()
        Me.wkPreSizeVal = New System.Windows.Forms.Label()
        Me.wkPostSizeVal = New System.Windows.Forms.Label()
        Me.lblCompactIssues = New System.Windows.Forms.Label()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
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
        Me.GamesTable.Location = New System.Drawing.Point(16, 191)
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
        Me.Label1.Location = New System.Drawing.Point(11, 149)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(131, 25)
        Me.Label1.TabIndex = 8
        Me.Label1.Text = "Online Results"
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.Transparent
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Controls.Add(Me.Panel3)
        Me.Panel1.Controls.Add(Me.Panel5)
        Me.Panel1.Controls.Add(Me.Panel4)
        Me.Panel1.Controls.Add(Me.Panel2)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.GamesTable)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(430, 375)
        Me.Panel1.TabIndex = 9
        '
        'Panel5
        '
        Me.Panel5.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel5.Location = New System.Drawing.Point(13, 23)
        Me.Panel5.Name = "Panel5"
        Me.Panel5.Size = New System.Drawing.Size(404, 10)
        Me.Panel5.TabIndex = 10
        '
        'Panel4
        '
        Me.Panel4.Location = New System.Drawing.Point(8, 24)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(10, 108)
        Me.Panel4.TabIndex = 10
        '
        'Panel3
        '
        Me.Panel3.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel3.Location = New System.Drawing.Point(406, 28)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(10, 104)
        Me.Panel3.TabIndex = 10
        '
        'Panel2
        '
        Me.Panel2.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel2.Controls.Add(Me.lblCompactIssues)
        Me.Panel2.Controls.Add(Me.Label4)
        Me.Panel2.Controls.Add(Me.wkPostSizeUnit)
        Me.Panel2.Controls.Add(Me.Label3)
        Me.Panel2.Controls.Add(Me.wkPreSizeUnit)
        Me.Panel2.Controls.Add(Me.wkPreSizeVal)
        Me.Panel2.Controls.Add(Me.wkPostSizeVal)
        Me.Panel2.Location = New System.Drawing.Point(17, 32)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(390, 100)
        Me.Panel2.TabIndex = 9
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.ForestGreen
        Me.Label4.Location = New System.Drawing.Point(210, 9)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(170, 17)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Estimated Compressed Size"
        '
        'wkPostSizeUnit
        '
        Me.wkPostSizeUnit.AutoSize = True
        Me.wkPostSizeUnit.BackColor = System.Drawing.Color.Transparent
        Me.wkPostSizeUnit.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.wkPostSizeUnit.ForeColor = System.Drawing.Color.DimGray
        Me.wkPostSizeUnit.Location = New System.Drawing.Point(292, 34)
        Me.wkPostSizeUnit.Margin = New System.Windows.Forms.Padding(0)
        Me.wkPostSizeUnit.Name = "wkPostSizeUnit"
        Me.wkPostSizeUnit.Size = New System.Drawing.Size(24, 17)
        Me.wkPostSizeUnit.TabIndex = 8
        Me.wkPostSizeUnit.Text = "GB"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.Label3.Location = New System.Drawing.Point(16, 9)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(170, 17)
        Me.Label3.TabIndex = 8
        Me.Label3.Text = "Current Uncompressed Size"
        '
        'wkPreSizeUnit
        '
        Me.wkPreSizeUnit.AutoSize = True
        Me.wkPreSizeUnit.BackColor = System.Drawing.Color.Transparent
        Me.wkPreSizeUnit.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.wkPreSizeUnit.ForeColor = System.Drawing.Color.DimGray
        Me.wkPreSizeUnit.Location = New System.Drawing.Point(70, 34)
        Me.wkPreSizeUnit.Margin = New System.Windows.Forms.Padding(0)
        Me.wkPreSizeUnit.Name = "wkPreSizeUnit"
        Me.wkPreSizeUnit.Size = New System.Drawing.Size(24, 17)
        Me.wkPreSizeUnit.TabIndex = 8
        Me.wkPreSizeUnit.Text = "GB"
        '
        'wkPreSizeVal
        '
        Me.wkPreSizeVal.AutoSize = True
        Me.wkPreSizeVal.BackColor = System.Drawing.Color.Transparent
        Me.wkPreSizeVal.Font = New System.Drawing.Font("Segoe UI", 24.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.wkPreSizeVal.Location = New System.Drawing.Point(10, 26)
        Me.wkPreSizeVal.Margin = New System.Windows.Forms.Padding(0)
        Me.wkPreSizeVal.Name = "wkPreSizeVal"
        Me.wkPreSizeVal.Size = New System.Drawing.Size(71, 45)
        Me.wkPreSizeVal.TabIndex = 8
        Me.wkPreSizeVal.Text = "300"
        '
        'wkPostSizeVal
        '
        Me.wkPostSizeVal.AutoSize = True
        Me.wkPostSizeVal.BackColor = System.Drawing.Color.Transparent
        Me.wkPostSizeVal.Font = New System.Drawing.Font("Segoe UI", 24.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.wkPostSizeVal.Location = New System.Drawing.Point(205, 26)
        Me.wkPostSizeVal.Margin = New System.Windows.Forms.Padding(0)
        Me.wkPostSizeVal.Name = "wkPostSizeVal"
        Me.wkPostSizeVal.Size = New System.Drawing.Size(95, 45)
        Me.wkPostSizeVal.TabIndex = 8
        Me.wkPostSizeVal.Text = "210.7"
        '
        'lblCompactIssues
        '
        Me.lblCompactIssues.AutoSize = True
        Me.lblCompactIssues.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCompactIssues.ForeColor = System.Drawing.Color.DarkRed
        Me.lblCompactIssues.Location = New System.Drawing.Point(213, 75)
        Me.lblCompactIssues.Name = "lblCompactIssues"
        Me.lblCompactIssues.Size = New System.Drawing.Size(103, 15)
        Me.lblCompactIssues.TabIndex = 9
        Me.lblCompactIssues.Text = "! Game has issues"
        Me.lblCompactIssues.Visible = False
        '
        'Form2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.WhiteSmoke
        Me.ClientSize = New System.Drawing.Size(430, 375)
        Me.Controls.Add(Me.Panel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "Form2"
        Me.Opacity = 0R
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.Text = "Form2"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents GamesTable As TableLayoutPanel
    Friend WithEvents Label1 As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents wkPostSizeUnit As Label
    Friend WithEvents wkPreSizeUnit As Label
    Friend WithEvents wkPostSizeVal As Label
    Friend WithEvents wkPreSizeVal As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Panel5 As Panel
    Friend WithEvents Panel4 As Panel
    Friend WithEvents Panel3 As Panel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents lblCompactIssues As Label
End Class
