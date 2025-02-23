
Imports System.Collections.ObjectModel

Imports Microsoft.Toolkit.Mvvm.ComponentModel
Imports DiskDetector

Public Class ActiveFolder : Inherits ObservableObject

    Public Property FolderName As String
    Public Property DisplayName As String
    Public Property AnalysisResults As List(Of Core.AnalysedFileDetails)
    Public Property PoorlyCompressedFiles As List(Of Core.ExtensionResult)
    Public Property SteamAppID As Integer
    Public Property WikiPoorlyCompressedFiles As New List(Of String)
    Public Property UncompressedBytes As Long
    Public Property CompressedBytes As Long
    Public Property SelectedCompressionMode = 0
    Public Property CompressionConfidence = -1
    Public Property WikiCompressionResults As New ObservableCollection(Of CompressionResult)

    Public Property IsFreshlyCompressed As Boolean = False

    Public ReadOnly Property DiskType As DiskDetector.Models.HardwareType
        Get
            If FolderName Is Nothing Then Return Models.HardwareType.Unknown

            Try
                Return DiskDetector.Detector.DetectDrive(FolderName.First, DiskDetector.Models.QueryType.RotationRate).HardwareType
            Catch ex As Exception
                Return Models.HardwareType.Unknown
            End Try

        End Get
    End Property

    Public ReadOnly Property CompressionRatio As Decimal
        Get
            If UncompressedBytes = 0 OrElse CompressedBytes = 0 OrElse CompressedBytes = 1010101010101010 Then Return 0
            Return CompressedBytes / UncompressedBytes
        End Get
    End Property
    Public ReadOnly Property GlobalPoorlyCompressedFilesCount As Integer
        Get
            If AnalysisResults Is Nothing OrElse SettingsHandler.AppSettings.NonCompressableList.Count = 0 Then Return 0
            Return AnalysisResults.Where(Function(fl) SettingsHandler.AppSettings.NonCompressableList.Contains(New IO.FileInfo(fl.FileName).Extension)).Count
        End Get
    End Property
    Public ReadOnly Property WikiPoorlyCompressedFilesCount As Integer
        Get
            If AnalysisResults Is Nothing OrElse WikiPoorlyCompressedFiles Is Nothing Then Return 0
            Return AnalysisResults.Where(Function(fl) WikiPoorlyCompressedFiles.Contains(New IO.FileInfo(fl.FileName).Extension)).Count
        End Get
    End Property

End Class

