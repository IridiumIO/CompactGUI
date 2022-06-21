Public Class ResultsDataSxS



    Public Shared ReadOnly LeftValueProperty As DependencyProperty = DependencyProperty.Register(NameOf(LeftValue), GetType(String), GetType(ResultsDataSxS), New PropertyMetadata(Nothing, Nothing, New CoerceValueCallback(AddressOf CoerceLeftValue)))



    Public ReadOnly leftLabelProperty As DependencyProperty = DependencyProperty.Register("leftLabel", GetType(String), GetType(ResultsDataSxS), New PropertyMetadata(String.Empty))
    Public Shared ReadOnly RightValueProperty As DependencyProperty = DependencyProperty.Register(NameOf(RightValue), GetType(String), GetType(ResultsDataSxS), New PropertyMetadata(Nothing, Nothing, New CoerceValueCallback(AddressOf CoerceRightValue)))


    Public ReadOnly rightLabelProperty As DependencyProperty = DependencyProperty.Register("rightLabel", GetType(String), GetType(ResultsDataSxS), New PropertyMetadata(String.Empty))


    Shared Function CoerceLeftValue(d As DependencyObject, baseValue As Object) As String
        Return ConvertBytesToReadable(baseValue)
    End Function
    Shared Function CoerceRightValue(d As DependencyObject, baseValue As Object) As String
        If baseValue = 1010101010101010 Then Return "?"
        Return ConvertBytesToReadable(baseValue)
    End Function


    Public Property LeftValue As String
        Get
            Return GetValue(LeftValueProperty)
        End Get
        Set(value As String)
            SetValue(LeftValueProperty, value)
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

    Public Property RightValue As String
        Get
            Return GetValue(RightValueProperty)
        End Get
        Set(value As String)
            SetValue(RightValueProperty, value)
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



    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub


    Shared Function ConvertBytesToReadable(byteCount As Long) As String

        Dim suf As String() = {" B", " KB", " MB", " GB", " TB", " PB", " EB"}
        If byteCount = 0 Then Return "0" & suf(0)
        Dim bytes As Long = Math.Abs(byteCount)
        Dim place As Integer = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)))
        Dim num As Double = Math.Round(bytes / Math.Pow(1024, place), 1)

        Return (Math.Sign(byteCount) * num).ToString() & suf(place)

    End Function

End Class
