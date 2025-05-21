Public Class SettingsPage

    Sub New(settingsviewmodel As SettingsViewModel)

        InitializeComponent()


        Me.DataContext = settingsviewmodel


        ScrollViewer.SetCanContentScroll(Me, False)

    End Sub




End Class
