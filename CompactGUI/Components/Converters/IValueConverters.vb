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

        Dim roundingPrecision As Integer = 1
        If parameter IsNot Nothing AndAlso Integer.TryParse(parameter.ToString(), roundingPrecision) Then
            roundingPrecision = Math.Max(0, roundingPrecision)
            'We want to round to 1 decimal place if the value is in the GB range or higher
            If Array.IndexOf(suf, suf(place)) > 2 AndAlso roundingPrecision = 0 Then
                roundingPrecision = 1
            End If
        End If

        Dim num As Double = Math.Round(bytes / Math.Pow(1024, place), roundingPrecision)

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
        Return Str.Substring(Str.LastIndexOf("\"c) + 1)
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class TokenisedFolderPathConverter : Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value Is Nothing Then Return Nothing
        Dim Str = CType(value, String)
        Dim formattedString = Str.Replace("\"c, " 🢒 ")
        Return formattedString
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class RelativeDateConverter : Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim dt = CType(value, DateTime)
        Dim ts As TimeSpan = DateTime.Now - dt

        If ts > TimeSpan.FromDays(19000) Then
            Return String.Format("Unknown")
        End If
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
        Dim clvl = CType(value, Core.WOFCompressionAlgorithm)
        Select Case clvl
            Case Core.WOFCompressionAlgorithm.NO_COMPRESSION : Return "NIL"
            Case Core.WOFCompressionAlgorithm.LZNT1 : Return "NT"
            Case Core.WOFCompressionAlgorithm.XPRESS4K : Return "X4"
            Case Core.WOFCompressionAlgorithm.XPRESS8K : Return "X8"
            Case Core.WOFCompressionAlgorithm.XPRESS16K : Return "X16"
            Case Core.WOFCompressionAlgorithm.LZX : Return "LZX"
            Case Else : Return "NIL"
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
            Case Else : Return "NIL"
        End Select

    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
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


Public Class ProgressBarColorConverter : Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim progress As Decimal = DirectCast(value, Decimal)

        If progress > 0.6 Then
            Return New SolidColorBrush(Color.FromRgb(239, 146, 146))
        ElseIf progress > 0.2 Then
            Return New SolidColorBrush(Color.FromRgb(239, 239, 146))
        Else
            Return New SolidColorBrush(Color.FromRgb(146, 241, 171))
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class BooleanToInverseVisibilityConverter : Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim b = CType(value, Boolean)
        If b Then Return Visibility.Collapsed
        Return Visibility.Visible
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

End Class

Public Class EnumToRadioButtonConverter : Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim enumValue = CType(value, [Enum])
        Dim parameterValue = CType(parameter, [Enum])
        Return enumValue.Equals(parameterValue)
    End Function
    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        If value Then
            Return parameter
        End If
        Return Binding.DoNothing
    End Function
End Class

Public Class FolderStatusToColorConverter : Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim status = CType(value, ActionState)
        Select Case status
            Case ActionState.Idle
                Return New SolidColorBrush(ColorConverter.ConvertFromString("#6E9DEF"))
            Case ActionState.Analysing, ActionState.Working, ActionState.Paused
                Return New SolidColorBrush(ColorConverter.ConvertFromString("#F1CE92"))
            Case ActionState.Results
                Return New SolidColorBrush(ColorConverter.ConvertFromString("#92F1AB"))
            Case Else
                Return New SolidColorBrush(ColorConverter.ConvertFromString("#FFBAC2CA"))
        End Select
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

Public Class FolderStatusToStringConverter : Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim status = CType(value, ActionState)
        Select Case status
            Case ActionState.Idle
                Return "Awaiting Compression"
            Case ActionState.Analysing
                Return "Analysing"
            Case ActionState.Working, ActionState.Paused
                Return "Working"
            Case ActionState.Results
                Return "Compressed"
            Case Else
                Return "Unknown"
        End Select
    End Function
    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class



Public Class FolderWorkingStateToPauseSymbolConverter : Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim status = CType(value, ActionState)
        Select Case status
            Case ActionState.Paused
                Return "Play12"
            Case Else
                Return "Pause12"
        End Select
    End Function
    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class IsSteamFolderConverter : Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim folder = CType(value, CompressableFolder)
        If folder Is Nothing Then Return False
        Return TypeOf (folder) Is SteamFolder
    End Function
    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class IsSteamFolderAndFreshlyCompressedMultiConverter : Implements IMultiValueConverter

    Public Function Convert(values As Object(), targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
        ' Ensure both properties are provided
        If values.Length < 2 OrElse
            values(0) Is Nothing OrElse values(1) Is Nothing OrElse
            values(0) Is DependencyProperty.UnsetValue OrElse values(1) Is DependencyProperty.UnsetValue Then
            Return Visibility.Collapsed
        End If

        ' Example logic: Both properties must be True for the element to be visible
        Dim isFreshlyCompressed As Boolean = CType(values(0), Boolean)
        Dim isSteamFolder As Boolean = CType(values(1), Boolean)

        If isFreshlyCompressed AndAlso isSteamFolder Then
            Return Visibility.Visible
        End If

        Return Visibility.Collapsed
    End Function

    Public Function ConvertBack(value As Object, targetTypes As Type(), parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

Public Class AnimationFactorToValueConverter
    Implements IMultiValueConverter

    Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
        If TypeOf values(0) IsNot Double Then
            Return 0.0
        End If

        Dim completeValue As Double = DirectCast(values(0), Double)

        If TypeOf values(1) IsNot Double Then
            Return 0.0
        End If

        Dim factor As Double = DirectCast(values(1), Double)

        If parameter IsNot Nothing AndAlso parameter.ToString() = "negative" Then
            factor = -factor
        End If

        Return factor * completeValue
    End Function

    Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class


Public Class FolderActionStateWorkingToVisibilityConverter
    Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        Dim status = CType(value, ActionState)
        Select Case status
            Case ActionState.Working
                Return Visibility.Visible
            Case Else
                Return Visibility.Collapsed
        End Select
    End Function
    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

End Class