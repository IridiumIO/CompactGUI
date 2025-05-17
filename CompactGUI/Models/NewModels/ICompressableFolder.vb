Imports System.Collections.ObjectModel
Imports System.Threading

Imports CommunityToolkit.Mvvm.ComponentModel

Imports CompactGUI.Core

Imports IWshRuntimeLibrary

Imports PropertyChanged


'Need this abstract class so we can use it in XAML
Public MustInherit Class CompressableFolder : Inherits ObservableObject


    Public Property FolderName As String
    Public Property DisplayName As String
    Public Property CurrentCompression As CompressionMode

    <AlsoNotifyFor(NameOf(BytesSaved), NameOf(CompressionRatio))>
    Public Property FolderActionState As ActionState

    Public Property UncompressedBytes As Long = 0
    Public Property CompressedBytes As Long = 0
    Public Property AnalysisResults As New ObservableCollection(Of AnalysedFileDetails)
    Public Property PoorlyCompressedFiles As List(Of ExtensionResult)
    Public Property CompressionOptions As CompressionOptions
    Public Property IsFreshlyCompressed As Boolean


    Public ReadOnly Property BytesSaved As Long
        Get
            Return UncompressedBytes - CompressedBytes
        End Get
    End Property

    Public ReadOnly Property CompressionRatio As Decimal
        Get
            If CompressedBytes = 0 Then Return 0
            Return CompressedBytes / UncompressedBytes
        End Get
    End Property


    Sub New()
        CompressionOptions = New CompressionOptions
    End Sub


    Public Property CompressionProgress As IProgress(Of CompressionProgress) = New Progress(Of CompressionProgress)(Sub(x As CompressionProgress)
                                                                                                                        ReportProgressChanged(x)
                                                                                                                    End Sub)


    Public Event CompressionProgressChanged As EventHandler(Of CompressionProgress)
    Public Sub ReportProgressChanged(e As CompressionProgress)
        RaiseEvent CompressionProgressChanged(Me, e)
    End Sub



    Public Compactor As Core.Compactor

    Public Async Function CompressFolder() As Task(Of Boolean)
        FolderActionState = ActionState.Working

        CompressionProgress.Report(New Core.CompressionProgress(0, ""))


        Compactor = New Core.Compactor(FolderName, Core.WOFConvertCompressionLevel(CompressionOptions.SelectedCompressionMode), CompressionOptions.GetExclusionList)

        'Dim HDDType As DiskDetector.Models.HardwareType = ActiveFolder.DiskType
        Dim IsLockedToOneThread As Boolean = SettingsHandler.AppSettings.LockHDDsToOneThread
        Debug.WriteLine(SettingsHandler.AppSettings.MaxCompressionThreads)
        Dim res = Await Compactor.RunCompactAsync(CompressionProgress, SettingsHandler.AppSettings.MaxCompressionThreads)

        If res Then
            FolderActionState = ActionState.Results
            IsFreshlyCompressed = True
        Else
            FolderActionState = ActionState.Results
            IsFreshlyCompressed = False
        End If

        Return res

    End Function


    Public Uncompactor As Core.Uncompactor

    Public Async Function UncompressFolder() As Task(Of Boolean)
        FolderActionState = ActionState.Working
        CompressionProgress.Report(New Core.CompressionProgress(0, ""))

        Uncompactor = New Core.Uncompactor

        Dim compressedFilesList = AnalysisResults.Where(Function(rs) rs.CompressedSize < rs.UncompressedSize).Select(Of String)(Function(f) f.FileName).ToList

        Dim res = Await Uncompactor.UncompactFiles(compressedFilesList, CompressionProgress, SettingsHandler.AppSettings.MaxCompressionThreads)

        FolderActionState = ActionState.Idle
        IsFreshlyCompressed = False

        Await AnalyseFolderAsync()
        Return res
    End Function



    Private CancellationTokenSource As CancellationTokenSource

    Public Async Function AnalyseFolderAsync() As Task(Of Integer)
        Me.FolderActionState = ActionState.Analysing

        CancellationTokenSource = New CancellationTokenSource()
        Dim token = CancellationTokenSource.Token

        Dim Analyser As New Core.Analyser(Me.FolderName)

        If Not Analyser.HasDirectoryWritePermission Then
            Me.FolderActionState = ActionState.Idle
            Return -1
        End If

        Dim containsCompressedFiles = Await Analyser.AnalyseFolder(token)
        If CancellationTokenSource.IsCancellationRequested Then
            Me.FolderActionState = ActionState.Idle
            Return 1
        End If

        Me.AnalysisResults = New ObservableCollection(Of Core.AnalysedFileDetails)(Analyser.FileCompressionDetailsList)
        Me.UncompressedBytes = Analyser.UncompressedBytes
        Me.CompressedBytes = Analyser.CompressedBytes

        If containsCompressedFiles OrElse Me.IsFreshlyCompressed Then : Me.FolderActionState = ActionState.Results
        Else : Me.FolderActionState = ActionState.Idle
        End If
        PoorlyCompressedFiles = Await Analyser.GetPoorlyCompressedExtensions()

        Return 0

    End Function




















End Class




Public Class CompressableFolderFactory
    Public Shared Function CreateCompressableFolder(path As String) As CompressableFolder
        Dim folderInfo = New IO.DirectoryInfo(path)

        If IsSteamFolder(folderInfo) Then
            Return If(CreateSteamFolder(folderInfo), New StandardFolder(path))
        Else
            Return New StandardFolder(path)
        End If

    End Function


    Private Shared Function IsSteamFolder(folderPath As IO.DirectoryInfo) As Boolean
        Return folderPath.Parent?.Parent?.Name = "steamapps"
    End Function


    Private Shared Function CreateSteamFolder(folderInfo As IO.DirectoryInfo) As CompressableFolder

        Dim SteamFolderData? = SteamACFParser.GetSteamNameAndIDFromFolder(folderInfo)

        If SteamFolderData Is Nothing Then Return Nothing

        Dim steamFolder As New SteamFolder()
        steamFolder.FolderName = folderInfo.FullName
        steamFolder.DisplayName = If(SteamFolderData?.GameName, folderInfo.FullName)
        steamFolder.SteamAppID = SteamFolderData?.AppID
        Return steamFolder
    End Function




End Class



Public Enum ActionState
    Idle
    Analysing
    Working
    Results
    Paused
    Waiting
End Enum