Imports System.Collections.ObjectModel
Imports System.Threading

Imports CommunityToolkit.Mvvm.ComponentModel

Imports CompactGUI.Core

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

    Public Property FolderBGImage As BitmapImage = Nothing

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


    Public ReadOnly Property GlobalPoorlyCompressedFileCount
        Get
            If AnalysisResults Is Nothing OrElse SettingsHandler.AppSettings.NonCompressableList.Count = 0 Then Return 0
            Return AnalysisResults.Where(Function(fl) SettingsHandler.AppSettings.NonCompressableList.Contains(New IO.FileInfo(fl.FileName).Extension)).Count
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



    Public Compactor As Compactor

    Public Async Function CompressFolder() As Task(Of Boolean)

        FolderActionState = ActionState.Working

        CompressionProgress.Report(New CompressionProgress(0, ""))

        Compactor = New Compactor(FolderName, WOFConvertCompressionLevel(CompressionOptions.SelectedCompressionMode), GetSkipList)
        Dim res = Await Compactor.RunAsync(Nothing, CompressionProgress, GetThreadCount)

        If res Then
            FolderActionState = ActionState.Results
            IsFreshlyCompressed = True
        Else
            FolderActionState = ActionState.Results
            IsFreshlyCompressed = False
        End If

        Return res

    End Function


    Public Uncompactor As Uncompactor

    Public Async Function UncompressFolder() As Task(Of Boolean)
        FolderActionState = ActionState.Working
        CompressionProgress.Report(New CompressionProgress(0, ""))

        Uncompactor = New Uncompactor

        Dim compressedFilesList = AnalysisResults.Where(Function(rs) rs.CompressedSize < rs.UncompressedSize).Select(Of String)(Function(f) f.FileName).ToList

        Dim res = Await Uncompactor.RunAsync(compressedFilesList, CompressionProgress, GetThreadCount)

        FolderActionState = ActionState.Idle
        IsFreshlyCompressed = False

        Await AnalyseFolderAsync()
        Return res
    End Function



    Private CancellationTokenSource As CancellationTokenSource

    Public Analyser As Analyser

    Public Async Function AnalyseFolderAsync() As Task(Of Integer)

        FolderActionState = ActionState.Analysing

        CancellationTokenSource = New CancellationTokenSource()
        Dim token = CancellationTokenSource.Token

        Analyser = New Analyser(FolderName)

        If Not Analyser.HasDirectoryWritePermission(FolderName) Then
            FolderActionState = ActionState.Idle
            Return -1
        End If

        Dim containsCompressedFiles = Await Analyser.AnalyseFolder(token)
        If CancellationTokenSource.IsCancellationRequested Then
            FolderActionState = ActionState.Idle
            Return 1
        End If

        AnalysisResults = New ObservableCollection(Of AnalysedFileDetails)(Analyser.FileCompressionDetailsList)
        UncompressedBytes = Analyser.UncompressedBytes
        CompressedBytes = Analyser.CompressedBytes

        If containsCompressedFiles OrElse IsFreshlyCompressed Then : FolderActionState = ActionState.Results
        Else : FolderActionState = ActionState.Idle
        End If
        PoorlyCompressedFiles = Await Analyser.GetPoorlyCompressedExtensions()

        Return 0

    End Function



    Protected Function GetThreadCount() As Integer
        Dim threadCount As Integer = SettingsHandler.AppSettings.MaxCompressionThreads
        If SettingsHandler.AppSettings.LockHDDsToOneThread Then
            Dim HDDType As DiskDetector.Models.HardwareType = GetDiskType()
            If HDDType = DiskDetector.Models.HardwareType.Hdd Then
                threadCount = 1
            End If
        End If
        Debug.WriteLine($"Thread count: {threadCount}")
        Return threadCount
    End Function

    Protected Function GetDiskType() As DiskDetector.Models.HardwareType
        If FolderName Is Nothing Then Return DiskDetector.Models.HardwareType.Unknown
        Try
            Return DiskDetector.Detector.DetectDrive(FolderName.First, DiskDetector.Models.QueryType.RotationRate).HardwareType
        Catch ex As Exception
            Return DiskDetector.Models.HardwareType.Unknown
        End Try
    End Function


    Protected Overridable Function GetSkipList() As String()
        Dim exclist As String() = Array.Empty(Of String)()
        If CompressionOptions.SkipPoorlyCompressedFileTypes AndAlso SettingsHandler.AppSettings.NonCompressableList.Count <> 0 Then
            exclist = exclist.Union(SettingsHandler.AppSettings.NonCompressableList).ToArray
        End If


        Return exclist
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

        Dim steamFolder As New SteamFolder(folderInfo.FullName, If(SteamFolderData?.GameName, folderInfo.FullName), SteamFolderData?.AppID)

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