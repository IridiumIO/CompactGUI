Imports System.Threading
Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports Microsoft.Toolkit.Mvvm.Input
Imports ModernWpf.Controls
Imports Ookii.Dialogs.Wpf
Imports CompactGUI.Watcher

Public Class MainViewModel : Inherits ObservableObject


    Sub New()
        SettingsHandler.InitialiseSettings()
        WikiHandler.GetUpdatedJSON()
        FireAndForgetCheckForUpdates()
        InitialiseNotificationTray()
        Watcher = New Watcher.Watcher   'This naming isn't going to get confusing at all...


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
            Dim msgError As New ContentDialog With {.Title = "Invalid Folder", .Content = "This is either a system folder, root directory or a non-NTFS drive and cannot be selected.", .CloseButtonText = "OK"}
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

        AnalyseFolderCommand.Execute(Nothing)

    End Sub

    Private Sub FireAndForgetGetSteamHeader()
        Dim url As String = $"https://steamcdn-a.akamaihd.net/steam/apps/{ActiveFolder.SteamAppID}/page_bg_generated_v6b.jpg"
        Dim bImg As New BitmapImage(New Uri(url))
        If SteamBGImage?.UriSource IsNot Nothing AndAlso SteamBGImage.UriSource = bImg.UriSource Then Return
        SteamBGImage = bImg
    End Sub

    Dim _cancellationTokenSource As CancellationTokenSource

    Public Property CancelCommand As RelayCommand = New RelayCommand(Sub() _cancellationTokenSource.Cancel())


    Private Async Sub AnalyseBegin()

        State = "AnalysingFolderSelected"
        Watcher.IsActive = True
        _cancellationTokenSource = New CancellationTokenSource
        Dim token = _cancellationTokenSource.Token

        Dim Analyser As New Core.Analyser(ActiveFolder.FolderName)

        If Not Analyser.HasDirectoryWritePermission Then
            InsufficientPermissionHandler()
        End If

        Dim containsCompressedFiles = Await Analyser.AnalyseFolder(token)
        If _cancellationTokenSource.IsCancellationRequested Then
            State = "ValidFolderSelected"
            Return
        End If
        ActiveFolder.AnalysisResults = Analyser.FileCompressionDetailsList
        ActiveFolder.CompressedBytes = Analyser.CompressedBytes
        ActiveFolder.UncompressedBytes = Analyser.UncompressedBytes

        If containsCompressedFiles OrElse ActiveFolder.IsFreshlyCompressed Then
            State = "FolderCompressedResults"

            If ActiveFolder.IsFreshlyCompressed Then
                ActiveFolder.PoorlyCompressedFiles = Await Analyser.GetPoorlyCompressedExtensions()
                If SettingsHandler.AppSettings.WatchFolderForChanges Then AddFolderToWatcher()
                Notify_Compressed(ActiveFolder.DisplayName, ActiveFolder.UncompressedBytes - ActiveFolder.CompressedBytes, ActiveFolder.CompressionRatio)
            Else
                Dim compRatioEstimate = Await GetWikiResultsAndSetPoorlyCompressedList()

                Dim allCompressionsInFolder = ActiveFolder.AnalysisResults.Select(Function(f) f.CompressionMode).GroupBy(Function(f) f).OrderByDescending(Function(f) f.Count).ToList
                Dim mainCompressionLVL = ActiveFolder.AnalysisResults.Select(Function(f) f.CompressionMode).Max

                ActiveFolder.SelectedCompressionMode = Core.WOFConvertBackCompressionLevel(mainCompressionLVL)

            End If
        Else

            Dim compRatioEstimate = Await GetWikiResultsAndSetPoorlyCompressedList()
            ActiveFolder.CompressedBytes = compRatioEstimate
            State = "FolderAnalysedResults"
        End If
        SubmitToWikiCommand.NotifyCanExecuteChanged()
        ChooseCompressionCommand.NotifyCanExecuteChanged()
        Watcher.IsActive = False

    End Sub


    Private Sub AddFolderToWatcher()

        Dim newWatched = New Watcher.WatchedFolder With {
            .Folder = ActiveFolder.FolderName,
            .DisplayName = ActiveFolder.DisplayName,
            .IsSteamGame = ActiveFolder.SteamAppID <> 0,
            .LastCompressedSize = ActiveFolder.CompressedBytes,
            .LastUncompressedSize = ActiveFolder.UncompressedBytes,
            .LastCompressedDate = DateTime.Now,
            .LastCheckedDate = DateTime.Now,
            .LastCheckedSize = ActiveFolder.CompressedBytes,
            .LastSystemModifiedDate = DateTime.Now,
            .CompressionLevel = ActiveFolder.AnalysisResults.Select(Function(f) f.CompressionMode).Max}

        Watcher.AddOrUpdateWatched(newWatched)

    End Sub

    Private Sub ChooseCompression()

        State = "ChooseCompressionOptions"

        SettingsHandler.AppSettings.SkipNonCompressable = False
        SettingsHandler.AppSettings.SkipUserNonCompressable = False
        SettingsHandler.AppSettings.Save()


    End Sub


    Private Async Sub CompressBegin()

        State = "CurrentlyCompressing"
        Watcher.IsActive = True
        CProgress.Report((0, ""))

        Dim exclist() As String = {}
        If SettingsHandler.AppSettings.SkipNonCompressable AndAlso SettingsHandler.AppSettings.NonCompressableList.Count <> 0 Then
            exclist = exclist.Union(SettingsHandler.AppSettings.NonCompressableList).ToArray
        End If
        If SettingsHandler.AppSettings.SkipUserNonCompressable AndAlso ActiveFolder.WikiPoorlyCompressedFiles.Count <> 0 Then
            exclist = exclist.Union(ActiveFolder.WikiPoorlyCompressedFiles).ToArray
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
        Return ActiveFolder.SteamAppID <> 0 AndAlso ActiveFolder.IsFreshlyCompressed AndAlso SettingsHandler.AppSettings.SkipUserNonCompressable = False AndAlso SettingsHandler.AppSettings.SkipNonCompressable = False
        'NEED TO RE-ADD CHECK TO NOT LET YOU SUBMIT IF YOU'RE SKIPPING FILES!!!!
    End Function


    Private Sub InsufficientPermissionHandler()

        Dim msg As New ContentDialog
        msg.Title = "Permissions Error"
        If IsAdministrator() Then
            msg.Content = "You are running as Administrator, however, you do not have permission to make changes to this folder - it is likely protected by the system. " & Environment.NewLine & "Analysis results are probably inaccurate and compression will likely fail or cause issues."
            msg.CloseButtonText = "OK"
        Else
            msg.Content = "You do not have permission to make changes to this folder. Would you like to try restarting as administrator?"
            msg.CloseButtonText = "No"
            msg.IsSecondaryButtonEnabled = True
            msg.SecondaryButtonText = "Restart App"
            msg.SecondaryButtonCommand = New RelayCommand(AddressOf RunAsAdmin)
        End If
        msg.ShowAsync()
    End Sub

    Private Sub RunAsAdmin()
        Dim myproc As New Process With {
            .StartInfo = New ProcessStartInfo With {
                .FileName = Environment.ProcessPath,
                .UseShellExecute = True,
                .Arguments = $"""{ActiveFolder.FolderName}""",
                .Verb = "runas"}
        }
        Application.mutex.Dispose()
        myproc.Start()
        Application.Current.Shutdown()

    End Sub


