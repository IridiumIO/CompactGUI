
Imports System.ComponentModel

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Imports Wpf.Ui.Controls

<PropertyChanged.AddINotifyPropertyChangedInterface>
Public Class FolderViewModel : Inherits ObservableObject


    Public Event PropertyChanged As PropertyChangedEventHandler

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


    Private _watcher As Watcher.Watcher
    Private ReadOnly _snackbarService As CustomSnackBarService

    Public Sub New(folder As CompressableFolder, watcher As Watcher.Watcher, snackbarService As CustomSnackBarService)
        Me.Folder = folder
        _watcher = watcher
        _snackbarService = snackbarService
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
        _watcher.UpdateWatched(Folder.FolderName, Folder.Analyser, False)

    End Function


    Public ReadOnly Property ApplyToAllCommand As RelayCommand = New RelayCommand(AddressOf ApplyToAll)
    Private Sub ApplyToAll()
        Dim allFolders = Application.GetService(Of HomeViewModel)().Folders

        For Each fl In allFolders.Where(Function(f) f.FolderActionState <> ActionState.Analysing AndAlso f.FolderActionState <> ActionState.Working AndAlso f.FolderActionState <> ActionState.Paused)
            If fl IsNot Folder Then
                fl.CompressionOptions = Folder.CompressionOptions.Clone
                fl.FolderActionState = ActionState.Idle
            End If
        Next


        _snackbarService.ShowAppliedToAllFolders()
    End Sub


    Public ReadOnly Property PauseCommand As IRelayCommand = New RelayCommand(Sub()

                                                                                  If Folder.FolderActionState = ActionState.Working Then
                                                                                      Folder.Compressor?.Pause()
                                                                                      Folder.FolderActionState = ActionState.Paused
                                                                                  Else
                                                                                      Folder.Compressor?.Resume()
                                                                                      Folder.FolderActionState = ActionState.Working

                                                                                  End If
                                                                              End Sub)

    Public ReadOnly Property CancelCommand As IRelayCommand = New RelayCommand(Sub() Folder.Compressor?.Cancel())

    Public ReadOnly Property SubmitToWikiCommand As IRelayCommand = New AsyncRelayCommand(AddressOf SubmitToWiki, AddressOf CanSubmitToWiki)

    Private Async Function SubmitToWiki() As Task

        SubmitToWikiCommand.NotifyCanExecuteChanged()

        Dim result = Await Application.GetService(Of IWikiService).SubmitToWiki(Folder.FolderName, Folder.AnalysisResults.ToList, Folder.PoorlyCompressedFiles, Folder.CompressionOptions.SelectedCompressionMode)

        Folder.IsFreshlyCompressed = False
        SubmitToWikiCommand.NotifyCanExecuteChanged()
    End Function

    Private Function CanSubmitToWiki() As Boolean
        Return TypeOf (Folder) _
            Is SteamFolder AndAlso
            Folder.IsFreshlyCompressed AndAlso
            Not Folder.CompressionOptions.SkipPoorlyCompressedFileTypes AndAlso
            Not Folder.CompressionOptions.SkipUserSubmittedFiletypes
    End Function


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






End Class

