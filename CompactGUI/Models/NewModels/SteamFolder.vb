Imports System.Collections.ObjectModel

Imports CompactGUI.Core

Public Class SteamFolder : Inherits CompressableFolder


    'Steam-Specific
    Public Property SteamAppID As Integer

    Public Sub New()
    End Sub


    Public Property WikiCompressionResults As WikiCompressionResults
    Public Property WikiPoorlyCompressedFiles As New List(Of String)



    Public Async Function GetWikiResults() As Task
        ' Dim wikihandler = Application.GetService(Of WikiHandler)()
        Dim res = Await WikiHandler.ParseData(SteamAppID)

        WikiPoorlyCompressedFiles = res.poorlyCompressedList?.Where(Function(k) k.Value > 100 AndAlso k.Key <> "").Select(Function(k) k.Key).ToList

        WikiCompressionResults = If(res.compressionResults IsNot Nothing, New WikiCompressionResults(res.compressionResults), Nothing)


    End Function



End Class

Public Class WikiCompressionResults
    Public Property XPress4K As New CompressionResult With {.CompType = Core.CompressionMode.XPRESS4K}
    Public Property XPress8K As New CompressionResult With {.CompType = Core.CompressionMode.XPRESS8K}
    Public Property XPress16K As New CompressionResult With {.CompType = Core.CompressionMode.XPRESS16K}
    Public Property LZX As New CompressionResult With {.CompType = Core.CompressionMode.LZX}

    Sub New(compressionResults As List(Of CompressionResult))
        For Each result In compressionResults
            Select Case result.CompType
                Case Core.WOFCompressionAlgorithm.XPRESS4K
                    XPress4K = result
                Case Core.WOFCompressionAlgorithm.XPRESS8K
                    XPress8K = result
                Case Core.WOFCompressionAlgorithm.XPRESS16K
                    XPress16K = result
                Case Core.WOFCompressionAlgorithm.LZX
                    LZX = result
            End Select
        Next

    End Sub


End Class