#Region "Properties"

    Public Property UpdateAvailable As New Tuple(Of Boolean, String)(False, Nothing)
    Public Property ActiveFolder As New ActiveFolder
    Public Property LastState As String
    Private _State As String
    Public Property State As String
        Get
            Return _State
        End Get
        Set(value As String)
            If State <> "FolderWatcherView" Then LastState = State
            _State = value
        End Set
    End Property
    Public Property SteamBGImage As BitmapImage = Nothing
    Public Property WorkingProgress As New Tuple(Of Integer, String)(0, "")
    Public Property Watcher As CompactGUI.Watcher.Watcher
    Public ReadOnly Property IsAdmin As Boolean = (New Security.Principal.WindowsPrincipal(Security.Principal.WindowsIdentity.GetCurrent()).IsInRole(Security.Principal.WindowsBuiltInRole.Administrator))
    Public ReadOnly Property BindableSettings As Settings
        Get
            Return SettingsHandler.AppSettings
        End Get
    End Property
    Public ReadOnly Property IsAdministrator() As Boolean
        Get
            Dim principal = New Security.Principal.WindowsPrincipal(Security.Principal.WindowsIdentity.GetCurrent())
            Return principal.IsInRole(Security.Principal.WindowsBuiltInRole.Administrator)
        End Get
    End Property



#End Region

#Region "Commands"
    Public Property AnalyseFolderCommand As ICommand = New RelayCommand(AddressOf AnalyseBegin)
    Public Property SubmitToWikiCommand As RelayCommand = New RelayCommand(AddressOf SubmitToWiki, AddressOf CanSubmitToWiki)
    Public Property ChooseCompressionCommand As RelayCommand = New RelayCommand(AddressOf ChooseCompression, Function() Not SubmitToWikiCommand.CanExecute(Nothing))
    Public Property CompressFolderCommand As RelayCommand = New RelayCommand(AddressOf CompressBegin)
    Public Property UncompressFolderCommand As RelayCommand = New RelayCommand(AddressOf UncompressBegin)
    Public Property MenuCompressionAreaCommand As RelayCommand = New RelayCommand(Sub() State = If(State = "FolderWatcherView", LastState, State))
    Public Property MenuWatcherAreaCommand As RelayCommand = New RelayCommand(Sub() State = "FolderWatcherView")
    Public Property RemoveWatcherCommand As ICommand = New RelayCommand(Of Watcher.WatchedFolder)(Sub(f) Watcher.RemoveWatched(f))
    Public Property ReCompressWatchedCommand As ICommand = New RelayCommand(Of Watcher.WatchedFolder)(Sub(f) SelectFolder(f.Folder))
    Property RefreshWatchedCommand As ICommand = New RelayCommand(Sub() Task.Run(Function() Watcher.ParseWatchers(True)))


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