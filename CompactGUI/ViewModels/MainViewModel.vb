Imports System.Windows.Input
Imports System.Windows.Media.Animation
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

    Public Property UpdateAvailable As New Tuple(Of Boolean, String)(False, Nothing)
    Public Property AnalyseFolderCommand As ICommand = New RelayCommand(AddressOf AnalyseBegin)
    Public Property SubmitToWikiCommand As RelayCommand = New RelayCommand(AddressOf SubmitToWiki, Function()
                                                                                                       Return ActiveFolder.steamAppID <> 0 AndAlso ActiveFolder.IsFreshlyCompressed
                                                                                                       'NEED TO RE-ADD CHECK TO NOT LET YOU SUBMIT IF YOU'RE SKIPPING FILES!!!!
                                                                                                   End Function)
    Public Property ChooseCompressionCommand As RelayCommand = New RelayCommand(AddressOf ChooseCompression, Function()
                                                                                                                 Return Not SubmitToWikiCommand.CanExecute(Nothing)
                                                                                                             End Function)
    Public Property CompressFolderCommand As RelayCommand = New RelayCommand(AddressOf CompressBegin)
    Public Property UncompressFolderCommand As RelayCommand = New RelayCommand(AddressOf UncompressBegin)

    Public Property ActiveFolder As New ActiveFolder
    Public Property State As String
    Public Property SteamBGImage As BitmapImage = Nothing

    Public ReadOnly Property BindableSettings As Settings
        Get
            Return SettingsHandler.AppSettings
        End Get
    End Property

    Private Async Sub FireAndForgetCheckForUpdates()
        Await Task.Delay(2000)
        Dim ret = Await UpdateHandler.CheckForUpdate(True)
        If ret Then
            UpdateAvailable = New Tuple(Of Boolean, String)(True, "update available  -  v" & UpdateHandler.NewVersion.ToString)

        End If
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

            Dim msgError As New ContentDialog With {
            .Title = "Invalid Folder",
            .Content = "For safety, this folder cannot be chosen.",
            .CloseButtonText = "OK"
            }

            msgError.ShowAsync()

            Return
        End If
        ActiveFolder = New ActiveFolder
        ActiveFolder.folderName = path
        ActiveFolder.steamAppID = GetSteamIDFromFolder(path)

        State = "ValidFolderSelected"

        FireAndForgetGetSteamHeader()


    End Sub

    Private Sub FireAndForgetGetSteamHeader()
        Dim url As String = $"https://steamcdn-a.akamaihd.net/steam/apps/{ActiveFolder.steamAppID}/page_bg_generated_v6b.jpg"
        Dim bImg As New BitmapImage(New Uri(url))
        If Not SteamBGImage?.UriSource Is Nothing AndAlso SteamBGImage.UriSource = bImg.UriSource Then Return
        SteamBGImage = bImg
    End Sub




    Private Async Sub AnalyseBegin()

        State = "AnalysingFolderSelected"

        Dim Analyser As New Core.Analyser(ActiveFolder.folderName)
        Dim containsCompressedFiles = Await Analyser.AnalyseFolder()
        ActiveFolder.analysisResults = Analyser.FileCompressionDetailsList
        ActiveFolder.CompressedBytes = Analyser.CompressedBytes
        ActiveFolder.UncompressedBytes = Analyser.UncompressedBytes

        If containsCompressedFiles OrElse ActiveFolder.IsFreshlyCompressed Then
            State = "FolderCompressedResults"

            If ActiveFolder.IsFreshlyCompressed Then
                ActiveFolder.poorlyCompressedFiles = Await Analyser.GetPoorlyCompressedExtensions()
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

    Private Async Function GetWikiResultsAndSetPoorlyCompressedList() As Task(Of Long)

        If ActiveFolder.steamAppID = 0 Then Return 1010101010101010
        Dim res = Await WikiHandler.ParseData(ActiveFolder.steamAppID)
        If res.Equals(Nothing) Then Return 1010101010101010

        'TODO: Modify the 100 cutoff based on level of aggressiveness selected by user in settings
        ActiveFolder.WikiPoorlyCompressedFiles = res.poorlyCompressedList.Where(Function(k) k.Value > 100 AndAlso k.Key <> "").Select(Function(k) k.Key).ToList

        Return CLng(ActiveFolder.UncompressedBytes * res.estimatedRatio)

    End Function




    Private Sub SubmitToWiki()
        Debug.WriteLine("JF")
    End Sub


    Private Async Sub ChooseCompression()

        State = "ChooseCompressionOptions"
        Await Task.Delay(3000)
        ActiveFolder.SelectedCompressionMode = 3

        SettingsHandler.AppSettings.SkipNonCompressable = False
        SettingsHandler.AppSettings.SkipUserNonCompressable = False
        SettingsHandler.AppSettings.Save()

    End Sub

    Public Property WorkingProgress As New Tuple(Of Integer, String)(0, "")
    Dim CProgress As IProgress(Of (Integer, String)) = New Progress(Of (Integer, String))(Sub(val) WorkingProgress = New Tuple(Of Integer, String)(val.Item1, val.Item2.Replace(ActiveFolder.folderName, "")))



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


        Dim cm As New Core.Compactor(ActiveFolder.folderName, Core.WOFConvertCompressionLevel(ActiveFolder.SelectedCompressionMode), exclist)
        Dim res = Await cm.RunCompactAsync(CProgress)

        ActiveFolder.IsFreshlyCompressed = True
        AnalyseBegin()

    End Sub

    Private Async Sub UncompressBegin()
        State = "CurrentlyCompressing"
        CProgress.Report((0, ""))

        Dim compressedFilesList = ActiveFolder.analysisResults.Where(Function(rs) rs.CompressedSize < rs.UncompressedSize).Select(Of String)(Function(f) f.FileName).ToList
        Dim ucm As New Core.Uncompactor
        Dim res = Await ucm.UncompactFiles(compressedFilesList, CProgress)

        ActiveFolder.IsFreshlyCompressed = False
        AnalyseBegin()

    End Sub


End Class











Public Class ImageControl : Inherits Image

    Public Shared ReadOnly SourceChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("SourceChanged", RoutingStrategy.Direct, GetType(RoutedEventHandler), GetType(ImageControl))

    Shared Sub New()
        Image.SourceProperty.OverrideMetadata(GetType(ImageControl), New FrameworkPropertyMetadata(Nothing, AddressOf SourcePropertyChanged))
    End Sub

    Public Custom Event SourceChanged As RoutedEventHandler
        AddHandler(ByVal value As RoutedEventHandler)
            Task.Delay(1000)

            [AddHandler](SourceChangedEvent, value)
        End AddHandler
        RemoveHandler(ByVal value As RoutedEventHandler)
            [RemoveHandler](SourceChangedEvent, value)
        End RemoveHandler
        RaiseEvent(sender As Object, e As RoutedEventArgs)

            [RaiseEvent](e)
        End RaiseEvent
    End Event

    Private Shared Sub SourcePropertyChanged(ByVal obj As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim image As Image = TryCast(obj, Image)
        If image IsNot Nothing Then
            image.[RaiseEvent](New RoutedEventArgs(SourceChangedEvent))
        End If
    End Sub
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