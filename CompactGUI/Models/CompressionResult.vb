
Imports CommunityToolkit.Mvvm.ComponentModel

Public Class CompressionResult : Inherits ObservableObject

    <ObservableProperty> Private _CompType As Core.CompressionMode

    <NotifyPropertyChangedFor(NameOf(CompressionSavings), NameOf(CompressionSavings), NameOf(BytesSaved), NameOf(IsNonExistent))>
    <ObservableProperty> Private _BeforeBytes As Long = 0

    <NotifyPropertyChangedFor(NameOf(CompressionSavings), NameOf(CompressionSavings), NameOf(BytesSaved), NameOf(IsNonExistent))>
    <ObservableProperty> Private _AfterBytes As Long = 0

    <ObservableProperty> Private _TotalResults As Integer = 0


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

    Public ReadOnly Property BytesSaved As Long
        Get
            Return BeforeBytes - AfterBytes
        End Get
    End Property

    Private ReadOnly Property IsNonExistent As Boolean
        Get
            Return BeforeBytes = 0 OrElse AfterBytes = 0
        End Get
    End Property

End Class

