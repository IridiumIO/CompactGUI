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

    <ObservableProperty> Private _UsesDirectStorage As Boolean

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