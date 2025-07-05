Imports System.Collections.ObjectModel
Imports System.Threading
Imports CommunityToolkit.Mvvm.ComponentModel

Public Class BatchCompressionManager : Inherits ObservableObject
    
    Public Property BatchJobs As New ObservableCollection(Of BatchCompressionJob)
    Public Property IsProcessing As Boolean = False
    Public Property CurrentJob As BatchCompressionJob
    Public Property OverallProgress As Integer = 0
    
    Private _cancellationTokenSource As CancellationTokenSource
    
    Public Event JobCompleted As EventHandler(Of BatchJobCompletedEventArgs)
    Public Event BatchCompleted As EventHandler
    
    Public Sub AddJob(job As BatchCompressionJob)
        BatchJobs.Add(job)
    End Sub
    
    Public Sub RemoveJob(job As BatchCompressionJob)
        If Not IsProcessing OrElse job IsNot CurrentJob Then
            BatchJobs.Remove(job)
        End If
    End Sub
    
    Public Async Function StartBatchProcessing() As Task
        If IsProcessing Then Return
        
        IsProcessing = True
        _cancellationTokenSource = New CancellationTokenSource()
        
        Try
            Dim totalJobs = BatchJobs.Count
            Dim completedJobs = 0
            
            For Each job In BatchJobs.ToList()
                If _cancellationTokenSource.Token.IsCancellationRequested Then Exit For
                
                CurrentJob = job
                job.Status = BatchJobStatus.Processing
                
                Await ProcessBatchJob(job, _cancellationTokenSource.Token)
                
                completedJobs += 1
                OverallProgress = CInt((completedJobs / totalJobs) * 100)
                
                RaiseEvent JobCompleted(Me, New BatchJobCompletedEventArgs(job))
            Next
            
            RaiseEvent BatchCompleted(Me, EventArgs.Empty)
            
        Finally
            IsProcessing = False
            CurrentJob = Nothing
            OverallProgress = 0
        End Try
    End Function
    
    Public Sub CancelBatch()
        _cancellationTokenSource?.Cancel()
    End Sub
    
    Private Async Function ProcessBatchJob(job As BatchCompressionJob, cancellationToken As CancellationToken) As Task
        Try
            job.StartTime = DateTime.Now
            
            For Each folderPath In job.FolderPaths
                If cancellationToken.IsCancellationRequested Then Exit For
                
                Dim folder = CompressableFolderFactory.CreateCompressableFolder(folderPath)
                
                ' Apply job settings
                folder.CompressionOptions.SelectedCompressionMode = job.CompressionMode
                folder.CompressionOptions.SkipPoorlyCompressedFileTypes = job.SkipPoorlyCompressed
                folder.CompressionOptions.SkipUserSubmittedFiletypes = job.SkipUserSubmitted
                
                ' Track progress
                AddHandler folder.CompressionProgressChanged, Sub(sender, e)
                    job.CurrentProgress = e.ProgressPercent
                    job.CurrentFile = e.FileName
                End Sub
                
                Await folder.CompressFolder()
                job.ProcessedFolders.Add(folderPath)
            Next
            
            job.Status = If(cancellationToken.IsCancellationRequested, BatchJobStatus.Cancelled, BatchJobStatus.Completed)
            job.EndTime = DateTime.Now
            
        Catch ex As Exception
            job.Status = BatchJobStatus.Failed
            job.ErrorMessage = ex.Message
            job.EndTime = DateTime.Now
        End Try
    End Function
    
End Class

Public Class BatchCompressionJob : Inherits ObservableObject
    
    Public Property Name As String
    Public Property FolderPaths As New List(Of String)
    Public Property CompressionMode As Core.CompressionMode
    Public Property SkipPoorlyCompressed As Boolean = True
    Public Property SkipUserSubmitted As Boolean = True
    Public Property Priority As BatchJobPriority = BatchJobPriority.Normal
    Public Property Status As BatchJobStatus = BatchJobStatus.Pending
    Public Property CurrentProgress As Integer = 0
    Public Property CurrentFile As String = ""
    Public Property ProcessedFolders As New List(Of String)
    Public Property StartTime As DateTime?
    Public Property EndTime As DateTime?
    Public Property ErrorMessage As String = ""
    
    Public ReadOnly Property Duration As TimeSpan?
        Get
            If StartTime.HasValue AndAlso EndTime.HasValue Then
                Return EndTime.Value - StartTime.Value
            End If
            Return Nothing
        End Get
    End Property
    
    Public ReadOnly Property TotalFolders As Integer
        Get
            Return FolderPaths.Count
        End Get
    End Property
    
    Public ReadOnly Property CompletedFolders As Integer
        Get
            Return ProcessedFolders.Count
        End Get
    End Property
    
End Class

Public Enum BatchJobStatus
    Pending
    Processing
    Completed
    Failed
    Cancelled
End Enum

Public Enum BatchJobPriority
    Low
    Normal
    High
End Enum

Public Class BatchJobCompletedEventArgs : Inherits EventArgs
    Public ReadOnly Property Job As BatchCompressionJob
    
    Public Sub New(job As BatchCompressionJob)
        Me.Job = job
    End Sub
End Class