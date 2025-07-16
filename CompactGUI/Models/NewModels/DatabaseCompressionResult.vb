Imports CommunityToolkit.Mvvm.ComponentModel

Public Class DatabaseCompressionResult : Inherits ObservableObject

    <ObservableProperty> Private _GameName As String
    <ObservableProperty> Private _SteamID As Integer
    <ObservableProperty> Private _Confidence As DBResultConfidence

    <ObservableProperty> Private _Result_X4K As CompressionResult
    <ObservableProperty> Private _Result_X8K As CompressionResult
    <ObservableProperty> Private _Result_X16K As CompressionResult
    <ObservableProperty> Private _Result_LZX As CompressionResult

    <ObservableProperty> Private _PoorlyCompressedExtensions As List(Of DBPoorlyCompressedExtension)

    Public ReadOnly Property MaxSavings As Decimal
        Get
            Return Math.Max(
            Math.Max(
                If(Result_X4K IsNot Nothing, Result_X4K.CompressionSavings, 0D),
                If(Result_X8K IsNot Nothing, Result_X8K.CompressionSavings, 0D)
            ),
            Math.Max(
                If(Result_X16K IsNot Nothing, Result_X16K.CompressionSavings, 0D),
                If(Result_LZX IsNot Nothing, Result_LZX.CompressionSavings, 0D)
            )
        )
        End Get
    End Property


End Class

Public Enum DBResultConfidence
    Low = 0
    Medium = 1
    High = 2
End Enum

Public Structure DBPoorlyCompressedExtension
    Public Property Extension As String
    Public Property Count As Integer
End Structure