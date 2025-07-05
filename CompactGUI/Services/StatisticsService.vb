Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.Json

Public Class StatisticsService
    Private ReadOnly _statistics As New ObservableCollection(Of CompressionStatistics)
    Private ReadOnly _statsFilePath As String
    
    Public ReadOnly Property Statistics As ObservableCollection(Of CompressionStatistics)
        Get
            Return _statistics
        End Get
    End Property
    
    Public Sub New()
        _statsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CompactGUI", "compression_statistics.json")
        
        ' Create directory if it doesn't exist
        Dim directory = Path.GetDirectoryName(_statsFilePath)
        If Not Directory.Exists(directory) Then
            Directory.CreateDirectory(directory)
        End If
        
        LoadStatistics()
    End Sub
    
    Public Sub AddStatistic(statistic As CompressionStatistics)
        _statistics.Add(statistic)
        SaveStatistics()
    End Sub
    
    Public Function GetStatisticsForFolder(folderPath As String) As IEnumerable(Of CompressionStatistics)
        Return _statistics.Where(Function(s) s.FolderPath = folderPath).OrderByDescending(Function(s) s.CompressionDate)
    End Function
    
    Public Function GetTotalSpaceSaved() As Long
        Return _statistics.Sum(Function(s) s.SpaceSaved)
    End Function
    
    Public Function GetAverageCompressionRatio() As Double
        If _statistics.Count = 0 Then Return 0
        Return _statistics.Average(Function(s) s.CompressionRatio)
    End Function
    
    Public Function GetMostCompressedFileTypes() As IEnumerable(Of FileTypeStatistic)
        Dim allFileTypes = New List(Of FileTypeStatistic)
        
        For Each stat In _statistics
            For Each fileType In stat.FileTypeStats
                Dim existingType = allFileTypes.FirstOrDefault(Function(ft) ft.FileExtension = fileType.FileExtension)
                If existingType IsNot Nothing Then
                    existingType.FileCount += fileType.FileCount
                    existingType.TotalOriginalSize += fileType.TotalOriginalSize
                    existingType.TotalCompressedSize += fileType.TotalCompressedSize
                Else
                    allFileTypes.Add(fileType)
                End If
            Next
        Next
        
        Return allFileTypes.OrderByDescending(Function(ft) ft.CompressionRatio)
    End Function
    
    Public Function GetStatisticsByMonth() As Dictionary(Of String, CompressionStatistics)
        Dim monthlyStats = New Dictionary(Of String, CompressionStatistics)
        
        For Each stat In _statistics
            Dim monthKey = $"{stat.CompressionDate.Year}-{stat.CompressionDate.Month}"
            
            If monthlyStats.ContainsKey(monthKey) Then
                Dim existingStat = monthlyStats(monthKey)
                existingStat.OriginalSize += stat.OriginalSize
                existingStat.CompressedSize += stat.CompressedSize
                existingStat.FileCount += stat.FileCount
            Else
                Dim newStat = New CompressionStatistics With {
                    .FolderName = "Monthly Summary",
                    .CompressionDate = New DateTime(stat.CompressionDate.Year, stat.CompressionDate.Month, 1),
                    .OriginalSize = stat.OriginalSize,
                    .CompressedSize = stat.CompressedSize,
                    .FileCount = stat.FileCount
                }
                monthlyStats.Add(monthKey, newStat)
            End If
        Next
        
        Return monthlyStats
    End Function
    
    Public Sub SaveStatistics()
        Try
            Dim options = New JsonSerializerOptions With {
                .WriteIndented = True
            }
            Dim json = JsonSerializer.Serialize(_statistics, options)
            File.WriteAllText(_statsFilePath, json)
        Catch ex As Exception
            Debug.WriteLine($"Error saving compression statistics: {ex.Message}")
        End Try
    End Sub
    
    Public Sub LoadStatistics()
        Try
            If File.Exists(_statsFilePath) Then
                Dim json = File.ReadAllText(_statsFilePath)
                Dim loadedStats = JsonSerializer.Deserialize(Of List(Of CompressionStatistics))(json)
                _statistics.Clear()
                For Each stat In loadedStats
                    _statistics.Add(stat)
                Next
            End If
        Catch ex As Exception
            Debug.WriteLine($"Error loading compression statistics: {ex.Message}")
        End Try
    End Sub
    
    Public Sub ClearStatistics()
        _statistics.Clear()
        SaveStatistics()
    End Sub
End Class