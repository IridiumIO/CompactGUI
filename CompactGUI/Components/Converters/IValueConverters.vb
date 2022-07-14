﻿Imports System.Globalization

Public Class DecimalToPercentageConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert

        'IF = invert and format, to show the "percentage smaller" text
        If parameter Is "IF" Then Return 100 - (CType(value, Decimal) * 100) & "%"

        If parameter Is "I" Then Return CInt(100 - (CType(value, Decimal) * 100))

        Return CInt(CType(value, Decimal) * 100)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class BytesToReadableConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim suf As String() = {" B", " KB", " MB", " GB", " TB", " PB", " EB"}

        If CStr(value) = "1010101010101010" Then Return "?"
        If CStr(value) = "0" Then Return "0" & suf(0)

        Dim bytes As Double = Math.Abs(CDbl(value))
        Dim place As Integer = CInt(Math.Floor(Math.Log(bytes, 1024)))
        Dim num As Double = Math.Round(bytes / Math.Pow(1024, place), 1)

        Return (Math.Sign(CDbl(value)) * num).ToString() & suf(place)
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
            Case Core.CompressionAlgorithm.LZNT1 : Return "NT"
            Case Core.CompressionAlgorithm.XPRESS4K : Return "X4"
            Case Core.CompressionAlgorithm.XPRESS8K : Return "X8"
            Case Core.CompressionAlgorithm.XPRESS16K : Return "X16"
            Case Core.CompressionAlgorithm.LZX : Return "LZX"
        End Select
        Return "NIL"
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
