Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Public Class StatisticsViewModel : Inherits ObservableObject
    Private ReadOnly _statisticsService As StatisticsService
    Private ReadOnly _fileTypeAnalysisService As FileTypeAnalysisService
    
    Public Property Statistics As New ObservableCollection(Of CompressionStatistics)
    Public Property FileTypeStats As New ObservableCollection(Of FileTypeStatistic)
    Public Property MonthlyStats As New ObservableCollection(Of CompressionStatistics)
    
    Public Property TotalSpaceSaved As Long
    Public Property AverageCompressionRatio As Double
    Public Property TotalFoldersCompressed As Integer
    
    Public ReadOnly Property TotalSpaceSavedFormatted As String
        Get
            Return FormatBytes(TotalSpaceSaved)
        End Get
    End Property
    
    Public ReadOnly Property AverageCompressionRatioFormatted As String
        Get
            Return $"{AverageCompressionRatio:P2}"
        End Get
    End Property
    
    Public Sub New(statisticsService As StatisticsService, fileTypeAnalysisService As FileTypeAnalysisService)
        _statisticsService = statisticsService
        _fileTypeAnalysisService = fileTypeAnalysisService
        
        LoadStatistics()
    End Sub
    
    Private Sub LoadStatistics()
        ' Load all statistics
        Statistics.Clear()
        For Each stat In _statisticsService.Statistics.OrderByDescending(Function(s) s.CompressionDate)
            Statistics.Add(stat)
        Next
        
        ' Calculate summary statistics
        TotalSpaceSaved = _statisticsService.GetTotalSpaceSaved()
        AverageCompressionRatio = _statisticsService.GetAverageCompressionRatio()
        TotalFoldersCompressed = Statistics.Select(Function(s) s.FolderPath).Distinct().Count()
        
        ' Load file type statistics
        FileTypeStats.Clear()
        For Each fileType In _statisticsService.GetMostCompressedFileTypes().OrderByDescending(Function(ft) ft.CompressionRatio)
            FileTypeStats.Add(fileType)
        Next
        
        ' Load monthly statistics
        MonthlyStats.Clear()
        For Each monthlyStat In _statisticsService.GetStatisticsByMonth().Values.OrderByDescending(Function(s) s.CompressionDate)
            MonthlyStats.Add(monthlyStat)
        Next
        
        OnPropertyChanged(NameOf(TotalSpaceSavedFormatted))
        OnPropertyChanged(NameOf(AverageCompressionRatioFormatted))
    End Sub
    
    Public ReadOnly Property ExportStatisticsCommand As IRelayCommand = New RelayCommand(AddressOf ExportStatistics)
    
    Private Sub ExportStatistics()
        Try
            Dim saveFileDialog = New Microsoft.Win32.SaveFileDialog With {
                .Filter = "CSV Files (*.csv)|*.csv",
                .DefaultExt = "csv",
                .Title = "Export Compression Statistics"
            }
            
            If saveFileDialog.ShowDialog() = True Then
                Dim filePath = saveFileDialog.FileName
                
                Using writer As New StreamWriter(filePath, False, Encoding.UTF8)
                    ' Write header
                    writer.WriteLine("Folder Name,Folder Path,Compression Date,Original Size,Compressed Size,Space Saved,Compression Ratio,File Count")
                    
                    ' Write data
                    For Each stat In Statistics
                        writer.WriteLine($"{stat.FolderName},{stat.FolderPath},{stat.CompressionDate},{stat.OriginalSize},{stat.CompressedSize},{stat.SpaceSaved},{stat.CompressionRatio},{stat.FileCount}")
                    Next
                End Using
                
                Application.GetService(Of CustomSnackBarService)().ShowMessage("Statistics exported successfully", "Statistics have been exported to CSV file.")
            End If
        Catch ex As Exception
            Application.GetService(Of CustomSnackBarService)().ShowError("Error exporting statistics", ex.Message)
        End Try
    End Sub
    
    Public ReadOnly Property ClearStatisticsCommand As IRelayCommand = New RelayCommand(AddressOf ClearStatistics)
    
    Private Async Sub ClearStatistics()
        Try
            Dim confirmed = Await Application.GetService(Of IWindowService)().ShowMessageBox("Clear Statistics", "Are you sure you want to clear all compression statistics? This action cannot be undone.")
            
            If confirmed Then
                _statisticsService.ClearStatistics()
                LoadStatistics()
                Application.GetService(Of CustomSnackBarService)().ShowMessage("Statistics cleared", "All compression statistics have been cleared.")
            End If
        Catch ex As Exception
            Application.GetService(Of CustomSnackBarService)().ShowError("Error clearing statistics", ex.Message)
        End Try
    End Sub
    
    Private Function FormatBytes(bytes As Long) As String
        Dim sizes() As String = {"B", "KB", "MB", "GB", "TB"}
        Dim order As Integer = 0
        Dim dblBytes As Double = bytes
        
        While dblBytes >= 1024 AndAlso order < sizes.Length - 1
            order += 1
            dblBytes /= 1024
        End While
        
        Return String.Format("{0:0.##} {1}", dblBytes, sizes(order))
    End Function
End Class