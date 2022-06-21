Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports Microsoft.Toolkit.Mvvm.Input
Imports ModernWpf.Controls
Imports Ookii.Dialogs.Wpf

Public Class MainViewModel : Inherits ObservableObject


    Sub New()
        SettingsHandler.InitialiseSettings()
        WikiHandler.GetUpdatedJSON()
        FireAndForgetCheckForUpdates()
    End Sub


    Private Async Sub FireAndForgetCheckForUpdates()
        Dim ret = Await UpdateHandler.CheckForUpdate(True)
        If ret Then UpdateAvailable = New Tuple(Of Boolean, String)(True, "update available  -  v" & UpdateHandler.NewVersion.ToString)
    End Sub


    Public Sub SelectFolder(Optional path As String = Nothing)
        If path Is Nothing Then
            Dim folderSelector As New VistaFolderBrowserDialog
            folderSelector.ShowDialog()
            If folderSelector.SelectedPath = "" Then Return
            path = folderSelector.SelectedPath
        End If
        Dim validFolder = Core.verifyFolder(path)
        If Not validFolder Then
            Dim msgError As New ContentDialog With {.Title = "Invalid Folder", .Content = "For safety, this folder cannot be chosen.", .CloseButtonText = "OK"}
            msgError.ShowAsync()
            Return
        End If

        ActiveFolder = New ActiveFolder
        ActiveFolder.FolderName = path

        Dim SteamFolderData = GetSteamNameAndIDFromFolder(path)

        ActiveFolder.SteamAppID = SteamFolderData.appID
        ActiveFolder.DisplayName = If(SteamFolderData.gameName, path)


        State = "ValidFolderSelected"

        FireAndForgetGetSteamHeader()

    End Sub

    Private Sub FireAndForgetGetSteamHeader()
        Dim url As String = $"https://steamcdn-a.akamaihd.net/steam/apps/{ActiveFolder.SteamAppID}/page_bg_generated_v6b.jpg"
        Dim bImg As New BitmapImage(New Uri(url))
        If SteamBGImage?.UriSource IsNot Nothing AndAlso SteamBGImage.UriSource = bImg.UriSource Then Return
        SteamBGImage = bImg
    End Sub


    Private Async Sub AnalyseBegin()

        State = "AnalysingFolderSelected"

        Dim Analyser As New Core.Analyser(ActiveFolder.FolderName)
        Dim containsCompressedFiles = Await Analyser.AnalyseFolder()
        ActiveFolder.AnalysisResults = Analyser.FileCompressionDetailsList
        ActiveFolder.CompressedBytes = Analyser.CompressedBytes
        ActiveFolder.UncompressedBytes = Analyser.UncompressedBytes

        If containsCompressedFiles OrElse ActiveFolder.IsFreshlyCompressed Then
            State = "FolderCompressedResults"

            If ActiveFolder.IsFreshlyCompressed Then
                ActiveFolder.PoorlyCompressedFiles = Await Analyser.GetPoorlyCompressedExtensions()
            Else
                Dim compRatioEstimate = Await GetWikiResultsAndSetPoorlyCompressedList()
            End If
        Else

            Dim compRatioEstimate = Await GetWikiResultsAndSetPoorlyCompressedList()
            ActiveFolder.CompressedBytes = compRatioEstimate
            State = "FolderAnalysedResults"
        End If
        SubmitToWikiCommand.NotifyCanExecuteChanged()
        ChooseCompressionCommand.NotifyCanExecuteChanged()


    End Sub


    Private Sub ChooseCompression()

        State = "ChooseCompressionOptions"

        SettingsHandler.AppSettings.SkipNonCompressable = False
        SettingsHandler.AppSettings.SkipUserNonCompressable = False
        SettingsHandler.AppSettings.Save()


    End Sub


    Private Async Sub CompressBegin()

        State = "CurrentlyCompressing"
        CProgress.Report((0, ""))

        Dim exclist() As String = {}
        If SettingsHandler.AppSettings.SkipNonCompressable AndAlso SettingsHandler.AppSettings.NonCompressableList.Count <> 0 Then
            '  exclist = exclist.Union(SettingsHandler.AppSettings.NonCompressableList).ToArray
        End If
        If SettingsHandler.AppSettings.SkipUserNonCompressable AndAlso ActiveFolder.WikiPoorlyCompressedFiles.Count <> 0 Then
            '  exclist = exclist.Union(activeFolder.WikiPoorlyCompressedFiles).ToArray
        End If


        Dim cm As New Core.Compactor(ActiveFolder.FolderName, Core.WOFConvertCompressionLevel(ActiveFolder.SelectedCompressionMode), exclist)
        Dim res = Await cm.RunCompactAsync(CProgress)

        ActiveFolder.IsFreshlyCompressed = True
        AnalyseBegin()

    End Sub


    Private Async Sub UncompressBegin()
        State = "CurrentlyCompressing"
        CProgress.Report((0, ""))

        Dim compressedFilesList = ActiveFolder.AnalysisResults.Where(Function(rs) rs.CompressedSize < rs.UncompressedSize).Select(Of String)(Function(f) f.FileName).ToList
        Dim ucm As New Core.Uncompactor
        Dim res = Await ucm.UncompactFiles(compressedFilesList, CProgress)

        ActiveFolder.IsFreshlyCompressed = False
        AnalyseBegin()

    End Sub

    Dim CProgress As IProgress(Of (Integer, String)) = New Progress(Of (Integer, String))(Sub(val) WorkingProgress = New Tuple(Of Integer, String)(val.Item1, val.Item2.Replace(ActiveFolder.FolderName, "")))


    Private Async Function GetWikiResultsAndSetPoorlyCompressedList() As Task(Of Long)

        If ActiveFolder.SteamAppID = 0 Then Return 1010101010101010
        Dim res = Await WikiHandler.ParseData(ActiveFolder.SteamAppID)
        If res.Equals(Nothing) Then Return 1010101010101010

        'TODO: Modify the 100 cutoff based on level of aggressiveness selected by user in settings
        ActiveFolder.WikiPoorlyCompressedFiles = res.poorlyCompressedList.Where(Function(k) k.Value > 100 AndAlso k.Key <> "").Select(Function(k) k.Key).ToList

        Return CLng(ActiveFolder.UncompressedBytes * res.estimatedRatio)

    End Function


    Private Async Sub SubmitToWiki()
        ActiveFolder.IsFreshlyCompressed = False
        SubmitToWikiCommand.NotifyCanExecuteChanged()
        Dim successfullySent = Await WikiHandler.SubmitToWiki(ActiveFolder.FolderName, ActiveFolder.AnalysisResults, ActiveFolder.PoorlyCompressedFiles, ActiveFolder.SelectedCompressionMode)
        If Not successfullySent Then ActiveFolder.IsFreshlyCompressed = True
        SubmitToWikiCommand.NotifyCanExecuteChanged()
        ChooseCompressionCommand.NotifyCanExecuteChanged()
    End Sub


    Private Function CanSubmitToWiki() As Boolean
        Return ActiveFolder.SteamAppID <> 0 AndAlso ActiveFolder.IsFreshlyCompressed
        'NEED TO RE-ADD CHECK TO NOT LET YOU SUBMIT IF YOU'RE SKIPPING FILES!!!!
    End Function


