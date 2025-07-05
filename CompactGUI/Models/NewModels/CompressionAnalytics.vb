Imports System.Collections.ObjectModel
Imports CommunityToolkit.Mvvm.ComponentModel

Public Class CompressionAnalytics : Inherits ObservableObject
    
    Public Property TotalSpaceSaved As Long
    Public Property TotalFilesCompressed As Integer
    Public Property TotalFoldersProcessed As Integer
    Public Property CompressionSessions As New ObservableCollection(Of CompressionSession)
    Public Property FileTypeStats As New Dictionary(Of String, FileTypeCompressionStats)
    
    Public ReadOnly Property AverageCompressionRatio As Double
        Get
            If CompressionSessions.Count = 0 Then Return 0
            Return CompressionSessions.Average(Function(s) s.CompressionRatio)
        End Get
    End Property
    
    Public ReadOnly Property TotalTimeSpent As TimeSpan
        Get
            Return TimeSpan.FromMilliseconds(CompressionSessions.Sum(Function(s) s.Duration.TotalMilliseconds))
        End Get
    End Property
    
    Public Sub RecordCompressionSession(session As CompressionSession)
        CompressionSessions.Add(session)
        TotalSpaceSaved += session.SpaceSaved
        TotalFilesCompressed += session.FilesProcessed
        TotalFoldersProcessed += 1
        
        ' Update file type statistics
        For Each fileType In session.FileTypeBreakdown
            If Not FileTypeStats.ContainsKey(fileType.Key) Then
                FileTypeStats(fileType.Key) = New FileTypeCompressionStats With {.Extension = fileType.Key}
            End If
            
            Dim stats = FileTypeStats(fileType.Key)
            stats.TotalFiles += fileType.Value.Count
            stats.TotalOriginalSize += fileType.Value.OriginalSize
            stats.TotalCompressedSize += fileType.Value.CompressedSize
        Next
        
        OnPropertyChanged(NameOf(AverageCompressionRatio))
        OnPropertyChanged(NameOf(TotalTimeSpent))
    End Sub
    
    Public Function GetCompressionTrends(days As Integer) As List(Of CompressionTrendData)
        Dim cutoffDate = DateTime.Now.AddDays(-days)
        Dim recentSessions = CompressionSessions.Where(Function(s) s.StartTime >= cutoffDate).ToList()
        
        Return recentSessions.GroupBy(Function(s) s.StartTime.Date) _
            .Select(Function(g) New CompressionTrendData With {
                .Date = g.Key,
                .SpaceSaved = g.Sum(Function(s) s.SpaceSaved),
                .FilesProcessed = g.Sum(Function(s) s.FilesProcessed),
                .AverageRatio = g.Average(Function(s) s.CompressionRatio)
            }).OrderBy(Function(t) t.Date).ToList()
    End Function
    
    Public Function GetTopFileTypes(count As Integer) As List(Of FileTypeCompressionStats)
        Return FileTypeStats.Values.OrderByDescending(Function(s) s.SpaceSaved).Take(count).ToList()
    End Function
    
End Class

Public Class CompressionSession
    Public Property StartTime As DateTime
    Public Property EndTime As DateTime
    Public Property FolderPath As String
    Public Property FolderName As String
    Public Property OriginalSize As Long
    Public Property CompressedSize As Long
    Public Property FilesProcessed As Integer
    Public Property CompressionMode As Core.CompressionMode
    Public Property ProfileUsed As String
    Public Property FileTypeBreakdown As New Dictionary(Of String, FileTypeSessionData)
    
    Public ReadOnly Property Duration As TimeSpan
        Get
            Return EndTime - StartTime
        End Get
    End Property
    
    Public ReadOnly Property SpaceSaved As Long
        Get
            Return OriginalSize - CompressedSize
        End Get
    End Property
    
    Public ReadOnly Property CompressionRatio As Double
        Get
            If OriginalSize = 0 Then Return 0
            Return CDbl(CompressedSize) / CDbl(OriginalSize)
        End Get
    End Property
    
End Class

Public Class FileTypeSessionData
    Public Property Count As Integer
    Public Property OriginalSize As Long
    Public Property CompressedSize As Long
End Class

Public Class FileTypeCompressionStats
    Public Property Extension As String
    Public Property TotalFiles As Integer
    Public Property TotalOriginalSize As Long
    Public Property TotalCompressedSize As Long
    
    Public ReadOnly Property SpaceSaved As Long
        Get
            Return TotalOriginalSize - TotalCompressedSize
        End Get
    End Property
    
    Public ReadOnly Property AverageCompressionRatio As Double
        Get
            If TotalOriginalSize = 0 Then Return 0
            Return CDbl(TotalCompressedSize) / CDbl(TotalOriginalSize)
        End Get
    End Property
    
End Class

Public Class CompressionTrendData
    Public Property Date As DateTime
    Public Property SpaceSaved As Long
    Public Property FilesProcessed As Integer
    Public Property AverageRatio As Double
End Class