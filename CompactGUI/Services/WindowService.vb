Public Interface IWindowService
    Sub ShowMainWindow()
    Sub MinimizeMainWindow()
    Sub HideMainWindow()
    Function ShowMessageBox(title As String, content As String) As Task(Of Boolean)
End Interface


Public Class WindowService
    Implements IWindowService

    Public Sub ShowMainWindow() Implements IWindowService.ShowMainWindow
        Dim mainWindow = Application.GetService(Of MainWindow)()
        mainWindow.Show()
        mainWindow.WindowState = WindowState.Normal
        mainWindow.Topmost = True
        mainWindow.Activate()
        mainWindow.Topmost = False
    End Sub

    Public Sub MinimizeMainWindow() Implements IWindowService.MinimizeMainWindow
        Dim mainWindow = Application.GetService(Of MainWindow)()
        mainWindow.WindowState = WindowState.Minimized
    End Sub

    Public Sub HideMainWindow() Implements IWindowService.HideMainWindow
        Dim mainWindow = Application.GetService(Of MainWindow)()
        mainWindow.Hide()
    End Sub

    Public Async Function ShowMessageBox(title As String, content As String) As Task(Of Boolean) Implements IWindowService.ShowMessageBox
        Dim msgBox = New Wpf.Ui.Controls.MessageBox With {
               .Title = title,
               .Content = content,
               .IsPrimaryButtonEnabled = True,
               .PrimaryButtonText = "Yes",
               .CloseButtonText = "Cancel"
           }
        Dim result = Await msgBox.ShowDialogAsync()
        Return result = Wpf.Ui.Controls.MessageBoxResult.Primary
    End Function
End Class
