Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Threading

Imports CommunityToolkit.Mvvm.ComponentModel

Imports CompactGUI.Core
Imports CompactGUI.Core.Settings
Imports CompactGUI.Core.WOFHelper

Imports Microsoft.Extensions.Logging

Imports PropertyChanged


'Need this abstract class so we can use it in XAML
Public MustInherit Class CompressableFolder : Inherits ObservableObject : Implements IDisposable


    Private Shared ReadOnly CompactorLogger As ILogger = Application.GetService(Of ILogger(Of Compactor))()
    Private Shared ReadOnly UncompactorLogger As ILogger = Application.GetService(Of ILogger(Of Uncompactor))()
    Private Shared ReadOnly AnalyserLogger As ILogger = Application.GetService(Of ILogger(Of Analyser))()


    <ObservableProperty> Private _FolderName As String
    <ObservableProperty> Private _DisplayName As String
    <ObservableProperty> Private _CurrentCompression As CompressionMode

    <NotifyPropertyChangedFor(NameOf(BytesSaved), NameOf(CompressionRatio))>
    <ObservableProperty> Private _FolderActionState As ActionState

    <NotifyPropertyChangedFor(NameOf(BytesSaved), NameOf(CompressionRatio))>
    <ObservableProperty> Private _UncompressedBytes As Long = 0

    <NotifyPropertyChangedFor(NameOf(BytesSaved), NameOf(CompressionRatio))>
    <ObservableProperty> Private _CompressedBytes As Long = 0

    <ObservableProperty> Private _AnalysisResults As New ObservableCollection(Of AnalysedFileDetails)
    <ObservableProperty> Private _PoorlyCompressedFiles As List(Of ExtensionResult)
    <ObservableProperty> Private _CompressionOptions As New CompressionOptions
    <ObservableProperty> Private _IsFreshlyCompressed As Boolean

    <ObservableProperty> Private _FolderBGImage As BitmapImage = Nothing


    <ObservableProperty> Private _IsGettingEstimate As Boolean = False

    <ObservableProperty> Private _WikiCompressionResults As WikiCompressionResults
    <ObservableProperty> Private _WikiPoorlyCompressedFiles As New List(Of String)

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
            If AnalysisResults Is Nothing OrElse Application.GetService(Of ISettingsService).AppSettings.NonCompressableList.Count = 0 Then Return 0
            Return AnalysisResults.Where(Function(fl) Application.GetService(Of ISettingsService).AppSettings.NonCompressableList.Contains(New IO.FileInfo(fl.FileName).Extension)).Count
        End Get
    End Property

    Public ReadOnly Property WikiPoorlyCompressedFilesCount As Integer
        Get
            If AnalysisResults Is Nothing OrElse WikiPoorlyCompressedFiles Is Nothing Then Return 0
            Return WikiPoorlyCompressedFiles.Count
        End Get
    End Property


    Public ReadOnly Property CompressionProgress As IProgress(Of CompressionProgress) = New Progress(Of CompressionProgress)(Sub(x) RaiseEvent CompressionProgressChanged(Me, x))
    Public Event CompressionProgressChanged As EventHandler(Of CompressionProgress)




    Public Compressor As ICompressor
    Public Analyser As Analyser
    Private CancellationTokenSource As CancellationTokenSource


    Public Async Function CompressFolder() As Task(Of Boolean)

        Compressor = New Compactor(FolderName, WOFConvertCompressionLevel(CompressionOptions.SelectedCompressionMode), GetSkipList, Analyser, CompactorLogger)
        Return Await RunCompressionAsync(Compressor, Nothing, True)

    End Function


    Public Async Function UncompressFolder() As Task(Of Boolean)

        Compressor = New Uncompactor(UncompactorLogger)
        Dim compressedFilesList = AnalysisResults.Where(Function(rs) rs.CompressedSize < rs.UncompressedSize).Select(Of String)(Function(f) f.FileName).ToList
        Return Await RunCompressionAsync(Compressor, compressedFilesList, isCompressing:=False)

    End Function

    Private Async Function RunCompressionAsync(compressor As ICompressor, filesList As List(Of String), isCompressing As Boolean) As Task(Of Boolean)
        FolderActionState = ActionState.Working

        If CancellationTokenSource IsNot Nothing AndAlso Not CancellationTokenSource.IsCancellationRequested Then
            CancellationTokenSource.Cancel()
        End If

        CompressionProgress.Report(New CompressionProgress(0, ""))

        Dim res = Await compressor.RunAsync(filesList, CompressionProgress, GetThreadCount)

        If isCompressing Then
            FolderActionState = ActionState.Results
            IsFreshlyCompressed = res
        Else
            FolderActionState = ActionState.Idle
            IsFreshlyCompressed = False
            Await AnalyseFolderAsync()
        End If
        compressor.Dispose()
        Return res
    End Function



    Public Async Function AnalyseFolderAsync() As Task(Of Integer)

        FolderActionState = ActionState.Analysing
        If CancellationTokenSource IsNot Nothing AndAlso Not CancellationTokenSource.IsCancellationRequested Then
            CancellationTokenSource.Cancel()
        End If
        CancellationTokenSource = New CancellationTokenSource()
        Dim token = CancellationTokenSource.Token

        Analyser?.Dispose()
        Analyser = New Analyser(FolderName, AnalyserLogger)

        If Not Core.SharedMethods.HasDirectoryWritePermission(FolderName) Then
            FolderActionState = ActionState.Idle
            Return -1
        End If

        Dim retAnalysisResults = Await Analyser.GetAnalysedFilesAsync(token)
        If CancellationTokenSource.IsCancellationRequested Then
            FolderActionState = ActionState.Idle
            Return 1
        End If

        AnalysisResults = New ObservableCollection(Of AnalysedFileDetails)(retAnalysisResults)
        UncompressedBytes = Analyser.UncompressedBytes
        CompressedBytes = Analyser.CompressedBytes

        If Analyser.ContainsCompressedFiles OrElse IsFreshlyCompressed Then : FolderActionState = ActionState.Results
        Else : FolderActionState = ActionState.Idle
        End If
        PoorlyCompressedFiles = Analyser.GetPoorlyCompressedExtensions()

        Return 0

    End Function




    Public Overridable Async Function GetEstimatedCompression() As Task
        IsGettingEstimate = True

        CancellationTokenSource = New CancellationTokenSource()

        Dim estimator As New Estimator


        Dim estimatedData As List(Of (AnalysedFile As AnalysedFileDetails, CompressionRatio As Single)) = Nothing

        Try
            Dim sw As New Stopwatch
            sw.Start()
            estimatedData = Await Task.Run(Function() estimator.EstimateCompression(AnalysisResults.ToList, IsHDD, GetThreadCount, Core.SharedMethods.GetClusterSize(FolderName), CancellationTokenSource.Token))
            sw.Stop()
            'Debug.WriteLine($"Estimated compression took {sw.ElapsedMilliseconds}ms")
        Catch ex As AggregateException
            IsGettingEstimate = False
            Return
        End Try

        For Each item In estimatedData
            If item.CompressionRatio >= 0.98 AndAlso item.AnalysedFile.FileName <> "" Then
                WikiPoorlyCompressedFiles.Add(item.AnalysedFile.FileName)
            End If
        Next

        Dim estimatedAfterBytes = estimatedData.Sum(Function(x) x.AnalysedFile.UncompressedSize * x.CompressionRatio)

        'This is absolutely stupid

        Dim X4KResult As New CompressionResult
        X4KResult.CompType = WOFCompressionAlgorithm.XPRESS4K
        X4KResult.BeforeBytes = UncompressedBytes
        X4KResult.AfterBytes = Math.Min(estimatedAfterBytes * 1.01, UncompressedBytes)
        X4KResult.TotalResults = 1

        Dim X8KResult As New CompressionResult
        X8KResult.CompType = WOFCompressionAlgorithm.XPRESS8K
        X8KResult.BeforeBytes = UncompressedBytes
        X8KResult.AfterBytes = Math.Min(estimatedAfterBytes * 1.0, UncompressedBytes)
        X8KResult.TotalResults = 1

        Dim X16KResult As New CompressionResult
        X16KResult.CompType = WOFCompressionAlgorithm.XPRESS16K
        X16KResult.BeforeBytes = UncompressedBytes
        X16KResult.AfterBytes = Math.Min(estimatedAfterBytes * 0.98, UncompressedBytes)
        X16KResult.TotalResults = 1

        Dim LZXResult As New CompressionResult
        LZXResult.CompType = WOFCompressionAlgorithm.LZX
        LZXResult.BeforeBytes = UncompressedBytes
        LZXResult.AfterBytes = Math.Min(estimatedAfterBytes * 0.95, UncompressedBytes)
        LZXResult.TotalResults = 1

        WikiCompressionResults = New WikiCompressionResults(New List(Of CompressionResult) From {X4KResult, X8KResult, X16KResult, LZXResult})

        IsGettingEstimate = False

        OnPropertyChanged(NameOf(WikiCompressionResults))
        OnPropertyChanged(NameOf(WikiPoorlyCompressedFiles))
        OnPropertyChanged(NameOf(WikiPoorlyCompressedFilesCount))
        OnPropertyChanged(NameOf(IsGettingEstimate))

    End Function

    Public Sub CancelEstimation()
        If CancellationTokenSource IsNot Nothing AndAlso Not CancellationTokenSource.IsCancellationRequested Then
            CancellationTokenSource.Cancel()
        End If
    End Sub

    Protected Function GetThreadCount() As Integer
        Dim threadCount As Integer = Application.GetService(Of ISettingsService).AppSettings.MaxCompressionThreads
        If Application.GetService(Of ISettingsService).AppSettings.LockHDDsToOneThread Then
            Dim HDDType As DiskDetector.Models.HardwareType = GetDiskType()
            If HDDType = DiskDetector.Models.HardwareType.Hdd Then
                threadCount = 1
            End If
        End If
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

    Protected Function IsHDD() As Boolean
        Dim HDDType As DiskDetector.Models.HardwareType = GetDiskType()
        Return HDDType = DiskDetector.Models.HardwareType.Hdd
    End Function

    Protected Overridable Function GetSkipList() As String()
        Dim exclist As String() = Array.Empty(Of String)()
        If CompressionOptions.SkipPoorlyCompressedFileTypes AndAlso Application.GetService(Of ISettingsService).AppSettings.NonCompressableList.Count <> 0 Then
            'Debug.WriteLine("Adding non-compressable list to exclusion list")
            exclist = exclist.Union(Application.GetService(Of ISettingsService).AppSettings.NonCompressableList).ToArray
        End If
        If CompressionOptions.SkipUserSubmittedFiletypes AndAlso WikiPoorlyCompressedFiles?.Count <> 0 Then
            'Debug.WriteLine("Adding estimator poorly compressed list to exclusion list")
            exclist = exclist.Union(WikiPoorlyCompressedFiles).ToArray
        End If


        Return exclist
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        Compressor?.Dispose()
        Analyser?.Dispose()
        CancellationTokenSource?.Dispose()

        AnalysisResults?.Clear()
        PoorlyCompressedFiles?.Clear()
        WikiPoorlyCompressedFiles?.Clear()
        GC.SuppressFinalize(Me)
    End Sub
End Class



Public Enum ActionState
    Idle
    Analysing
    Working
    Results
    Paused
    Waiting
End Enum