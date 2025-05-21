Public Class SettingsPage

    Sub New(settingsviewmodel As SettingsViewModel)

        InitializeComponent()


        DataContext = settingsviewmodel


        ScrollViewer.SetCanContentScroll(Me, False)

    End Sub




End Class
