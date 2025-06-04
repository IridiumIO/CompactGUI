
Imports System.IO
Imports System.Net.Http

Public Class SteamFolder : Inherits CompressableFolder


    'Steam-Specific
    Public Property SteamAppID As Integer


    Public Sub New(folderName As String, displayName As String, steamappId As Integer)
        Me.FolderName = folderName
        Me.SteamAppID = steamappId
        Me.DisplayName = displayName

        Dim GetSteamHeader As Task = GetSteamHeaderAsync()
        GetSteamHeader.ContinueWith(Sub(t)
                                        If t.Exception IsNot Nothing Then
                                            Debug.WriteLine($"Error getting Steam header: {t.Exception.Message}")
                                        End If
                                    End Sub, TaskContinuationOptions.OnlyOnFaulted)

    End Sub



    Public Overloads Property WikiCompressionResults As WikiCompressionResults
    Public Overloads Property WikiPoorlyCompressedFiles As New List(Of String)

    Public Overloads ReadOnly Property WikiPoorlyCompressedFilesCount As Integer
        Get
            If AnalysisResults Is Nothing OrElse WikiPoorlyCompressedFiles Is Nothing Then Return 0
            Return AnalysisResults.Where(Function(fl) WikiPoorlyCompressedFiles.Contains(New FileInfo(fl.FileName).Extension)).Count
        End Get
    End Property


    Public Async Function GetWikiResults() As Task
        ' Dim wikihandler = Application.GetService(Of WikiHandler)()
        Dim res = Await Application.GetService(Of IWikiService).ParseData(SteamAppID)

        WikiPoorlyCompressedFiles = res.poorlyCompressedList?.Where(Function(k) k.Value > 100 AndAlso k.Key <> "").Select(Function(k) k.Key).ToList

        WikiCompressionResults = If(res.compressionResults IsNot Nothing, New WikiCompressionResults(res.compressionResults), Nothing)


    End Function

    Private Async Function GetSteamHeaderAsync() As Task

        If SteamAppID = 0 Then Return

        Dim tempImg As BitmapImage = Nothing


        Dim EnvironmentPath = Environment.GetEnvironmentVariable("IridiumIO", EnvironmentVariableTarget.User)
        Dim imagePath = Path.Combine(EnvironmentPath, "CompactGUI", "SteamCache", $"{SteamAppID}.jpg")

        If Not Path.Exists(Path.GetDirectoryName(imagePath)) Then Directory.CreateDirectory(Path.GetDirectoryName(imagePath))

        If File.Exists(imagePath) Then
            tempImg = LoadImageFromDisk(imagePath)
            Debug.WriteLine("Loaded Steam header image from disk")
        Else

            Dim url As String = $"https://steamcdn-a.akamaihd.net/steam/apps/{SteamAppID}/page_bg_generated_v6b.jpg"
            'If FolderBGImage?.UriSource IsNot Nothing AndAlso FolderBGImage.UriSource.ToString() = url Then Return

            Try
                Using client As New HttpClient()
                    Dim imageData As Byte() = Await client.GetByteArrayAsync(url)
                    tempImg = LoadImageFromMemoryStream(imageData)
                    Await File.WriteAllBytesAsync(imagePath, imageData)
                End Using
            Catch ex As Exception
                Debug.WriteLine($"Failed to load Steam header image: {ex.Message}")
            End Try

        End If

        Application.Current.Dispatcher.Invoke(Sub()
                                                  If tempImg IsNot Nothing Then
                                                      FolderBGImage = tempImg
                                                  End If
                                              End Sub)


    End Function


    Protected Overrides Function GetSkipList() As String()
        Dim exclist As String() = Array.Empty(Of String)()
        If CompressionOptions.SkipPoorlyCompressedFileTypes AndAlso SettingsHandler.AppSettings.NonCompressableList.Count <> 0 Then
            Debug.WriteLine("Adding non-compressable list to exclusion list")
            exclist = exclist.Union(SettingsHandler.AppSettings.NonCompressableList).ToArray
        End If

        If CompressionOptions.SkipUserSubmittedFiletypes AndAlso WikiPoorlyCompressedFiles?.Count <> 0 Then
            Debug.WriteLine("Adding wiki poorly compressed list to exclusion list")
            exclist = exclist.Union(WikiPoorlyCompressedFiles).ToArray
        End If

        Return exclist
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
