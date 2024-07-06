Imports Microsoft.Toolkit.Mvvm.ComponentModel


' Used to hold compression results from parsed existing wiki file (above)
Public Class CompressionResult : Inherits ObservableObject

    Public Property CompType As Integer
    Public Property BeforeBytes As Long
    Public Property AfterBytes As Long
    Public Property TotalResults As Integer

End Class

