Public Class SettingsView

    Public Property SettingsVM As New SettingsViewModel

    Sub New()

        Me.DataContext = SettingsVM

        InitializeComponent()

    End Sub




End Class
