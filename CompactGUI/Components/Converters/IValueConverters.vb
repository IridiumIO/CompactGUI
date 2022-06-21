Imports System.Globalization

Public Class DecimalToPercentageConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert

        'IF = invert and format, to show the "percentage smaller" text
        If parameter = "IF" Then
            Return CInt(100 - (CType(value, Decimal) * 100)) & "%"
        End If

        Return CInt(CType(value, Decimal) * 100)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
