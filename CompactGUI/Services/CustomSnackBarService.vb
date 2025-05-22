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
End Class