Imports System.Threading
Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports Microsoft.Toolkit.Mvvm.Input
Imports ModernWpf.Controls
Imports CompactGUI.Watcher
Imports System.IO
Imports System.Net.Http
Imports System.Runtime

Public Class MainViewModel : Inherits ObservableObject


    Sub New()

        Dim WikiUpdate = WikiHandler.GetUpdatedJSONAsync()
        Dim UpdateTask = CheckForUpdatesAsync()
        InitialiseNotificationTray()
        Watcher = New Watcher.Watcher(GetSkipList)   'This naming isn't going to get confusing at all...


    End Sub


    Private Async Function CheckForUpdatesAsync() As Task

        Dim ret = Await UpdateHandler.CheckForUpdate(SettingsHandler.AppSettings.EnablePreReleaseUpdates)
        If ret Then UpdateAvailable = New Tuple(Of Boolean, String)(True, "update available  -  v" & UpdateHandler.NewVersion.Friendly)
    End Function


    Public Async Function SelectFolderAsync(Optional path As String = Nothing) As Task
        path = path?.TrimEnd(IO.Path.DirectorySeparatorChar, IO.Path.AltDirectorySeparatorChar).Replace("/"c, "\"c)
        If path Is Nothing Then
            Dim folderSelector As New Microsoft.Win32.OpenFolderDialog
            folderSelector.ShowDialog()
            If folderSelector.FolderName = "" Then Return
            path = folderSelector.FolderName
        End If
        Dim validFolder = Core.verifyFolder(path)
        If Not validFolder.isValid Then
            Dim msgError As New ContentDialog With {.Title = "Invalid Folder", .Content = $"{validFolder.msg}", .CloseButtonText = "OK"}
            Await msgError.ShowAsync()
            Return
        End If
        Dim SteamFolderData = GetSteamNameAndIDFromFolder(path)

        ActiveFolder = New ActiveFolder With {
            .FolderName = path,
            .SteamAppID = SteamFolderData.appID,
            .DisplayName = If(SteamFolderData.gameName, path)
        }


        State = UIState.ValidFolderSelected

        Dim GetSteamHeader As Task = GetSteamHeaderAsync()

        AnalyseFolderCommand.Execute(Nothing)

        Await GetSteamHeader

    End Function



    Private Async Function GetSteamHeaderAsync() As Task

        If ActiveFolder.SteamAppID = 0 Then Return

        Dim EnvironmentPath = Environment.GetEnvironmentVariable("IridiumIO", EnvironmentVariableTarget.User)
        Dim imagePath = Path.Combine(EnvironmentPath, "CompactGUI", "SteamCache", $"{ActiveFolder.SteamAppID}.jpg")

        If Not Path.Exists(Path.GetDirectoryName(imagePath)) Then Directory.CreateDirectory(Path.GetDirectoryName(imagePath))

        If File.Exists(imagePath) Then
            SteamBGImage = Helper.LoadImageFromDisk(imagePath)
            Debug.WriteLine("Loaded Steam header image from disk")
            Return
        End If

        Dim url As String = $"https://steamcdn-a.akamaihd.net/steam/apps/{ActiveFolder.SteamAppID}/page_bg_generated_v6b.jpg"
        If SteamBGImage?.UriSource IsNot Nothing AndAlso SteamBGImage.UriSource.ToString() = url Then Return

        Try
            Using client As New HttpClient()
                Dim imageData As Byte() = Await client.GetByteArrayAsync(url)
                SteamBGImage = LoadImageFromMemoryStream(imageData)
                Await File.WriteAllBytesAsync(imagePath, imageData)
            End Using
        Catch ex As Exception
            Debug.WriteLine($"Failed to load Steam header image: {ex.Message}")
        End Try
    End Function



    Private Async Function AnalyseAsync() As Task

        Await Watcher.DisableBackgrounding

        State = UIState.AnalysingFolderSelected

        CancellationTokenSource = New CancellationTokenSource
        Dim token = CancellationTokenSource.Token

        Dim Analyser As New Core.Analyser(ActiveFolder.FolderName)

        If Not Analyser.HasDirectoryWritePermission Then
            Await InsufficientPermissionHandler()
            State = UIState.ValidFolderSelected
            Return
        End If

        Dim containsCompressedFiles = Await Analyser.AnalyseFolder(token)
        If CancellationTokenSource.IsCancellationRequested Then
            State = UIState.ValidFolderSelected
            Return
        End If

        ActiveFolder.AnalysisResults = Analyser.FileCompressionDetailsList
        ActiveFolder.CompressedBytes = Analyser.CompressedBytes
        ActiveFolder.UncompressedBytes = Analyser.UncompressedBytes

        Await UpdateWatcherAndStateAsync(containsCompressedFiles, Analyser)


        SubmitToWikiCommand.NotifyCanExecuteChanged()
        ChooseCompressionCommand.NotifyCanExecuteChanged()

        Await Watcher.EnableBackgrounding()

    End Function


    Private Async Function UpdateWatcherAndStateAsync(containsCompressedFiles As Boolean, Analyser As Core.Analyser) As Task

        If containsCompressedFiles OrElse ActiveFolder.IsFreshlyCompressed Then
            State = UIState.FolderCompressedResults

            If ActiveFolder.IsFreshlyCompressed Then
                ActiveFolder.PoorlyCompressedFiles = Await Analyser.GetPoorlyCompressedExtensions()
                If SettingsHandler.AppSettings.WatchFolderForChanges Then AddFolderToWatcher()
                Notify_Compressed(ActiveFolder.DisplayName, ActiveFolder.UncompressedBytes - ActiveFolder.CompressedBytes, ActiveFolder.CompressionRatio)
            Else
                Await GetWikiResultsAndSetPoorlyCompressedListAsync()
                Dim mainCompressionLVL = ActiveFolder.AnalysisResults.Select(Function(f) f.CompressionMode).Max
                Watcher.UpdateWatched(ActiveFolder.FolderName, Analyser)
                ActiveFolder.SelectedCompressionMode = Core.WOFConvertBackCompressionLevel(mainCompressionLVL)
            End If
        Else
            Watcher.UpdateWatched(ActiveFolder.FolderName, Analyser)
            Dim compRatioEstimate = Await GetWikiResultsAndSetPoorlyCompressedListAsync()
            ActiveFolder.CompressedBytes = compRatioEstimate
            State = UIState.FolderAnalysedResults
        End If

    End Function


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
        ActiveFolder.SelectedCompressionMode = BindableSettings.SelectedCompressionMode
        State = UIState.ChooseCompressionOptions
    End Sub


    Private Async Function CompressBeginAsync() As Task

        Await Watcher.DisableBackgrounding

        State = UIState.CurrentlyCompressing
        PauseResumeStatus = "Pause"
        CancelStatus = "Cancel"


        CProgress.Report((0, ""))

        Dim exclist As String() = GetSkipList()

        CoreCompactor = New Core.Compactor(ActiveFolder.FolderName, Core.WOFConvertCompressionLevel(ActiveFolder.SelectedCompressionMode), exclist)
        Dim HDDType As DiskDetector.Models.HardwareType = ActiveFolder.DiskType
        Dim IsLockedToOneThread As Boolean = SettingsHandler.AppSettings.LockHDDsToOneThread
        Dim res = Await CoreCompactor.RunCompactAsync(CProgress, If(HDDType = DiskDetector.Models.HardwareType.Hdd AndAlso IsLockedToOneThread, 1, SettingsHandler.AppSettings.MaxCompressionThreads))

        ActiveFolder.IsFreshlyCompressed = False
        If res Then ActiveFolder.IsFreshlyCompressed = True

        BindableSettings.SelectedCompressionMode = ActiveFolder.SelectedCompressionMode
        Settings.Save()

        Await AnalyseAsync()

        Await Watcher.EnableBackgrounding()

    End Function


    Private Function GetSkipList() As String()
        Dim exclist As String() = Array.Empty(Of String)()
        If SettingsHandler.AppSettings.SkipNonCompressable AndAlso SettingsHandler.AppSettings.NonCompressableList.Count <> 0 Then
            exclist = exclist.Union(SettingsHandler.AppSettings.NonCompressableList).ToArray
        End If
        If SettingsHandler.AppSettings.SkipUserNonCompressable AndAlso ActiveFolder.WikiPoorlyCompressedFiles.Count <> 0 Then
            exclist = exclist.Union(ActiveFolder.WikiPoorlyCompressedFiles).ToArray
        End If

        Return exclist

    End Function


    Private Async Function UncompressBeginAsync() As Task

        Await Watcher.DisableBackgrounding()

        State = UIState.CurrentlyCompressing
        PauseResumeStatus = "Pause"
        CancelStatus = "Cancel"

        CProgress.Report((0, ""))

        Dim compressedFilesList = ActiveFolder.AnalysisResults.Where(Function(rs) rs.CompressedSize < rs.UncompressedSize).Select(Of String)(Function(f) f.FileName).ToList
        CoreUncompactor = New Core.Uncompactor
        Await CoreUncompactor.UncompactFiles(compressedFilesList, CProgress, If(ActiveFolder.DiskType = DiskDetector.Models.HardwareType.Hdd AndAlso SettingsHandler.AppSettings.LockHDDsToOneThread, 1, SettingsHandler.AppSettings.MaxCompressionThreads))

        ActiveFolder.IsFreshlyCompressed = False

        Await AnalyseAsync()

        Await Watcher.EnableBackgrounding()
    End Function

    Dim CProgress As IProgress(Of (Integer, String)) = New Progress(Of (Integer, String))(Sub(val) WorkingProgress = New Tuple(Of Integer, String, Double, String)(val.Item1, val.Item2.Replace(ActiveFolder.FolderName, ""), val.Item1 / 100.0, WorkingProgress.Item4))


    Private Async Function GetWikiResultsAndSetPoorlyCompressedListAsync() As Task(Of Long)

        If ActiveFolder.SteamAppID = 0 Then Return 1010101010101010
        Dim res = Await WikiHandler.ParseData(ActiveFolder.SteamAppID)
        If res.Equals(Nothing) Then Return 1010101010101010

        'TODO: Modify the 100 cutoff based on level of aggressiveness selected by user in settings
        ActiveFolder.WikiPoorlyCompressedFiles = res.poorlyCompressedList.Where(Function(k) k.Value > 100 AndAlso k.Key <> "").Select(Function(k) k.Key).ToList
        ActiveFolder.CompressionConfidence = res.confidence
        ActiveFolder.WikiCompressionResults.Clear()

        For Each item In res.compressionResults
            ActiveFolder.WikiCompressionResults.Add(item)
        Next

        Return CLng(ActiveFolder.UncompressedBytes * res.estimatedRatio)

    End Function


    Private Async Function SubmitToWikiAsync() As Task
        ActiveFolder.IsFreshlyCompressed = False
        SubmitToWikiCommand.NotifyCanExecuteChanged()
        Dim successfullySent = Await WikiHandler.SubmitToWiki(ActiveFolder.FolderName, ActiveFolder.AnalysisResults, ActiveFolder.PoorlyCompressedFiles, ActiveFolder.SelectedCompressionMode)
        If Not successfullySent Then ActiveFolder.IsFreshlyCompressed = True
        SubmitToWikiCommand.NotifyCanExecuteChanged()
        ChooseCompressionCommand.NotifyCanExecuteChanged()
    End Function


    Private Function CanSubmitToWiki() As Boolean
        Return ActiveFolder.SteamAppID <> 0 AndAlso ActiveFolder.IsFreshlyCompressed AndAlso Not SettingsHandler.AppSettings.SkipUserNonCompressable AndAlso Not SettingsHandler.AppSettings.SkipNonCompressable
        'NEED TO RE-ADD CHECK TO NOT LET YOU SUBMIT IF YOU'RE SKIPPING FILES!!!!
    End Function


    Private Async Function InsufficientPermissionHandler() As Task

        Dim msg As New ContentDialog With {.Title = "Permissions Error"}
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
        Await msg.ShowAsync()
    End Function


    Private Sub RunAsAdmin()
        Dim myproc As New Process With {
            .StartInfo = New ProcessStartInfo With {
                .FileName = Environment.ProcessPath,
                .UseShellExecute = True,
                .Arguments = $"""{ActiveFolder.FolderName}""",
                .Verb = "runas"}
        }
        Dim app As Application = Application.Current
        app.ShutdownPipeServer().ContinueWith(
            Sub()
                app.Dispatcher.Invoke(
                    Sub()
                        Application.mutex.ReleaseMutex()
                        Application.mutex.Dispose()
                    End Sub
                )
                myproc.Start()
                app.Dispatcher.Invoke(Sub() app.Shutdown())
            End Sub
        )
    End Sub


    Private Async Function ManuallyAddFolderToWatcher() As Task

        Dim path As String = ""

        Dim folderSelector As New Microsoft.Win32.OpenFolderDialog
        folderSelector.ShowDialog()
        If folderSelector.FolderName = "" Then Return
        path = folderSelector.FolderName

        Dim validFolder = Core.verifyFolder(path)
        If Not validFolder.isValid Then
            Dim msgError As New ContentDialog With {.Title = "Invalid Folder", .Content = $"{validFolder.msg}", .CloseButtonText = "OK"}
            Await msgError.ShowAsync()
            Return
        End If

        Dim newFolder As New ActiveFolder With {.FolderName = path}

        Dim SteamFolderData = GetSteamNameAndIDFromFolder(path)

        newFolder.SteamAppID = SteamFolderData.appID
        newFolder.DisplayName = If(SteamFolderData.gameName, path)


        Dim newWatched = New Watcher.WatchedFolder With {
           .Folder = newFolder.FolderName,
           .DisplayName = newFolder.DisplayName,
           .IsSteamGame = newFolder.SteamAppID <> 0,
           .LastCompressedSize = 0,
           .LastUncompressedSize = 0,
           .LastCompressedDate = DateTime.UnixEpoch,
           .LastCheckedDate = DateTime.UnixEpoch,
           .LastCheckedSize = 0,
           .LastSystemModifiedDate = DateTime.UnixEpoch,
           .CompressionLevel = Core.CompressionAlgorithm.NO_COMPRESSION}

        Watcher.AddOrUpdateWatched(newWatched)
        Await Watcher.Analyse(path, True)

    End Function

    Public Async Function RefreshWatchedAsync() As Task

        Await Task.Run(Function() Watcher.ParseWatchers(True))
    End Function

    Public Enum UIState
        FreshLaunch
        ValidFolderSelected
        AnalysingFolderSelected
        FolderAnalysedResults
        FolderCompressedResults
        ChooseCompressionOptions
        CurrentlyCompressing
        FolderWatcherView
        UINothing
    End Enum


#Region "Properties"

    Private Property CoreCompactor As Core.Compactor
    Private Property CoreUncompactor As Core.Uncompactor
    Private Property CancellationTokenSource As CancellationTokenSource
    Public Property UpdateAvailable As New Tuple(Of Boolean, String)(False, Nothing)
    Public Property ActiveFolder As New ActiveFolder
    Public Property LastState As UIState
    Private _State As UIState
    Public Property State As UIState
        Get
            Return _State
        End Get
        Set(value As UIState)
            If State <> UIState.FolderWatcherView Then LastState = State
            _State = value

            If State = UIState.AnalysingFolderSelected Then
                WorkingProgress = New Tuple(Of Integer, String, Double, String)(WorkingProgress.Item1, WorkingProgress.Item2, WorkingProgress.Item3, "Indeterminate")
            ElseIf State = UIState.CurrentlyCompressing Then
                WorkingProgress = New Tuple(Of Integer, String, Double, String)(WorkingProgress.Item1, WorkingProgress.Item2, WorkingProgress.Item3, "Normal")
            Else
                WorkingProgress = New Tuple(Of Integer, String, Double, String)(WorkingProgress.Item1, WorkingProgress.Item2, WorkingProgress.Item3, "None")
            End If

        End Set
    End Property
    Public Property SteamBGImage As BitmapImage = Nothing
    Public Property WorkingProgress As New Tuple(Of Integer, String, Double, String)(0, "", 0, "None")
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
    Public ReadOnly Property Version As String = UpdateHandler.CurrentVersion.Friendly
    Public Property PauseResumeStatus As String = "Pause"
    Public Property CancelStatus As String = "Cancel"

#End Region

#Region "Commands"
    Public Property AnalyseFolderCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf AnalyseAsync)
    Public Property SubmitToWikiCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf SubmitToWikiAsync, AddressOf CanSubmitToWiki)
    Public Property ChooseCompressionCommand As RelayCommand = New RelayCommand(AddressOf ChooseCompression, Function() Not SubmitToWikiCommand.CanExecute(Nothing))
    Public Property CompressFolderCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf CompressBeginAsync)
    Public Property UncompressFolderCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf UncompressBeginAsync)
    Public Property MenuCompressionAreaCommand As RelayCommand = New RelayCommand(Sub() State = If(State = UIState.FolderWatcherView, LastState, State))
    Public Property MenuWatcherAreaCommand As RelayCommand = New RelayCommand(Sub() State = UIState.FolderWatcherView)
    Public Property RemoveWatcherCommand As ICommand = New RelayCommand(Of Watcher.WatchedFolder)(Sub(f) Watcher.RemoveWatched(f))
    Public Property ReCompressWatchedCommand As ICommand = New AsyncRelayCommand(Of Watcher.WatchedFolder)(Function(f) SelectFolderAsync(f.Folder))
    Public Property RefreshWatchedCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf RefreshWatchedAsync)
    Public Property ManuallyAddFolderToWatcherCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf ManuallyAddFolderToWatcher)
    Public Property CancelCommand As RelayCommand = New RelayCommand(Sub() CancellationTokenSource.Cancel())
    Public Property PauseCompressionCommand As RelayCommand = New RelayCommand(Sub()

                                                                                   If PauseResumeStatus = "Pause" Then
                                                                                       PauseResumeStatus = "Pausing..."
                                                                                       If CoreCompactor IsNot Nothing Then CoreCompactor.PauseCompression()
                                                                                       If CoreUncompactor IsNot Nothing Then CoreUncompactor.PauseCompression()
                                                                                       PauseResumeStatus = "Resume"
                                                                                   Else
                                                                                       If CoreCompactor IsNot Nothing Then CoreCompactor.ResumeCompression()
                                                                                       If CoreUncompactor IsNot Nothing Then CoreUncompactor.ResumeCompression()
                                                                                       PauseResumeStatus = "Pause"
                                                                                   End If

                                                                               End Sub)

    Public Property CancelCompressionCommand As RelayCommand = New RelayCommand(Sub()
                                                                                    CancelStatus = "Cancelling..."
                                                                                    If CoreCompactor IsNot Nothing Then CoreCompactor.Cancel()
                                                                                    If CoreUncompactor IsNot Nothing Then CoreUncompactor.Cancel()
                                                                                End Sub)

#End Region


End Class
