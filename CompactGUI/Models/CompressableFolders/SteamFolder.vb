
Imports System.IO
Imports System.Net.Http

Imports CommunityToolkit.Mvvm.ComponentModel

Imports CompactGUI.Core.Settings

Public Class SteamFolder : Inherits CompressableFolder


    'Steam-Specific
    <ObservableProperty> Private _SteamAppID As Integer


    Public Sub New(folderName As String, displayName As String, steamappId As Integer)
        Me.FolderName = folderName
        Me.SteamAppID = steamappId
        Me.DisplayName = displayName

        If Not CompressableFolderService.IsHDD(Me) AndAlso Core.SharedMethods.IsDirectStorageGameFolder(folderName) Then Application.GetService(Of CustomSnackBarService).ShowDirectStorageWarning(displayName)

    End Sub


    Public Async Function InitializeAsync() As Task
        Try
            Await GetSteamHeaderAsync(Me)
        Catch ex As Exception
            Debug.WriteLine($"Error getting Steam header: {ex.Message}")
        End Try
    End Function


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
        If WikiCompressionResults Is Nothing Then Return

        Dim tempX4KLvl = WikiCompressionResults.XPress4K.CompressionPercent
        WikiCompressionResults.XPress4K.BeforeBytes = UncompressedBytes
        WikiCompressionResults.XPress4K.AfterBytes = UncompressedBytes * tempX4KLvl / 100

        Dim tempX8KLvl = WikiCompressionResults.XPress8K.CompressionPercent
        WikiCompressionResults.XPress8K.BeforeBytes = UncompressedBytes
        WikiCompressionResults.XPress8K.AfterBytes = UncompressedBytes * tempX8KLvl / 100

        Dim tempX16KLvl = WikiCompressionResults.XPress16K.CompressionPercent
        WikiCompressionResults.XPress16K.BeforeBytes = UncompressedBytes
        WikiCompressionResults.XPress16K.AfterBytes = UncompressedBytes * tempX16KLvl / 100

        Dim tempLZXLvl = WikiCompressionResults.LZX.CompressionPercent
        WikiCompressionResults.LZX.BeforeBytes = UncompressedBytes
        WikiCompressionResults.LZX.AfterBytes = UncompressedBytes * tempLZXLvl / 100

    End Function

    Public Shared Async Function GetSteamHeaderAsync(folder As SteamFolder) As Task

        If folder.SteamAppID = 0 Then Return

        Dim tempImg As BitmapImage = Nothing

        Dim EnvironmentPath = Environment.GetEnvironmentVariable("IridiumIO", EnvironmentVariableTarget.User)
        Dim imageDir = Path.Combine(EnvironmentPath, "CompactGUI", "SteamCache")
        Dim imagePath = Path.Combine(imageDir, $"{folder.SteamAppID}.jpg")

        If Not Directory.Exists(imageDir) Then Directory.CreateDirectory(imageDir)

        If File.Exists(imagePath) Then
            tempImg = LoadImageFromDisk(imagePath)
            Debug.WriteLine("Loaded Steam header image from disk")
        Else

            Dim url As String = $"https://steamcdn-a.akamaihd.net/steam/apps/{folder.SteamAppID}/page_bg_generated_v6b.jpg"
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
                                                      folder.FolderBGImage = tempImg
                                                  End If
                                              End Sub)


    End Function


End Class
