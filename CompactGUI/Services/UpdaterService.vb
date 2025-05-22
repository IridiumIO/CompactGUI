Imports System.Net.Http
Imports System.Text.Json

Imports Wpf.Ui.Controls

Public Interface IUpdaterService
    Function CheckForUpdate(includePrerelease As Boolean) As Task
End Interface

Public Class UpdaterService : Implements IUpdaterService

    Public Shared ReadOnly UpdateURL As String = "https://raw.githubusercontent.com/IridiumIO/CompactGUI/database/version.json"
    Public Shared ReadOnly httpClient As New HttpClient()


    Public Async Function CheckForUpdate(includePrerelease As Boolean) As Task Implements IUpdaterService.CheckForUpdate
        Try
            Dim ret = Await httpClient.GetStringAsync(UpdateURL)
            Dim jVer = JsonSerializer.Deserialize(Of Dictionary(Of String, SemVersion))(ret)
            Dim newVersion As SemVersion = If(includePrerelease, jVer("Latest"), jVer("LatestNonPreRelease"))
            If newVersion > Application.AppVersion Then
                ShowUpdateSnackbar(newVersion, newVersion.IsPreRelease)
            End If
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
    End Function

    Public Sub ShowUpdateSnackbar(newVersion As SemVersion, Optional isPreRelease As Boolean = False)

        Dim snackbarSV = Application.GetService(Of CustomSnackBarService)()

        Dim textBlock = New TextBlock
        textBlock.Text = "Click to download"

        ' Show the custom snackbar
        snackbarSV.ShowCustom(textBlock, $"Update Available ▸ Version {newVersion.Friendly}", If(isPreRelease, ControlAppearance.Info, ControlAppearance.Success), timeout:=TimeSpan.FromSeconds(10))

        Dim handler As MouseButtonEventHandler = Nothing
        Dim closedHandler As TypedEventHandler(Of Snackbar, RoutedEventArgs) = Nothing

        handler = Sub(sender, e)
                      Process.Start(New ProcessStartInfo("https://github.com/IridiumIO/CompactGUI/releases/") With {.UseShellExecute = True})
                      RemoveHandler snackbarSV.GetSnackbarPresenter.MouseDown, handler
                      RemoveHandler snackbarSV._snackbar.Closed, closedHandler
                  End Sub

        closedHandler = Sub(sender, e)
                            RemoveHandler snackbarSV.GetSnackbarPresenter.MouseDown, handler
                            RemoveHandler snackbarSV._snackbar.Closed, closedHandler
                        End Sub

        AddHandler snackbarSV.GetSnackbarPresenter.MouseDown, handler
        AddHandler snackbarSV._snackbar.Closed, closedHandler
    End Sub


End Class
