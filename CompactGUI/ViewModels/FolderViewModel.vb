Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports System.Windows.Forms.AxHost

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Imports CompactGUI.MainViewModel

Imports MeasurePerformance.IL.Weaver

Imports PropertyChanged

Imports Wpf.Ui

Imports Wpf.Ui.Controls

Public Class FolderViewModel : Inherits ObservableObject




    Public Property Folder As CompressableFolder

    Public ReadOnly Property IsAnalysing As Boolean
        Get
            Return Folder?.FolderActionState = ActionState.Analysing
        End Get

    End Property

    Public ReadOnly Property IsNotResultsOrAnalysing As Boolean
        Get
            Return Folder?.FolderActionState <> ActionState.Results AndAlso Not IsAnalysing
        End Get

    End Property

    Public ReadOnly Property CompressionDisplayLevel As String
        Get

            If Folder.AnalysisResults Is Nothing OrElse
                Not Folder.AnalysisResults.Any(Function(x) x.CompressionMode <> Core.WOFCompressionAlgorithm.NO_COMPRESSION) Then
                Return "Not Compressed"
            End If

            Return "Compressed"

        End Get
    End Property


    Public ReadOnly Property TotalCompressedFiles As Integer
        Get
            Return Folder.AnalysisResults.Where(Function(x) x.CompressionMode <> Core.WOFCompressionAlgorithm.NO_COMPRESSION).Count()
        End Get
    End Property

    Public ReadOnly Property DominantCompressionMode As Core.WOFCompressionAlgorithm
        Get
            Return Folder?.AnalysisResults _
            .Where(Function(x) x.CompressionMode <> Core.WOFCompressionAlgorithm.NO_COMPRESSION) _
            .GroupBy(Function(x) x.CompressionMode) _
            .OrderByDescending(Function(g) g.Count()) _
            .Select(Function(g) g.Key) _
            .FirstOrDefault()
        End Get
    End Property


    Public Sub New(folder As CompressableFolder)
        Me.Folder = folder
        AddHandler folder.PropertyChanged, AddressOf OnFolderPropertyChanged
        AddHandler folder.CompressionProgressChanged, AddressOf OnCompressionProgressChanged
        AddHandler folder.CompressionOptions.PropertyChanged, AddressOf OnCompressionOptionsPropertyChanged
    End Sub

    Private Sub OnCompressionOptionsPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        Dim compressionOptions = CType(sender, CompressionOptions)
        If e.PropertyName = NameOf(compressionOptions.SelectedCompressionMode) Then
            OnPropertyChanged(NameOf(DisplayedFolderAfterSize))
        End If
    End Sub

    Public Property CompressionProgress As Integer
    Public Property CompressionProgressFile As String

    Private Sub OnCompressionProgressChanged(sender As Object, e As Core.CompressionProgress)
        CompressionProgress = e.ProgressPercent
        CompressionProgressFile = e.FileName.Replace(Folder.FolderName, "")
    End Sub

    Private Sub OnFolderPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
        If e.PropertyName = NameOf(Folder.FolderActionState) Then
            Debug.WriteLine("FolderActionState changed to " & Folder.FolderActionState.ToString())
            OnPropertyChanged(NameOf(IsAnalysing))
            OnPropertyChanged(NameOf(IsNotResultsOrAnalysing))
            OnPropertyChanged(NameOf(CompressionDisplayLevel))
            OnPropertyChanged(NameOf(DisplayedFolderAfterSize))

        End If
    End Sub


    Public ReadOnly Property CompressAgainCommand As RelayCommand = New RelayCommand(Sub() Folder.FolderActionState = ActionState.Idle)

    Public ReadOnly Property UncompressCommand As AsyncRelayCommand = New AsyncRelayCommand(AddressOf UncompressFolderAsync)

    Public Async Function UncompressFolderAsync() As Task
        Await Folder.UncompressFolder()
    End Function


    Public ReadOnly Property ApplyToAllCommand As RelayCommand = New RelayCommand(AddressOf ApplyToAll)
    Private Sub ApplyToAll()
        Dim allFolders = Application.GetService(Of HomeViewModel)().Folders

        For Each fl In allFolders
            If fl IsNot Folder Then
                fl.CompressionOptions = Folder.CompressionOptions.Clone
                fl.FolderActionState = ActionState.Idle
            End If
        Next

        Dim snackbar = Application.GetService(Of CustomSnackBarService)()
        snackbar.Show("Applied to all folders", "Success", Wpf.Ui.Controls.ControlAppearance.Success, Nothing, TimeSpan.FromSeconds(5))

    End Sub


    Public ReadOnly Property PauseCommand As IRelayCommand = New RelayCommand(Sub()

                                                                                  If Folder.FolderActionState = ActionState.Working Then
                                                                                      Folder.Compactor?.PauseCompression()
                                                                                      Folder.Uncompactor?.PauseCompression()
                                                                                      Folder.FolderActionState = ActionState.Paused
                                                                                  Else
                                                                                      Folder.Compactor?.ResumeCompression()
                                                                                      Folder.Uncompactor?.ResumeCompression()
                                                                                      Folder.FolderActionState = ActionState.Working

                                                                                  End If
                                                                              End Sub)

    Public ReadOnly Property CancelCommand As RelayCommand = New RelayCommand(Sub()

                                                                                  Folder.Compactor?.Cancel()
                                                                                  Folder.Uncompactor?.Cancel()

                                                                              End Sub)



    Public ReadOnly Property DisplayedFolderAfterSize As Long
        Get
            If TypeOf (Folder) Is SteamFolder AndAlso (Folder.FolderActionState = ActionState.Idle OrElse Folder.FolderActionState = ActionState.Working) Then
                Dim working = CType(Folder, SteamFolder)
                If working.WikiCompressionResults Is Nothing Then Return Folder.CompressedBytes
                Select Case working.CompressionOptions.SelectedCompressionMode
                    Case Core.CompressionMode.XPRESS4K
                        Return CLng(working.WikiCompressionResults.XPress4K?.CompressionPercent / 100 * working.UncompressedBytes)
                    Case Core.CompressionMode.XPRESS8K
                        Return CLng(working.WikiCompressionResults.XPress8K?.CompressionPercent / 100 * working.UncompressedBytes)
                    Case Core.CompressionMode.XPRESS16K
                        Return CLng(working.WikiCompressionResults.XPress16K?.CompressionPercent / 100 * working.UncompressedBytes)
                    Case Core.CompressionMode.LZX
                        Return CLng(working.WikiCompressionResults.LZX?.CompressionPercent / 100 * working.UncompressedBytes)
                End Select


            End If
            Return Folder.CompressedBytes
        End Get
    End Property




    Public Shared Async Function InsufficientPermissionHandler() As Task

        Dim snackbarSV = Application.GetService(Of CustomSnackBarService)()

        Dim button = New Wpf.Ui.Controls.HyperlinkButton
        button.Background = System.Windows.Media.Brushes.Transparent

        button.Content = "Restart as Admin"
        button.Command = New RelayCommand(Async Sub()
                                              Dim msg As New Wpf.Ui.Controls.MessageBox With {
                                                    .Title = "Restart as Admin",
                                                    .Content = "TODO."
                                                }

                                              Await msg.ShowDialogAsync()
                                          End Sub)
        button.Padding = New Thickness(3)

        ' Show the custom snackbar
        snackbarSV.ShowCustom(button, "Insufficient permission to access this folder.", ControlAppearance.Dark, timeout:=TimeSpan.FromSeconds(60))

    End Function



End Class

