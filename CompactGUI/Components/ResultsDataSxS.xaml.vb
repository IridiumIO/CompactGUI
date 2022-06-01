Public Class ResultsDataSxS



    Public ReadOnly leftValueProperty As DependencyProperty = DependencyProperty.Register("leftValue", GetType(String), GetType(ResultsDataSxS), New PropertyMetadata(Nothing))
    Public ReadOnly leftLabelProperty As DependencyProperty = DependencyProperty.Register("leftLabel", GetType(String), GetType(ResultsDataSxS), New PropertyMetadata(Nothing))
    Public ReadOnly rightValueProperty As DependencyProperty = DependencyProperty.Register("rightValue", GetType(String), GetType(ResultsDataSxS), New PropertyMetadata(Nothing))
    Public ReadOnly rightLabelProperty As DependencyProperty = DependencyProperty.Register("rightLabel", GetType(String), GetType(ResultsDataSxS), New PropertyMetadata(Nothing))

    Public Property leftValue As String
        Get
            Return GetValue(leftValueProperty)
        End Get
        Set(value As String)
            SetValue(leftValueProperty, value)
        End Set
    End Property

    Public Property leftLabel As String
        Get
            Return GetValue(leftLabelProperty)
        End Get
        Set(value As String)
            SetValue(leftLabelProperty, value)
        End Set
    End Property

    Public Property rightValue As String
        Get
            Return GetValue(rightValueProperty)
        End Get
        Set(value As String)
            SetValue(rightValueProperty, value)
        End Set
    End Property
    Public Property rightLabel As String
        Get
            Return GetValue(rightLabelProperty)
        End Get
        Set(value As String)
            SetValue(rightLabelProperty, value)
        End Set
    End Property


    Sub SetLeftValue(bytes As Long)
        SetValue(leftValueProperty, ConvertBytesToReadable(bytes))
    End Sub

    Sub SetRightValue(bytes As Long)
        If bytes = 1010101010101010 Then
            SetValue(rightValueProperty, "?")
            Return
        End If
        SetValue(rightValueProperty, ConvertBytesToReadable(bytes))
    End Sub

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub


    Private Function ConvertBytesToReadable(byteCount As Long) As String

        Dim suf As String() = {" B", " KB", " MB", " GB", " TB", " PB", " EB"}
        If byteCount = 0 Then Return "0" & suf(0)
        Dim bytes As Long = Math.Abs(byteCount)
        Dim place As Integer = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)))
        Dim num As Double = Math.Round(bytes / Math.Pow(1024, place), 1)

        Return (Math.Sign(byteCount) * num).ToString() & suf(place)

    End Function

End Class
