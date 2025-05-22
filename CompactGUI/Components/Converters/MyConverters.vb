Imports FunctionalConverters

Public Class MyConverters : Inherits ExtensibleConverter

    Public Sub New(ConverterName As String)
        MyBase.New(ConverterName)
    End Sub


    Public Shared Function BytesToProgressMultiConverter() As MultiConverter(Of Long, Integer)

        Dim convert = Function(folderBytes As Long()) As Integer
                          Return 25
                      End Function

        Return CreateMultiConverter(convert)

    End Function


End Class
