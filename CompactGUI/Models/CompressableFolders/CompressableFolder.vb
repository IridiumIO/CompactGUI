Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Threading

Imports CommunityToolkit.Mvvm.ComponentModel

Imports CompactGUI.Core
Imports CompactGUI.Core.Settings
Imports CompactGUI.Core.WOFHelper

Imports Microsoft.Extensions.Logging

'Imports PropertyChanged


'Need this abstract class so we can use it in XAML
Public MustInherit Class CompressableFolder : Inherits ObservableObject : Implements IDisposable

    <ObservableProperty> Private _FolderName As String
    <ObservableProperty> Private _DisplayName As String
    <ObservableProperty> Private _CurrentCompression As CompressionMode

    <NotifyPropertyChangedFor(NameOf(BytesSaved), NameOf(CompressionRatio))>
    <ObservableProperty> Private _FolderActionState As ActionState

    <NotifyPropertyChangedFor(NameOf(BytesSaved), NameOf(CompressionRatio))>
    <ObservableProperty> Private _UncompressedBytes As Long = 0

    <NotifyPropertyChangedFor(NameOf(BytesSaved), NameOf(CompressionRatio))>
    <ObservableProperty> Private _CompressedBytes As Long = 0

    <NotifyPropertyChangedFor(NameOf(GlobalPoorlyCompressedFileCount), NameOf(WikiPoorlyCompressedFilesCount), NameOf(CustomPoorlyCompressedFileCount), NameOf(SkippedFileCount))>
    <ObservableProperty> Private _AnalysisResults As New ObservableCollection(Of AnalysedFileDetails)
    <ObservableProperty> Private _PoorlyCompressedFiles As List(Of ExtensionResult)
    <ObservableProperty> Private _CompressionOptions As New CompressionOptions
    <ObservableProperty> Private _IsFreshlyCompressed As Boolean

    <ObservableProperty> Private _FolderBGImage As BitmapImage = Nothing


    <ObservableProperty> Private _IsGettingEstimate As Boolean = False

    <ObservableProperty> Private _WikiCompressionResults As WikiCompressionResults
    <NotifyPropertyChangedFor(NameOf(WikiPoorlyCompressedFilesCount), NameOf(SkippedFileCount))>
    <ObservableProperty> Private _WikiPoorlyCompressedFiles As New List(Of String)

    <ObservableProperty> Private _IsDirectStorage As Boolean = False


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
            Dim skipList = Application.GetService(Of ISettingsService).AppSettings.NonCompressableList
            If AnalysisResults Is Nothing OrElse skipList.Count = 0 Then Return 0
            Dim excluded = SkipListMatcher.GetExcludedFiles(FolderName, AnalysisResults.Select(Function(fl) fl.FileName), skipList)
            Return AnalysisResults.Where(Function(fl) excluded.Contains(fl.FileName)).Count
        End Get
    End Property

    Public ReadOnly Property CustomPoorlyCompressedFileCount
        Get
            If AnalysisResults Is Nothing OrElse CompressionOptions.SkipList Is Nothing OrElse CompressionOptions.SkipList.Count = 0 Then Return 0
            Dim excluded = SkipListMatcher.GetExcludedFiles(FolderName, AnalysisResults.Select(Function(fl) fl.FileName), CompressionOptions.SkipList)
            Return AnalysisResults.Where(Function(fl) excluded.Contains(fl.FileName)).Count
        End Get
    End Property

    Public ReadOnly Property SkippedFileCount As Integer
        Get
            If Not CompressionOptions.SkipListEnabled Then Return 0

            Dim effectiveList As New List(Of String)
            If CompressionOptions.SkipList IsNot Nothing Then
                effectiveList.AddRange(CompressionOptions.SkipList)
            Else
                If CompressionOptions.SkipPoorlyCompressedFileTypes Then
                    effectiveList.AddRange(Application.GetService(Of ISettingsService).AppSettings.NonCompressableList)
                End If
                If CompressionOptions.SkipUserSubmittedFiletypes AndAlso WikiPoorlyCompressedFiles IsNot Nothing Then
                    effectiveList.AddRange(WikiPoorlyCompressedFiles)
                End If
            End If

            If AnalysisResults Is Nothing OrElse effectiveList.Count = 0 Then Return 0
            Dim excluded = SkipListMatcher.GetExcludedFiles(FolderName, AnalysisResults.Select(Function(fl) fl.FileName), effectiveList)
            Return AnalysisResults.Where(Function(fl) excluded.Contains(fl.FileName)).Count
        End Get
    End Property

    Public ReadOnly Property WikiPoorlyCompressedFilesCount As Integer
        Get
            If AnalysisResults Is Nothing OrElse WikiPoorlyCompressedFiles Is Nothing Then Return 0
            Return WikiPoorlyCompressedFiles.Count
        End Get
    End Property


    <ObservableProperty> Private _CompressionProgress As CompressionProgress


    Public Compressor As ICompressor
    Public Analyser As Analyser


    Public Sub NotifyPropertyChanged(name As String)
        OnPropertyChanged(name)
    End Sub


    Public Sub Dispose() Implements IDisposable.Dispose
        Compressor?.Dispose()
        Analyser?.Dispose()

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