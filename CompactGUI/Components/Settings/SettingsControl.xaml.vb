Public Class SettingsControl
    Public Sub New()

        InitializeComponent()
        uiSettingsNavView.SelectedItem = uiSettingsNavView.MenuItems(0)

        AddExecutableToRegistry()

    End Sub

    Private Sub NavigationView_SelectionChanged(sender As ModernWpf.Controls.NavigationView, args As ModernWpf.Controls.NavigationViewSelectionChangedEventArgs)

        If args.SelectedItemContainer Is Nothing Then Return

        Dim navItemTag = args.SelectedItemContainer.Tag

        If navItemTag Is "main" Then
            ContentFrame.Content = New Settings_main
        End If


    End Sub

    Public Shared Sub AddExecutableToRegistry()
        Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\software\IridiumIO\CompactGUI\", "Executable Path", IO.Directory.GetCurrentDirectory)
    End Sub

End Class
