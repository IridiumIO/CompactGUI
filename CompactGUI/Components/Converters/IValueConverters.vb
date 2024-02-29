Imports System.Globalization

Public Class DecimalToPercentageConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert

        'IF = invert and format, to show the "percentage smaller" text
        If parameter = "IF" Then Return CInt(100 - (CType(value, Decimal) * 100)) & "%"

        If parameter = "I" Then Return CInt(100 - (CType(value, Decimal) * 100))

        Return CInt(CType(value, Decimal) * 100)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class BytesToReadableConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim suf As String() = {" B", " KB", " MB", " GB", " TB", " PB", " EB"}

        If value = 1010101010101010 Then Return "?"

        If value = 0 Then Return "0" & suf(0)
        Dim bytes As Long = Math.Abs(value)
        Dim place As Integer = CInt(Math.Floor(Math.Log(bytes, 1024)))
        Dim num As Double = Math.Round(bytes / Math.Pow(1024, place), 1)

        Return (Math.Sign(value) * num).ToString() & suf(place)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class StrippedFolderPathConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value Is Nothing Then Return Nothing
        Dim Str = CType(value, String)
        Return Str.Substring(Str.LastIndexOf("\") + 1)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class RelativeDateConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim dt = CType(value, DateTime)
        Dim ts As TimeSpan = DateTime.Now - dt

        If ts > TimeSpan.FromDays(2) Then
            Return String.Format("{0:0} days ago", ts.TotalDays)
        ElseIf ts > TimeSpan.FromHours(2) Then
            Return String.Format("{0:0} hours ago", ts.TotalHours)
        ElseIf ts > TimeSpan.FromMinutes(2) Then
            Return String.Format("{0:0} minutes ago", ts.TotalMinutes)
        Else
            Return "just now"

        End If


    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

Public Class CompressionLevelAbbreviatedConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim clvl = CType(value, Core.CompressionAlgorithm)
        Select Case clvl
            Case Core.CompressionAlgorithm.NO_COMPRESSION : Return "NIL"
            Case Core.CompressionAlgorithm.LZNT1 : Return "NT"
            Case Core.CompressionAlgorithm.XPRESS4K : Return "X4"
            Case Core.CompressionAlgorithm.XPRESS8K : Return "X8"
            Case Core.CompressionAlgorithm.XPRESS16K : Return "X16"
            Case Core.CompressionAlgorithm.LZX : Return "LZX"
        End Select
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

Public Class ConfidenceIntToStringConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Select Case value
            Case 0
                Return "▬"
            Case 1
                Return "▬▬"
            Case 2
                Return "▬▬▬"
            Case Else
                Return "▭▭▭"
        End Select
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

Public Class ConfidenceIntToColorConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Select Case value
            Case 0
                Return New SolidColorBrush(ColorConverter.ConvertFromString("#FF996B6B"))
            Case 1
                Return New SolidColorBrush(ColorConverter.ConvertFromString("#F1CE92"))
            Case 2
                Return New SolidColorBrush(ColorConverter.ConvertFromString("#92F1AB"))
            Case Else
                Return New SolidColorBrush(ColorConverter.ConvertFromString("#BAC2CA"))
        End Select
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

Public Class WindowScalingConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert

        Dim dimension = CInt(parameter)


        Return CInt(value * dimension)

    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class WikiCompressionLevelAbbreviatedConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim clvl = CType(value, Integer)
        Select Case clvl
            Case 0 : Return "X4"
            Case 1 : Return "X8"
            Case 2 : Return "X16"
            Case 3 : Return "LZX"
        End Select
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class RatioConverter : Implements IMultiValueConverter

    Public Function Convert(values As Object(), targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
        If values.Length <> 2 Then
            Throw New ArgumentException("Two values should be provided.")
        End If

        Dim afterBytes As Long
        Dim beforeBytes As Long

        If Not Long.TryParse(values(0).ToString(), afterBytes) OrElse Not Long.TryParse(values(1).ToString(), beforeBytes) Then
            Throw New ArgumentException("Both values should be of type double.")
        End If

        Dim ratio = Math.Round((1 - afterBytes / beforeBytes) * 100, 0)

        Return $"{ratio}%"
    End Function

    Public Function ConvertBack(value As Object, targetTypes As Type(), parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
        Throw New NotImplementedException("ConvertBack is not implemented.")
    End Function

End Class


Public Class NonZeroToVisConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim clvl = CType(value, Integer)
        If clvl = 0 Then Return Visibility.Collapsed

        Return Visibility.Visible
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
