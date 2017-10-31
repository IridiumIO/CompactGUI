Public Class WikiPopup
    Private Sub Form2_Hide(sender As Object, e As EventArgs) Handles Me.MouseLeave
        ' Me.Hide()
    End Sub

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint
        Dim p As New Pen(Color.Gray, 1)
        e.Graphics.DrawLine(p, 20, 100, GamesTable.Width + 10, 100)
    End Sub
End Class