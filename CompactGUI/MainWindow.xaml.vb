Imports Ookii.Dialogs.Wpf
Imports MethodTimer
Imports System.Windows.Media.Animation
Imports ModernWpf.Controls
Imports CompactGUI.Core

Class MainWindow

    Sub New()

        InitializeComponent()

        Me.DataContext = ViewModel
        _searchBar.DataContext = ViewModel
        ' VisualStateManager.GoToElementState(BaseView, "FreshLaunch", True)
        ViewModel.State = "FreshLaunch"

    End Sub

    Public Property ViewModel As New MainViewModel

    Property activeFolder As ActiveFolder

    Private Sub SearchClicked(sender As Object, e As MouseButtonEventArgs)

        ViewModel.SelectFolder()

    End Sub



    Private Async Sub AnalyseBegin(hasCompressionRun As Boolean)

        VisualStateManager.GoToElementState(BaseView, "AnalysingFolderSelected", True)

        Dim analyser As New Analyser(activeFolder.folderName)
        Dim containsCompressedFiles = Await analyser.AnalyseFolder()
        Dim appid = activeFolder.steamAppID
        activeFolder.analysisResults = analyser.FileCompressionDetailsList
        uiAnalysisResultsSxS.SetLeftValue(analyser.UncompressedBytes)
        uiAnalysisResultsSxS.SetRightValue(analyser.CompressedBytes)

        If containsCompressedFiles OrElse hasCompressionRun Then

            uiResultsBarAfterSize.Value = CInt(analyser.CompressedBytes / analyser.UncompressedBytes * 100)
            uiResultsPercentSmaller.Text = CInt(100 - (analyser.CompressedBytes / analyser.UncompressedBytes * 100)) & "%"
            btnSubmitToWiki.IsEnabled = hasCompressionRun AndAlso activeFolder.steamAppID <> 0
            VisualStateManager.GoToElementState(BaseView, "FolderCompressedResults", True)

            If hasCompressionRun Then
                activeFolder.poorlyCompressedFiles = Await GetPoorlyCompressedExtensions(analyser.FileCompressionDetailsList)
                btnSubmitToWiki.Content = "submit results"
                btnSubmitToWiki.IsEnabled = activeFolder.steamAppID <> 0

                If SettingsHandler.AppSettings.SkipUserNonCompressable AndAlso activeFolder.WikiPoorlyCompressedFiles.Count > 0 Then
                    '  btnSubmitToWiki.IsEnabled = False
                End If

                'TODO: Add ability to save poor extensions for next time
            Else
                Dim compRatioEstimate = Await GetWikiResultsAndSetPoorlyCompressedList(analyser.UncompressedBytes, appid)
                btnSubmitToWiki.Content = "compress again"
                btnSubmitToWiki.IsEnabled = True
            End If

        Else

            Dim compRatioEstimate = Await GetWikiResultsAndSetPoorlyCompressedList(analyser.UncompressedBytes, appid)
            uiAnalysisResultsSxS.SetRightValue(compRatioEstimate)
            VisualStateManager.GoToElementState(BaseView, "FolderAnalysedResults", True)

        End If
    End Sub


    Private Async Function GetPoorlyCompressedExtensions(FilesList As List(Of Core.AnalysedFileDetails)) As Task(Of List(Of ExtensionResults))

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


    Private Async Function GetWikiResultsAndSetPoorlyCompressedList(beforeBytes As Long, appid As Integer) As Task(Of Long)

        If appid = 0 Then Return 1010101010101010
        Dim res = Await WikiHandler.ParseData(appid)
        If res.Equals(Nothing) Then Return 1010101010101010 '1010101010101010 is used as a flag to set the "?" instead

        'TODO: Modify the 100 cutoff based on level of aggressiveness selected by user in settings
        Dim poorlyCompressed = res.poorlyCompressedList
        activeFolder.WikiPoorlyCompressedFiles = poorlyCompressed.Where(Function(k) k.Value > 100 AndAlso k.Key <> "").Select(Function(k) k.Key).ToList

        Return CLng(beforeBytes * res.estimatedRatio)

    End Function

    Private Sub AnalyseClicked(sender As Object, e As RoutedEventArgs)
        AnalyseBegin(False)
    End Sub

    Private Sub btnChooseCompressionOptions_Click(sender As Object, e As RoutedEventArgs)
        VisualStateManager.GoToElementState(BaseView, "ChooseCompressionOptions", True)
        uiChkSkipPoorlyCompressed.IsChecked = False
        uiChkSkipUserPoorlyCompressed.IsChecked = False
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
        Dim exclist() As String = {}

        If SettingsHandler.AppSettings.SkipNonCompressable AndAlso SettingsHandler.AppSettings.NonCompressableList.Count <> 0 Then
            '  exclist = exclist.Union(SettingsHandler.AppSettings.NonCompressableList).ToArray
        End If
        If SettingsHandler.AppSettings.SkipUserNonCompressable AndAlso activeFolder.WikiPoorlyCompressedFiles.Count <> 0 Then
            '  exclist = exclist.Union(activeFolder.WikiPoorlyCompressedFiles).ToArray
        End If

        Dim cm As New Core.Compactor(activeFolder.folderName, Core.WOFConvertCompressionLevel(comboBoxSelectCompressionMode.SelectedIndex), exclist)
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

        Dim compressedFilesList = activeFolder.analysisResults.Where(Function(res) res.CompressedSize < res.UncompressedSize).Select(Of String)(Function(f) f.FileName).ToList
        Dim uncompactor As New Uncompactor
        Await uncompactor.UncompactFiles(compressedFilesList, progress)

        AnalyseBegin(False)

    End Sub

    Private Async Sub submitToWikiClicked()
        If btnSubmitToWiki.Content = "compress again" Then
            VisualStateManager.GoToElementState(BaseView, "ChooseCompressionOptions", True)
            uiChkSkipPoorlyCompressed.IsChecked = False
            uiChkSkipUserPoorlyCompressed.IsChecked = False
            Return
        End If
        btnSubmitToWiki.IsEnabled = False
        Dim successfullySent = Await WikiHandler.SubmitToWiki(activeFolder.folderName, activeFolder.analysisResults, activeFolder.poorlyCompressedFiles, comboBoxSelectCompressionMode.SelectedIndex)
        If Not successfullySent Then btnSubmitToWiki.IsEnabled = True
    End Sub

    Private Sub uiUpdateBanner_MouseUp(sender As Object, e As MouseButtonEventArgs)

        Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI/releases/") With {.UseShellExecute = True})

    End Sub

    Private Sub uiBtnOptions_Click(sender As Object, e As RoutedEventArgs) Handles uiBtnOptions.Click

        Dim settingsDialog As New ContentDialog With {.Content = New SettingsControl}
        settingsDialog.PrimaryButtonText = "save and close"
        settingsDialog.ShowAsync()

    End Sub

    Private Function ParseFolderAndCountFilesToSkip(skiplist As List(Of String)) As Integer
        Dim skippableCount = activeFolder.analysisResults.Where(Function(fl) skiplist.Contains(New IO.FileInfo(fl.FileName).Extension))

    End Function


    Private Sub uiChkSkipPoorlyCompressed_Checked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.SkipNonCompressable = True
        SettingsHandler.AppSettings.Save()

        Dim skippableCount = activeFolder.analysisResults.Where(Function(fl) SettingsHandler.AppSettings.NonCompressableList.Contains(New IO.FileInfo(fl.FileName).Extension))
        uiSkipGlobalFiles_Count.Text = $"{skippableCount.Count} files will be skipped"

    End Sub

    Private Sub uiChkSkipPoorlyCompressed_Unchecked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.SkipNonCompressable = False
        SettingsHandler.AppSettings.Save()
        uiSkipGlobalFiles_Count.Text = ""
    End Sub

    Private Sub uiChkSkipUserPoorlyCompressed_Checked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.SkipUserNonCompressable = True
        SettingsHandler.AppSettings.Save()
        If activeFolder.WikiPoorlyCompressedFiles Is Nothing Then Return
        Dim skippableCount = activeFolder.analysisResults.Where(Function(fl) activeFolder.WikiPoorlyCompressedFiles.Contains(New IO.FileInfo(fl.FileName).Extension))

        If skippableCount.Count = 0 Then
            uiSkipFiles_Count.Text = $"Not enough user submissions to build skiplist."
        Else
            uiSkipFiles_Count.Text = $"{skippableCount.Count} files will be skipped"
        End If

    End Sub

    Private Sub uiChkSkipUserPoorlyCompressed_Unchecked(sender As Object, e As RoutedEventArgs)
        SettingsHandler.AppSettings.SkipUserNonCompressable = False
        SettingsHandler.AppSettings.Save()
        uiSkipFiles_Count.Text = ""
    End Sub
End Class
