Imports CommunityToolkit.Mvvm.ComponentModel

Public Class WikiCompressionResults : Inherits ObservableObject
    <ObservableProperty> Private _XPress4K As New CompressionResult With {.CompType = Core.CompressionMode.XPRESS4K}
    <ObservableProperty> Private _XPress8K As New CompressionResult With {.CompType = Core.CompressionMode.XPRESS8K}
    <ObservableProperty> Private _XPress16K As New CompressionResult With {.CompType = Core.CompressionMode.XPRESS16K}
    <ObservableProperty> Private _LZX As New CompressionResult With {.CompType = Core.CompressionMode.LZX}

    Sub New(compressionResults As List(Of CompressionResult))
        For Each result In compressionResults
            Select Case result.CompType
                Case Core.CompressionMode.XPRESS4K
                    XPress4K = result
                Case Core.CompressionMode.XPRESS8K
                    XPress8K = result
                Case Core.CompressionMode.XPRESS16K
                    XPress16K = result
                Case Core.CompressionMode.LZX
                    LZX = result
            End Select
        Next

    End Sub


End Class

