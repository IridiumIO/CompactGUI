Imports System.Windows.Media.Animation
Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports Microsoft.Toolkit.Mvvm.Input
Imports ModernWpf.Controls
Imports Ookii.Dialogs.Wpf

Public Class MainViewModel : Inherits ObservableObject


    Sub New()
        SettingsHandler.InitialiseSettings()

        WikiHandler.GetUpdatedJSON()

        FireAndForgetCheckForUpdates()


    End Sub

    Public Property UpdateAvailable As New Tuple(Of Boolean, String)(False, Nothing)
    Public Property SelectFolderCommand As ICommand = New RelayCommand(AddressOf SelectFolder)
    Public Property ActiveFolder As New ActiveFolder
    Public Property State As String
    Public Property SteamBGImage As BitmapImage = Nothing

    Private Async Sub FireAndForgetCheckForUpdates()
        Await Task.Delay(2000)
        Dim ret = Await UpdateHandler.CheckForUpdate(True)
        If ret Then
            UpdateAvailable = New Tuple(Of Boolean, String)(True, "update available  -  v" & UpdateHandler.NewVersion.ToString)

        End If
    End Sub


    Public Sub SelectFolder(Optional path As String = Nothing)
        If path Is Nothing Then
            Dim folderSelector As New VistaFolderBrowserDialog
            folderSelector.ShowDialog()

            If folderSelector.SelectedPath = "" Then Return
            path = folderSelector.SelectedPath
        End If
        Dim validFolder = Core.verifyFolder(path)
        If Not validFolder Then

            Dim msgError As New ContentDialog With {
            .Title = "Invalid Folder",
            .Content = "For safety, this folder cannot be chosen.",
            .CloseButtonText = "OK"
            }

            msgError.ShowAsync()

            Return
        End If


        UpdateAvailable = New Tuple(Of Boolean, String)(True, path)

        ActiveFolder.folderName = path
        ActiveFolder.steamAppID = GetSteamIDFromFolder(path)

        State = "ValidFolderSelected"

        FireAndForgetGetSteamHeader()


    End Sub

    Private Sub FireAndForgetGetSteamHeader()
        Dim url As String = $"https://steamcdn-a.akamaihd.net/steam/apps/{ActiveFolder.steamAppID}/page_bg_generated_v6b.jpg"
        Dim bImg As New BitmapImage(New Uri(url))
        If Not SteamBGImage?.UriSource Is Nothing AndAlso SteamBGImage.UriSource = bImg.UriSource Then Return
        SteamBGImage = bImg
    End Sub





End Class











Public Class ImageControl : Inherits Image

    Public Shared ReadOnly SourceChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("SourceChanged", RoutingStrategy.Direct, GetType(RoutedEventHandler), GetType(ImageControl))

    Shared Sub New()
        Image.SourceProperty.OverrideMetadata(GetType(ImageControl), New FrameworkPropertyMetadata(Nothing, AddressOf SourcePropertyChanged))
    End Sub

    Public Custom Event SourceChanged As RoutedEventHandler
        AddHandler(ByVal value As RoutedEventHandler)
            Task.Delay(1000)

            [AddHandler](SourceChangedEvent, value)
        End AddHandler
        RemoveHandler(ByVal value As RoutedEventHandler)
            [RemoveHandler](SourceChangedEvent, value)
        End RemoveHandler
        RaiseEvent(sender As Object, e As RoutedEventArgs)

            [RaiseEvent](e)
        End RaiseEvent
    End Event

    Private Shared Sub SourcePropertyChanged(ByVal obj As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        Dim image As Image = TryCast(obj, Image)
        If image IsNot Nothing Then
            image.[RaiseEvent](New RoutedEventArgs(SourceChangedEvent))
        End If
    End Sub
End Class








Public Class VisualStateApplier
    Public Shared Function GetVisualState(ByVal target As DependencyObject) As String
        Return TryCast(target.GetValue(VisualStateProperty), String)
    End Function

    Public Shared Sub SetVisualState(ByVal target As DependencyObject, ByVal value As String)
        target.SetValue(VisualStateProperty, value)
    End Sub

    Public Shared ReadOnly VisualStateProperty As DependencyProperty = DependencyProperty.RegisterAttached("VisualState", GetType(String), GetType(VisualStateApplier), New PropertyMetadata(Nothing, AddressOf VisualStatePropertyChangedCallback))

    Private Shared Sub VisualStatePropertyChangedCallback(ByVal target As DependencyObject, ByVal args As DependencyPropertyChangedEventArgs)
        VisualStateManager.GoToElementState(CType(target, FrameworkElement), TryCast(args.NewValue, String), True)
    End Sub
End Class