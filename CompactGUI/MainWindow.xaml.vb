Imports Ookii.Dialogs.Wpf
Imports MethodTimer
Imports System.Windows.Media.Animation
Imports ModernWpf.Controls

Class MainWindow

    Sub New()

        InitializeComponent()

        VisualStateManager.GoToElementState(BaseView, "FreshLaunch", True)

        SettingsHandler.InitialiseSettings()

        WikiHandler.GetUpdatedJSON()

        FireAndForgetCheckForUpdates()

    End Sub


    Property activeFolder As ActiveFolder


    Private Sub SearchClicked(sender As Object, e As MouseButtonEventArgs)

        Dim folderSelector As New VistaFolderBrowserDialog
        folderSelector.ShowDialog()

        If folderSelector.SelectedPath = "" Then Return
        Dim validFolder = _searchBar.SetDataPathAndReturn(folderSelector.SelectedPath)
        If Not validFolder Then Return

        activeFolder = New ActiveFolder
        activeFolder.folderName = folderSelector.SelectedPath
        activeFolder.steamAppID = GetSteamIDFromFolder(_searchBar.DataPath)

        VisualStateManager.GoToElementState(BaseView, "ValidFolderSelected", True)

        FireAndForgetGetSteamHeader()

    End Sub

    Private Async Sub FireAndForgetCheckForUpdates()

        Dim ret = Await UpdateHandler.CheckForUpdate(True)
        If ret Then
            uiUpdateBanner.Visibility = Visibility.Visible
            uiUpdateText.Text = "update available  -  v" & UpdateHandler.NewVersion.ToString
        End If
    End Sub

    Private Sub FireAndForgetGetSteamHeader()

        steamBG.Opacity = 0
        Dim appid = activeFolder.steamAppID
        Dim url As String = $"https://steamcdn-a.akamaihd.net/steam/apps/{appid}/page_bg_generated_v6b.jpg"
        Dim bImg As New BitmapImage(New Uri(url))

        If Not steamBG.Source Is Nothing AndAlso TryCast(steamBG.Source, BitmapImage)?.UriSource = bImg.UriSource Then Return

        Dim fadeInAnimation = New DoubleAnimation(0.6, New Duration(New TimeSpan(0, 0, 2)))
        Dim fadeOutAnimation = New DoubleAnimation(0, New Duration(New TimeSpan(0, 0, 0, 0, 500)))

        AddHandler bImg.DownloadCompleted,
            Sub()
                AddHandler fadeOutAnimation.Completed,
                        Sub(o, e)
                            steamBG.Source = bImg
                            steamBG.BeginAnimation(Image.OpacityProperty, fadeInAnimation)
                        End Sub
                steamBG.BeginAnimation(Image.OpacityProperty, fadeOutAnimation)
            End Sub

    End Sub


    Private Async Sub AnalyseBegin(hasCompressionRun As Boolean)

        VisualStateManager.GoToElementState(BaseView, "AnalysingFolderSelected", True)
        Dim bytesData = Await Compactor.AnalyseFolder(activeFolder.folderName, hasCompressionRun)
        Dim appid = activeFolder.steamAppID
        activeFolder.analysisResults = bytesData.fileCompressionDetailsList
        uiAnalysisResultsSxS.SetLeftValue(bytesData.uncompressed)
        uiAnalysisResultsSxS.SetRightValue(bytesData.compressed)

        If bytesData.containsCompressedFiles OrElse hasCompressionRun Then

            uiResultsBarAfterSize.Value = CInt(bytesData.compressed / bytesData.uncompressed * 100)
            uiResultsPercentSmaller.Text = CInt(100 - (bytesData.compressed / bytesData.uncompressed * 100)) & "%"
            btnSubmitToWiki.IsEnabled = hasCompressionRun AndAlso activeFolder.steamAppID <> 0
            VisualStateManager.GoToElementState(BaseView, "FolderCompressedResults", True)

            If hasCompressionRun Then
                activeFolder.poorlyCompressedFiles = Await GetPoorlyCompressedExtensions(bytesData.fileCompressionDetailsList)
                'TODO: Add ability to save poor extensions for next time
            End If

        Else

            Dim compRatioEstimate = Await GetWikiResults(bytesData.uncompressed, appid)
            uiAnalysisResultsSxS.SetRightValue(compRatioEstimate)
            VisualStateManager.GoToElementState(BaseView, "FolderAnalysedResults", True)

        End If
    End Sub


    Private Async Function GetPoorlyCompressedExtensions(FilesList As List(Of FileDetails)) As Task(Of List(Of ExtensionResults))

        Dim extClassResults As List(Of ExtensionResults) = Await Task.Run(
            Function()
                Dim extRes As New List(Of ExtensionResults)
                For Each fl In FilesList
                    Dim fInfo As New IO.FileInfo(fl.FileName)
                    Dim xt = fInfo.Extension
                    If fl.UncompressedSize = 0 Then Continue For
                    Dim obj = extRes.FirstOrDefault(Function(x) x.extension = xt, Nothing)
                    If obj Is Nothing Then
                        extRes.Add(New ExtensionResults With {.extension = xt, .totalFiles = 1, .uncompressedBytes = fl.UncompressedSize, .compressedBytes = fl.CompressedSize})
                        Continue For
                    End If
                    obj.uncompressedBytes += fl.UncompressedSize
                    obj.compressedBytes += fl.CompressedSize
                    obj.totalFiles += 1
                Next
                Return extRes
            End Function)

        Return extClassResults.Where(Function(f) f.cRatio > 0.95).ToList()

    End Function


    Private Async Function GetWikiResults(beforeBytes As Long, appid As Integer) As Task(Of Long)

        If appid = 0 Then Return 1010101010101010
        Dim res = Await WikiHandler.ParseData(appid)
        If res.Equals(Nothing) Then Return 1010101010101010 '1010101010101010 is used as a flag to set the "?" instead
        Return CLng(beforeBytes * res.estimatedRatio)

    End Function

    Private Sub AnalyseClicked(sender As Object, e As RoutedEventArgs)
        AnalyseBegin(False)
    End Sub

    Private Sub btnChooseCompressionOptions_Click(sender As Object, e As RoutedEventArgs)
        VisualStateManager.GoToElementState(BaseView, "ChooseCompressionOptions", True)

    End Sub

    Private Async Sub btnCompress_Click(sender As Object, e As RoutedEventArgs)
        VisualStateManager.GoToElementState(BaseView, "CurrentlyCompressing", True)

        Dim progress As IProgress(Of (Integer, String)) = New Progress(Of (Integer, String)) _
            (Sub(val)
                 uiProgBarCompress.Value = val.Item1
                 uiProgPercentage.Text = val.Item1 & "%"
                 uiCurrentFileCompress.Text = val.Item2.Replace(activeFolder.folderName, "")
             End Sub)
        progress.Report((0, ""))
        Dim exclist As New List(Of String)({".vanim_c", ".vmat_c", ".vxml_c", ".vjs_c", ".res", ".vcfg", ".vphys_c", ".vseq_c", ".vpcf_c", ".cab", ".webm"})

        Dim cm As New Compactor(activeFolder.folderName, comboBoxSelectCompressionMode.SelectedIndex, exclist)
        Dim res = Await cm.RunCompactAsync(progress)

        AnalyseBegin(True)

    End Sub

    Private Async Sub UncompressClicked()

        VisualStateManager.GoToElementState(BaseView, "CurrentlyCompressing", True)

        Dim selectedFolder As String = activeFolder.folderName

        Dim progress As IProgress(Of (Integer, String)) = New Progress(Of (Integer, String)) _
            (Sub(val)
                 uiProgBarCompress.Value = val.Item1
                 uiProgPercentage.Text = val.Item1 & "%"
                 uiCurrentFileCompress.Text = val.Item2.Replace(selectedFolder, "")
             End Sub)
        progress.Report((0, ""))

        Dim compressedFilesList = activeFolder.analysisResults.Where(Function(res) res.CompressedSize < res.UncompressedSize).Select(Of String)(Function(f) f.FileName)

        Await Compactor.UncompressFolder(activeFolder.folderName, compressedFilesList.ToList, progress)

        AnalyseBegin(False)

    End Sub

    Private Async Sub submitToWikiClicked()
        btnSubmitToWiki.IsEnabled = False
        Dim successfullySent = Await WikiHandler.SubmitToWiki(activeFolder.folderName, activeFolder.analysisResults, activeFolder.poorlyCompressedFiles, comboBoxSelectCompressionMode.SelectedIndex)
        If Not successfullySent Then btnSubmitToWiki.IsEnabled = True
    End Sub

    Private Sub uiUpdateBanner_MouseUp(sender As Object, e As MouseButtonEventArgs)

        Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI/releases/") With {.UseShellExecute = True})

    End Sub
End Class
