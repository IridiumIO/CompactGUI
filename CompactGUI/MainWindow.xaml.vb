Imports System.Threading
Imports Ookii.Dialogs.Wpf
Imports MethodTimer
Imports System.Windows.Media.Animation

Class MainWindow

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        VisualStateManager.GoToElementState(BaseView, "FreshLaunch", True)

        ' Add any initialization after the InitializeComponent() call.

    End Sub


    Private Sub SearchClicked(sender As Object, e As MouseButtonEventArgs)

        Dim folderSelector As New VistaFolderBrowserDialog
        folderSelector.ShowDialog()

        If folderSelector.SelectedPath = "" Then Return
        _searchBar.SetDataPathAndReturn(folderSelector.SelectedPath)
        VisualStateManager.GoToElementState(BaseView, "ValidFolderSelected", True)

        FireAndForgetGetSteamHeader()

    End Sub

    Private Async Sub FireAndForgetGetSteamHeader()
        Dim appid = Await Task.Run(Function() GetSteamIDFromFolder(_searchBar.DataPath))

        Dim bImg As New BitmapImage(New Uri($"https://steamcdn-a.akamaihd.net/steam/apps/{appid}/page_bg_generated_v6b.jpg"))

        If Not steamBG.Source Is Nothing AndAlso TryCast(steamBG.Source, BitmapImage).UriSource = bImg.UriSource Then
            Return
        End If

        Dim fadeInAnimation = New DoubleAnimation(0.6, New Duration(New TimeSpan(0, 0, 2)))
        Dim fadeOutAnimation = New DoubleAnimation(0, New Duration(New TimeSpan(0, 0, 0, 0, 500)))

        AddHandler fadeOutAnimation.Completed, Sub(o, e)
                                                   steamBG.Source = bImg
                                                   steamBG.BeginAnimation(Image.OpacityProperty, fadeInAnimation)
                                               End Sub

        steamBG.BeginAnimation(Image.OpacityProperty, fadeOutAnimation)

    End Sub



    <Time>
    Private Async Sub AnalyseBegin(hasCompressionRun As Boolean)

        VisualStateManager.GoToElementState(BaseView, "AnalysingFolderSelected", True)


        Dim bytesData = Await Compactor.AnalyseFolder(_searchBar.DataPath, hasCompressionRun)
        Dim appid = Await Task.Run(Function() GetSteamIDFromFolder(_searchBar.DataPath))

        uiAnalysisResultsSxS.SetLeftValue(bytesData.uncompressed)
        uiAnalysisResultsSxS.SetRightValue(bytesData.compressed)


        If bytesData.containsCompressedFiles Then

            uiAnalysisResultsSxS.leftLabel = "before"
            uiAnalysisResultsSxS.rightLabel = "after"
            uiResultsBarAfterSize.Value = CInt(bytesData.compressed / bytesData.uncompressed * 100)
            uiResultsPercentSmaller.Text = CInt(bytesData.compressed / bytesData.uncompressed * 100) & "%"
            VisualStateManager.GoToElementState(BaseView, "FolderCompressedResults", True)

            If hasCompressionRun = True Then
                Dim poorlyCompressedExtensions = Await GetPoorlyCompressedExtensions(bytesData.fileCompressionDetailsList)

            End If

        Else

            Dim compRatioEstimate = Await GetWikiResults(bytesData.uncompressed, appid)
            uiAnalysisResultsSxS.SetRightValue(compRatioEstimate)
            uiAnalysisResultsSxS.leftLabel = "current size"
            uiAnalysisResultsSxS.rightLabel = "estimated size"
            VisualStateManager.GoToElementState(BaseView, "FolderAnalysedResults", True)

        End If



    End Sub

    'TODO: Move this to a WikiSubmission Class
    <Time>
    Private Async Function GetPoorlyCompressedExtensions(FilesList As List(Of FileDetails)) As Task(Of Dictionary(Of String, Integer))

        Dim extensionsResults As New Concurrent.ConcurrentDictionary(Of String, (cRatio As Decimal, cCount As Integer))

        Await Parallel.ForEachAsync(FilesList,
                         Function(fl, ct)

                             Dim fInfo As New IO.FileInfo(fl.FileName)
                             Dim xt = fInfo.Extension

                             extensionsResults.AddOrUpdate(xt,
                                            (fl.CompressedSize / fl.UncompressedSize, 1),
                                            Function(k, v)
                                                Return (((v.cRatio + (fl.CompressedSize / fl.UncompressedSize)) / 2), v.cCount + 1)
                                            End Function)

                         End Function)

        Return extensionsResults.Where(Function(x) x.Value.Item1 > 0.95).ToDictionary(Of String, Integer)(Function(k) k.Key, Function(v) v.Value.cCount)



    End Function


    <Time>
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

        Dim selectedCompressionMode As String = Compactor.CompressionLevel.XPRESS4K
        Dim selectedFolder As String = _searchBar.DataPath

        Select Case comboBoxSelectCompressionMode.SelectedIndex
            Case 0 : selectedCompressionMode = Compactor.CompressionLevel.XPRESS4K
            Case 1 : selectedCompressionMode = Compactor.CompressionLevel.XPRESS8K
            Case 2 : selectedCompressionMode = Compactor.CompressionLevel.XPRESS16K
            Case 3 : selectedCompressionMode = Compactor.CompressionLevel.LZX
        End Select

        Dim progress As IProgress(Of (Integer, String)) = New Progress(Of (Integer, String)) _
            (Sub(val)
                 uiProgBarCompress.Value = val.Item1
                 uiProgPercentage.Text = val.Item1 & "%"
                 uiCurrentFileCompress.Text = val.Item2.Replace(selectedFolder, "")
             End Sub)


        Dim cm As New Compactor(selectedFolder, selectedCompressionMode)
        Await cm.RunCompactAsync(progress)

        AnalyseBegin(True)

    End Sub
End Class