#Region "Properties"

    Public Property UpdateAvailable As New Tuple(Of Boolean, String)(False, Nothing)
    Public Property ActiveFolder As New ActiveFolder
    Public Property State As String
    Public Property SteamBGImage As BitmapImage = Nothing
    Public Property WorkingProgress As New Tuple(Of Integer, String)(0, "")
    Public ReadOnly Property BindableSettings As Settings
        Get
            Return SettingsHandler.AppSettings
        End Get
    End Property

#End Region

#Region "Commands"
    Public Property AnalyseFolderCommand As ICommand = New RelayCommand(AddressOf AnalyseBegin)
    Public Property SubmitToWikiCommand As RelayCommand = New RelayCommand(AddressOf SubmitToWiki, AddressOf CanSubmitToWiki)
    Public Property ChooseCompressionCommand As RelayCommand = New RelayCommand(AddressOf ChooseCompression, Function() Not SubmitToWikiCommand.CanExecute(Nothing))
    Public Property CompressFolderCommand As RelayCommand = New RelayCommand(AddressOf CompressBegin)
    Public Property UncompressFolderCommand As RelayCommand = New RelayCommand(AddressOf UncompressBegin)

#End Region


End Class









Public Class VisualStateApplier
    Public Shared Function GetVisualState(ByVal target As DependencyObject) As String
        Return TryCast(target.GetValue(VisualStateProperty), String)
    End Function

    Public Shared Sub SetVisualState(ByVal target As DependencyObject, ByVal value As String)
        target.SetValue(VisualStateProperty, value)
    End Sub

    Public Shared ReadOnly VisualStateProperty As DependencyProperty = DependencyProperty.RegisterAttached("VisualState", GetType(String), GetType(VisualStateApplier), New PropertyMetadata(Nothing, AddressOf VisualStatePropertyChangedCallback))

    Private Shared Sub VisualStatePropertyChangedCallback(ByVal target As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        VisualStateManager.GoToElementState(CType(target, FrameworkElement), TryCast(args.NewValue, String), True)
    End Sub
End Class