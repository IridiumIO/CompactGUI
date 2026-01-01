Imports System.Collections.ObjectModel
Imports System.Threading

Imports CompactGUI.Core
Imports CompactGUI.Core.Settings

'Imports Microsoft.CodeAnalysis.Diagnostics

Imports Microsoft.Extensions.Logging

Public Class CompressableFolderService


    Private Shared ReadOnly CompactorLogger As ILogger = Application.GetService(Of ILogger(Of Compactor))()
    Private Shared ReadOnly UncompactorLogger As ILogger = Application.GetService(Of ILogger(Of Uncompactor))()
    Private Shared ReadOnly AnalyserLogger As ILogger = Application.GetService(Of ILogger(Of Analyser))()

    Private folderTokens As New Dictionary(Of CompressableFolder, CancellationTokenSource)


    Public Async Function CompressFolder(folder As CompressableFolder) As Task(Of Boolean)
        folder.Compressor = New Compactor(folder.FolderName, WOFHelper.WOFConvertCompressionLevel(folder.CompressionOptions.SelectedCompressionMode), GetSkipList(folder), folder.Analyser, CompactorLogger)
        Return Await RunCompressionAsync(folder, folder.Compressor, Nothing, True)


    End Function


    Public Async Function UncompressFolder(folder As CompressableFolder) As Task(Of Boolean)

        folder.Compressor = New Uncompactor(UncompactorLogger)
        Dim compressedFilesList = folder.AnalysisResults.Where(Function(rs) rs.CompressedSize < rs.UncompressedSize).Select(Of String)(Function(f) f.FileName).ToList
        Return Await RunCompressionAsync(folder, folder.Compressor, compressedFilesList, isCompressing:=False)


    End Function

    Private Async Function RunCompressionAsync(folder As CompressableFolder, compressor As ICompressor, filesList As List(Of String), isCompressing As Boolean) As Task(Of Boolean)
        folder.FolderActionState = ActionState.Working

        CancelEstimation(folder)
        Dim cts = New CancellationTokenSource()
        folderTokens(folder) = cts
        Dim progress As IProgress(Of CompressionProgress) = New Progress(Of CompressionProgress)(Sub(x) folder.CompressionProgress = x)

        progress.Report(New CompressionProgress(0, ""))

        Dim res = Await compressor.RunAsync(filesList, progress, GetThreadCount(folder))

        If isCompressing Then
            folder.FolderActionState = ActionState.Results
            folder.IsFreshlyCompressed = res
        Else
            folder.FolderActionState = ActionState.Idle
            folder.IsFreshlyCompressed = False
            Await AnalyseFolderAsync(folder)
        End If
        compressor.Dispose()


        folderTokens(folder).Dispose()
        folderTokens.Remove(folder)


        Return res
    End Function


    Public Async Function AnalyseFolderAsync(folder As CompressableFolder) As Task(Of Integer)

        folder.FolderActionState = ActionState.Analysing
        CancelEstimation(folder)

        Dim cts = New CancellationTokenSource()
        folderTokens(folder) = cts
        Dim token = cts.Token


        folder.Analyser?.Dispose()
        folder.Analyser = New Analyser(folder.FolderName, AnalyserLogger)

        If Not Core.SharedMethods.HasDirectoryWritePermission(folder.FolderName) Then
            folder.FolderActionState = ActionState.Idle
            Return -1
        End If

        Dim retAnalysisResults = Await folder.Analyser.GetAnalysedFilesAsync(token)
        If cts.IsCancellationRequested Then
            folder.FolderActionState = ActionState.Idle
            Return 1
        End If

        folder.AnalysisResults = New ObservableCollection(Of AnalysedFileDetails)(retAnalysisResults)
        folder.UncompressedBytes = folder.Analyser.UncompressedBytes
        folder.CompressedBytes = folder.Analyser.CompressedBytes

        If folder.Analyser.ContainsCompressedFiles OrElse folder.IsFreshlyCompressed Then
            folder.FolderActionState = ActionState.Results
        Else
            folder.FolderActionState = ActionState.Idle
        End If
        folder.PoorlyCompressedFiles = folder.Analyser.GetPoorlyCompressedExtensions()

        Return 0

    End Function

    Public Overridable Async Function GetEstimatedCompression(folder As CompressableFolder) As Task
        folder.IsGettingEstimate = True

        CancelEstimation(folder)
        Dim cts = New CancellationTokenSource()
        folderTokens(folder) = cts

        Dim estimator As New Estimator
        Dim estimatedData As List(Of (AnalysedFile As AnalysedFileDetails, CompressionRatio As Single)) = Nothing

        Try
            estimatedData = Await Task.Run(Function() estimator.EstimateCompression(folder.AnalysisResults.ToList, IsHDD(folder), GetThreadCount(folder), Core.SharedMethods.GetClusterSize(folder.FolderName), cts.Token))

        Catch ex As AggregateException
            folder.IsGettingEstimate = False
            Return
        End Try

        For Each item In estimatedData
            If item.CompressionRatio >= 0.98 AndAlso item.AnalysedFile.FileName <> "" Then
                folder.WikiPoorlyCompressedFiles.Add(item.AnalysedFile.FileName)
            End If
        Next

        Dim estimatedAfterBytes = estimatedData.Sum(Function(x) x.AnalysedFile.UncompressedSize * x.CompressionRatio)

        'This is absolutely stupid

        Dim X4KResult As New CompressionResult
        X4KResult.CompType = CompressionMode.XPRESS4K
        X4KResult.BeforeBytes = folder.UncompressedBytes
        X4KResult.AfterBytes = Math.Min(estimatedAfterBytes * 1.01, folder.UncompressedBytes)
        X4KResult.TotalResults = 1

        Dim X8KResult As New CompressionResult
        X8KResult.CompType = CompressionMode.XPRESS8K
        X8KResult.BeforeBytes = folder.UncompressedBytes
        X8KResult.AfterBytes = Math.Min(estimatedAfterBytes * 1.0, folder.UncompressedBytes)
        X8KResult.TotalResults = 1

        Dim X16KResult As New CompressionResult
        X16KResult.CompType = CompressionMode.XPRESS16K
        X16KResult.BeforeBytes = folder.UncompressedBytes
        X16KResult.AfterBytes = Math.Min(estimatedAfterBytes * 0.98, folder.UncompressedBytes)
        X16KResult.TotalResults = 1

        Dim LZXResult As New CompressionResult
        LZXResult.CompType = CompressionMode.LZX
        LZXResult.BeforeBytes = folder.UncompressedBytes
        LZXResult.AfterBytes = Math.Min(estimatedAfterBytes * 0.95, folder.UncompressedBytes)
        LZXResult.TotalResults = 1

        folder.WikiCompressionResults = New WikiCompressionResults(New List(Of CompressionResult) From {X4KResult, X8KResult, X16KResult, LZXResult})

        folder.IsGettingEstimate = False


        folder.NotifyPropertyChanged(NameOf(folder.WikiCompressionResults))
        folder.NotifyPropertyChanged(NameOf(folder.WikiPoorlyCompressedFiles))
        folder.NotifyPropertyChanged(NameOf(folder.WikiPoorlyCompressedFilesCount))
        folder.NotifyPropertyChanged(NameOf(folder.IsGettingEstimate))

    End Function
    Public Sub CancelEstimation(folder As CompressableFolder)
        If folderTokens.ContainsKey(folder) AndAlso Not folderTokens(folder).IsCancellationRequested Then
            folderTokens(folder).Cancel()
        End If
    End Sub


    Public Shared Function GetThreadCount(folder As CompressableFolder) As Integer
        Dim threadCount As Integer = Application.GetService(Of ISettingsService).AppSettings.MaxCompressionThreads
        If Application.GetService(Of ISettingsService).AppSettings.LockHDDsToOneThread Then
            Dim HDDType As DiskDetector.Models.HardwareType = GetDiskType(folder)
            If HDDType = DiskDetector.Models.HardwareType.Hdd Then
                threadCount = 1
            End If
        End If
        Return threadCount
    End Function

    Public Shared Function GetDiskType(folder As CompressableFolder) As DiskDetector.Models.HardwareType
        If folder.FolderName Is Nothing Then Return DiskDetector.Models.HardwareType.Unknown
        Try
            Return DiskDetector.Detector.DetectDrive(folder.FolderName.First, DiskDetector.Models.QueryType.RotationRate).HardwareType
        Catch ex As Exception
            Return DiskDetector.Models.HardwareType.Unknown
        End Try
    End Function

    Public Shared Function IsHDD(folder As CompressableFolder) As Boolean
        Dim HDDType As DiskDetector.Models.HardwareType = GetDiskType(folder)
        Return HDDType = DiskDetector.Models.HardwareType.Hdd
    End Function

    Private Function GetSkipList(folder As CompressableFolder) As String()
        Dim exclist As String() = Array.Empty(Of String)()

        If folder.CompressionOptions.SkipPoorlyCompressedFileTypes AndAlso Application.GetService(Of ISettingsService).AppSettings.NonCompressableList.Count <> 0 Then
            'Debug.WriteLine("Adding non-compressable list to exclusion list")
            exclist = exclist.Union(Application.GetService(Of ISettingsService).AppSettings.NonCompressableList).ToArray
        End If
        If folder.CompressionOptions.SkipUserSubmittedFiletypes AndAlso folder.WikiPoorlyCompressedFiles?.Count <> 0 Then
            'Debug.WriteLine("Adding estimator poorly compressed list to exclusion list")
            exclist = exclist.Union(folder.WikiPoorlyCompressedFiles).ToArray
        End If

        Return exclist
    End Function


End Class
