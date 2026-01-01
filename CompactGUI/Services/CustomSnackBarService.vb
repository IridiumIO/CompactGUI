Imports CommunityToolkit.Mvvm.Input

Imports CompactGUI.Core.SharedMethods
Imports CompactGUI.Logging

Imports Microsoft.Extensions.Logging

Imports Wpf.Ui.Controls

Public Class CustomSnackBarService
    Inherits Wpf.Ui.SnackbarService

    Private ReadOnly logger As ILogger(Of CustomSnackBarService)
    Public _snackbar As Snackbar

    Public Sub New(logger As ILogger(Of CustomSnackBarService))
        MyBase.New()
        Me.logger = logger
    End Sub

    Public Sub ShowCustom(message As UIElement, title As String, appearance As ControlAppearance, Optional icon As IconElement = Nothing, Optional timeout As TimeSpan = Nothing)

        If GetSnackbarPresenter() Is Nothing Then Throw New InvalidOperationException(LanguageHelper.GetString("SnackBar_SnackbarPresenter")) 'The SnackbarPresenter was never set
        If _snackbar Is Nothing Then _snackbar = New Snackbar(GetSnackbarPresenter())

        _snackbar.SetCurrentValue(Snackbar.TitleProperty, title)
        _snackbar.SetCurrentValue(ContentControl.ContentProperty, message)
        _snackbar.SetCurrentValue(Snackbar.AppearanceProperty, appearance)
        _snackbar.SetCurrentValue(Snackbar.IconProperty, icon)
        _snackbar.SetCurrentValue(Snackbar.TimeoutProperty, If(timeout = Nothing, DefaultTimeOut, timeout))

        _snackbar.Show(True)
    End Sub



    Public Sub ShowInvalidFoldersMessage(InvalidFolders As List(Of String), InvalidMessages As List(Of FolderVerificationResult))

        Dim messageString = ""
        For i = 0 To InvalidFolders.Count - 1
            SnackbarServiceLog.ShowInvalidFoldersMessage(logger, InvalidFolders(i), GetFolderVerificationMessage(InvalidMessages(i)))
            If InvalidFolders.Count = 1 AndAlso InvalidMessages(i) = FolderVerificationResult.InsufficientPermission Then
                ShowInsufficientPermission(InvalidFolders(i))
                Return
            End If
            messageString &= $"{InvalidFolders(i)}: {GetFolderVerificationMessage(InvalidMessages(i))}" & vbCrLf
        Next

        Show("Invalid Folders", messageString, Wpf.Ui.Controls.ControlAppearance.Danger, Nothing, TimeSpan.FromSeconds(10))

    End Sub

    Public Sub ShowInsufficientPermission(folderName As String)
        Dim button = New Button With {
            .Content = LanguageHelper.GetString("SnackBar_RestartAdmin"), '"Restart as Admin"
            .Command = New RelayCommand(Sub() RunAsAdmin(folderName)),
            .Margin = New Thickness(-3, 10, 0, 0)
        }
        ShowCustom(button, LanguageHelper.GetString("SnackBar_RestartAdminTip"), ControlAppearance.Danger, timeout:=TimeSpan.FromSeconds(60)) '"Insufficient permission to access this folder."
    End Sub

    Public Sub ShowUpdateAvailable(newVersion As String, isPreRelease As Boolean)
        Dim textBlock = New TextBlock
        textBlock.Text = LanguageHelper.GetString("SnackBar_UpdateDownload") '"Click to download"

        ' Show the custom snackbar
        SnackbarServiceLog.ShowUpdateAvailable(logger, newVersion, isPreRelease)
        Dim title As String = String.Format(LanguageHelper.GetString("SnackBar_UpdateAvailable"), newVersion) 'Update Available ▸ Version {newVersion}
        ShowCustom(textBlock, title, If(isPreRelease, ControlAppearance.Info, ControlAppearance.Success), timeout:=TimeSpan.FromSeconds(10))

        Dim handler As MouseButtonEventHandler = Nothing
        Dim closedHandler As TypedEventHandler(Of Snackbar, RoutedEventArgs) = Nothing

        handler = Sub(sender, e)
                      Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI/releases/") With {.UseShellExecute = True})
                      RemoveHandler Me.GetSnackbarPresenter.MouseDown, handler
                      RemoveHandler Me._snackbar.Closed, closedHandler
                  End Sub

        closedHandler = Sub(sender, e)
                            RemoveHandler Me.GetSnackbarPresenter.MouseDown, handler
                            RemoveHandler Me._snackbar.Closed, closedHandler
                        End Sub

        AddHandler Me.GetSnackbarPresenter.MouseDown, handler
        AddHandler Me._snackbar.Closed, closedHandler
    End Sub

    Public Sub ShowFailedToSubmitToWiki()
        Show(LanguageHelper.GetString("SnackBar_SubmitWikiFailed"), LanguageHelper.GetString("SnackBar_SubmitWikiFailedTip"), Wpf.Ui.Controls.ControlAppearance.Danger, Nothing, TimeSpan.FromSeconds(5))
        '"Failed to submit to wiki", "Please check your internet connection and try again"
        SnackbarServiceLog.ShowFailedToSubmitToWiki(logger)
    End Sub

    Public Sub ShowSubmittedToWiki(steamsubmitdata As SteamSubmissionData, compressionMode As Integer)
        Dim compressionName As String = [Enum].GetName(GetType(Core.WOFCompressionAlgorithm), Core.WOFHelper.WOFConvertCompressionLevel(compressionMode))
        Dim message As String = $"{LanguageHelper.GetString("SnackBar_SubmitWiki_UID")}: {steamsubmitdata.UID}{vbCrLf}" &
                           $"{LanguageHelper.GetString("SnackBar_SubmitWiki_Game")}: {steamsubmitdata.GameName}{vbCrLf}" &
                           $"{LanguageHelper.GetString("SnackBar_SubmitWiki_SteamID")}: {steamsubmitdata.SteamID}{vbCrLf}" &
                           $"{LanguageHelper.GetString("SnackBar_SubmitWiki_Compression")}: {compressionName}"
        'Show("Submitted to wiki", $"UID: {0}{1}Game: {2}{1}SteamID: {3}{1}Compression: {4}

        Show(LanguageHelper.GetString("SnackBar_SubmitWikiTitle"), message, Wpf.Ui.Controls.ControlAppearance.Success, Nothing, TimeSpan.FromSeconds(10))
        SnackbarServiceLog.ShowSubmittedToWiki(logger, steamsubmitdata.UID, steamsubmitdata.GameName, steamsubmitdata.SteamID, steamsubmitdata.CompressionMode)
    End Sub


    Public Sub ShowAppliedToAllFolders()
        Show(LanguageHelper.GetString("SnackBar_AppliedAllFolders"), LanguageHelper.GetString("SnackBar_AppliedAllFoldersTip"), Wpf.Ui.Controls.ControlAppearance.Success, Nothing, TimeSpan.FromSeconds(5))
        '"Applied to all folders", "Compression options have been applied to all folders"
        SnackbarServiceLog.ShowAppliedToAllFolders(logger)
    End Sub

    Public Sub ShowCannotRemoveFolder()
        Show(LanguageHelper.GetString("SnackBar_CannotRemoveFolder"), LanguageHelper.GetString("SnackBar_CannotRemoveFolderTip"), Wpf.Ui.Controls.ControlAppearance.Caution, Nothing, TimeSpan.FromSeconds(5))
        '"Cannot remove folder", "Please wait until the current operation is finished"
        SnackbarServiceLog.ShowCannotRemoveFolder(logger)
    End Sub

    Public Sub ShowAddedToQueue()
        Show(LanguageHelper.GetString("SnackBar_Success"), LanguageHelper.GetString("SnackBar_SuccessTip"), Wpf.Ui.Controls.ControlAppearance.Success, Nothing, TimeSpan.FromSeconds(5))
        '"Success", "Added to Queue"
        SnackbarServiceLog.ShowAddedToQueue(logger)
    End Sub

    Public Sub ShowDirectStorageWarning(displayName As String)
        Show(displayName,
            LanguageHelper.GetString("SnackBar_DirectStorageTechnology"),
            Wpf.Ui.Controls.ControlAppearance.Info,
            Nothing,
            TimeSpan.FromSeconds(20))
        '"This game uses DirectStorage technology. If you are using this feature, you should not compress this game.",
        SnackbarServiceLog.ShowDirectStorageWarning(logger, displayName)
    End Sub
End Class