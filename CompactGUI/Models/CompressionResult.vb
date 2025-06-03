
' Used to hold compression results from parsed existing wiki file (above)
Imports CommunityToolkit.Mvvm.ComponentModel

Public Class CompressionResult : Inherits ObservableObject

    Public Property CompType As Core.CompressionMode
    Public Property BeforeBytes As Long = 0
    Public Property AfterBytes As Long = 0
    Public Property TotalResults As Integer = 0


    Public ReadOnly Property CompressionPercent As Integer
        Get
            If BeforeBytes = 0 OrElse AfterBytes = 0 Then Return 0
            Return Math.Round((AfterBytes / BeforeBytes) * 100, 2)
        End Get
    End Property

    Public ReadOnly Property CompressionSavings As Integer
        Get
            If BeforeBytes = 0 OrElse AfterBytes = 0 Then Return 0
            Return Math.Round((BeforeBytes - AfterBytes) / BeforeBytes * 100, 2)
        End Get
    End Property

End Class

