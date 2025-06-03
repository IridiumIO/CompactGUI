Imports CommunityToolkit.Mvvm.Input

Imports CompactGUI.Core.SharedMethods

Imports Wpf.Ui.Controls

Public Class CustomSnackBarService
    Inherits Wpf.Ui.SnackbarService

    Public _snackbar As Snackbar

    Public Sub ShowCustom(message As UIElement, title As String, appearance As ControlAppearance, Optional icon As IconElement = Nothing, Optional timeout As TimeSpan = Nothing)

        If GetSnackbarPresenter() Is Nothing Then Throw New InvalidOperationException("The SnackbarPresenter was never set")
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
            If InvalidMessages(i) = FolderVerificationResult.InsufficientPermission Then
                ShowInsufficientPermission(InvalidFolders(i))
                Return
            End If

            messageString &= $"{InvalidFolders(i)}: {GetFolderVerificationMessage(InvalidMessages(i))}" & vbCrLf
        Next

        Show("Invalid Folders", messageString, Wpf.Ui.Controls.ControlAppearance.Danger, Nothing, TimeSpan.FromSeconds(10))

    End Sub

    Public Sub ShowInsufficientPermission(folderName As String)
        Dim button = New Button With {
            .Content = "Restart as Admin",
            .Command = New RelayCommand(Sub() RunAsAdmin(folderName)),
            .Margin = New Thickness(-3, 10, 0, 0)
        }
        ShowCustom(button, "Insufficient permission to access this folder.", ControlAppearance.Danger, timeout:=TimeSpan.FromSeconds(60))
    End Sub

    Public Sub ShowUpdateAvailable(newVersion As String, isPreRelease As Boolean)
        Dim textBlock = New TextBlock
        textBlock.Text = "Click to download"

        ' Show the custom snackbar
        ShowCustom(textBlock, $"Update Available ▸ Version {newVersion}", If(isPreRelease, ControlAppearance.Info, ControlAppearance.Success), timeout:=TimeSpan.FromSeconds(10))

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
        Show("Failed to submit to wiki", "Please check your internet connection and try again", Wpf.Ui.Controls.ControlAppearance.Danger, Nothing, TimeSpan.FromSeconds(5))
    End Sub

    Public Sub ShowSubmittedToWiki(steamsubmitdata As SteamSubmissionData, compressionMode As Integer)
        Show("Submitted to wiki", $"UID: {steamsubmitdata.UID}{vbCrLf}Game: {steamsubmitdata.GameName}{vbCrLf}SteamID: {steamsubmitdata.SteamID}{vbCrLf}Compression: {[Enum].GetName(GetType(Core.WOFCompressionAlgorithm), Core.WOFConvertCompressionLevel(compressionMode))}", Wpf.Ui.Controls.ControlAppearance.Success, Nothing, TimeSpan.FromSeconds(10))
    End Sub


    Public Sub ShowAppliedToAllFolders()
        Show("Applied to all folders", "Compression options have been applied to all folders", Wpf.Ui.Controls.ControlAppearance.Success, Nothing, TimeSpan.FromSeconds(5))
    End Sub

    Public Sub ShowCannotRemoveFolder()
        Show("Cannot remove folder", "Please wait until the current operation is finished", Wpf.Ui.Controls.ControlAppearance.Caution, Nothing, TimeSpan.FromSeconds(5))
    End Sub

    Public Sub ShowAddedToQueue()
        Show("Success", "Added to Queue", Wpf.Ui.Controls.ControlAppearance.Success, Nothing, TimeSpan.FromSeconds(5))
    End Sub

End Class