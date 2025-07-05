Imports System.Collections.ObjectModel
Imports System.IO

Public Class FileTypeAnalysisService
    Private ReadOnly _statisticsService As StatisticsService
    
    Public Sub New(statisticsService As StatisticsService)
        _statisticsService = statisticsService
    End Sub
    
    Public Function GetFileTypeCompressionRatios() As Dictionary(Of String, Double)
        Dim fileTypeRatios = New Dictionary(Of String, Double)
        Dim fileTypeCounts = New Dictionary(Of String, Integer)
        
        ' Get all file type statistics from the statistics service
        Dim allFileTypes = _statisticsService.GetMostCompressedFileTypes()
        
        For Each fileType In allFileTypes
            If fileTypeRatios.ContainsKey(fileType.FileExtension) Then
                ' Update existing entry with weighted average
                Dim currentRatio = fileTypeRatios(fileType.FileExtension)
                Dim currentCount = fileTypeCounts(fileType.FileExtension)
                Dim newCount = currentCount + fileType.FileCount
                Dim newRatio = ((currentRatio * currentCount) + (fileType.CompressionRatio * fileType.FileCount)) / newCount
                
                fileTypeRatios(fileType.FileExtension) = newRatio
                fileTypeCounts(fileType.FileExtension) = newCount
            Else
                ' Add new entry
                fileTypeRatios.Add(fileType.FileExtension, fileType.CompressionRatio)
                fileTypeCounts.Add(fileType.FileExtension, fileType.FileCount)
            End If
        Next
        
        Return fileTypeRatios
    End Function
    
    Public Function GetRecommendedSkipList() As List(Of String)
        Dim fileTypeRatios = GetFileTypeCompressionRatios()
        Dim skipList = New List(Of String)
        
        ' Add file types with poor compression ratio (less than 10%)
        For Each fileType In fileTypeRatios
            If fileType.Value < 0.1 Then ' Less than 10% compression
                skipList.Add(fileType.Key)
            End If
        Next
        
        Return skipList
    End Function
    
    Public Function AnalyzeFolderFileTypes(folderPath As String) As Dictionary(Of String, (Count As Integer, TotalSize As Long))
        Dim fileTypeBreakdown = New Dictionary(Of String, (Count As Integer, TotalSize As Long))
        
        Try
            ' Get all files in the folder and subfolders
            Dim files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
            
            For Each file In files
                Dim extension = Path.GetExtension(file).ToLowerInvariant()
                Dim fileInfo = New FileInfo(file)
                
                If fileTypeBreakdown.ContainsKey(extension) Then
                    ' Update existing entry
                    Dim current = fileTypeBreakdown(extension)
                    fileTypeBreakdown(extension) = (current.Count + 1, current.TotalSize + fileInfo.Length)
                Else
                    ' Add new entry
                    fileTypeBreakdown.Add(extension, (1, fileInfo.Length))
                End If
            Next
        Catch ex As Exception
            Debug.WriteLine($"Error analyzing folder file types: {ex.Message}")
        End Try
        
        Return fileTypeBreakdown
    End Function
    
    Public Function GetCompressionRecommendation(folderPath As String) As (RecommendedAlgorithm As Integer, SkipNonCompressable As Boolean, EstimatedSavings As Double)
        Dim fileTypeBreakdown = AnalyzeFolderFileTypes(folderPath)
        Dim fileTypeRatios = GetFileTypeCompressionRatios()
        
        ' Calculate total size and estimated compressed size for different algorithms
        Dim totalSize As Long = 0
        Dim estimatedSavingsXpress4k As Double = 0
        Dim estimatedSavingsXpress8k As Double = 0
        Dim estimatedSavingsXpress16k As Double = 0
        Dim estimatedSavingsLzx As Double = 0
        
        ' Compression ratios for different algorithms (estimated)
        Dim ratioMultiplierXpress4k = 0.8 ' 20% better than average
        Dim ratioMultiplierXpress8k = 0.7 ' 30% better than average
        Dim ratioMultiplierXpress16k = 0.6 ' 40% better than average
        Dim ratioMultiplierLzx = 0.5 ' 50% better than average
        
        ' Calculate total size and potential savings
        For Each fileType In fileTypeBreakdown
            Dim extension = fileType.Key
            Dim count = fileType.Value.Count
            Dim size = fileType.Value.TotalSize
            totalSize += size
            
            ' If we have statistics for this file type, use them
            If fileTypeRatios.ContainsKey(extension) Then
                Dim compressionRatio = fileTypeRatios(extension)
                
                ' Skip files with poor compression ratio
                If compressionRatio < 0.1 Then Continue For
                
                ' Calculate estimated savings for each algorithm
                estimatedSavingsXpress4k += size * compressionRatio * ratioMultiplierXpress4k
                estimatedSavingsXpress8k += size * compressionRatio * ratioMultiplierXpress8k
                estimatedSavingsXpress16k += size * compressionRatio * ratioMultiplierXpress16k
                estimatedSavingsLzx += size * compressionRatio * ratioMultiplierLzx
            Else
                ' For unknown file types, use conservative estimates
                estimatedSavingsXpress4k += size * 0.2 ' 20% savings
                estimatedSavingsXpress8k += size * 0.25 ' 25% savings
                estimatedSavingsXpress16k += size * 0.3 ' 30% savings
                estimatedSavingsLzx += size * 0.35 ' 35% savings
            End If
        Next
        
        ' Determine best algorithm based on estimated savings
        Dim bestSavings = Math.Max(Math.Max(estimatedSavingsXpress4k, estimatedSavingsXpress8k), 
                                  Math.Max(estimatedSavingsXpress16k, estimatedSavingsLzx))
        
        Dim recommendedAlgorithm As Integer
        Dim estimatedSavingsRatio As Double
        
        If bestSavings = estimatedSavingsLzx Then
            recommendedAlgorithm = 3 ' LZX
            estimatedSavingsRatio = estimatedSavingsLzx / totalSize
        ElseIf bestSavings = estimatedSavingsXpress16k Then
            recommendedAlgorithm = 2 ' XPRESS16K
            estimatedSavingsRatio = estimatedSavingsXpress16k / totalSize
        ElseIf bestSavings = estimatedSavingsXpress8k Then
            recommendedAlgorithm = 1 ' XPRESS8K
            estimatedSavingsRatio = estimatedSavingsXpress8k / totalSize
        Else
            recommendedAlgorithm = 0 ' XPRESS4K
            estimatedSavingsRatio = estimatedSavingsXpress4k / totalSize
        End If
        
        ' Determine if skipping non-compressable files is recommended
        Dim skipNonCompressable = True ' Generally recommended
        
        Return (recommendedAlgorithm, skipNonCompressable, estimatedSavingsRatio)
    End Function
End Class