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
                Application.GetService(Of CustomSnackBarService).ShowUpdateAvailable(newVersion.Friendly, newVersion.IsPreRelease)
            End If
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
    End Function


End Class
