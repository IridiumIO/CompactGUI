Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.Json
Imports System.Threading

Public Class BatchProcessingService
    Private ReadOnly _jobs As New ObservableCollection(Of BatchProcessingJob)
    Private ReadOnly _jobsFilePath As String
    Private ReadOnly _profileService As ProfileService
    Private ReadOnly _statisticsService As StatisticsService
    Private ReadOnly _watcher As Watcher.Watcher
    
    Private _currentJob As BatchProcessingJob
    Private _cancellationTokenSource As CancellationTokenSource
    Private _pauseTokenSource As CancellationTokenSource
    Private _pauseCompletionSource As TaskCompletionSource(Of Boolean)
    
    Public ReadOnly Property Jobs As ObservableCollection(Of BatchProcessingJob)
        Get
            Return _jobs
        End Get
    End Property
    
    Public ReadOnly Property IsProcessing As Boolean
        Get
            Return _currentJob IsNot Nothing AndAlso _currentJob.Status = BatchJobStatus.Running
        End Get
    End Property
    
    Public Event JobStatusChanged(job As BatchProcessingJob)
    Public Event JobProgressChanged(job As BatchProcessingJob, progress As Double)
    
    Public Sub New(profileService As ProfileService, statisticsService As StatisticsService, watcher As Watcher.Watcher)
        _profileService = profileService
        _statisticsService = statisticsService
        _watcher = watcher
        _jobsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CompactGUI", "batch_jobs.json")
        
        ' Create directory if it doesn't exist
        Dim directory = Path.GetDirectoryName(_jobsFilePath)
        If Not Directory.Exists(directory) Then
            Directory.CreateDirectory(directory)
        End If
        
        LoadJobs()
    End Sub
    
    Public Sub AddJob(job As BatchProcessingJob)
        _jobs.Add(job)
        SaveJobs()
        RaiseEvent JobStatusChanged(job)
    End Sub
    
    Public Sub UpdateJob(job As BatchProcessingJob)
        Dim existingJob = _jobs.FirstOrDefault(Function(j) j.Id = job.Id)
        If existingJob IsNot Nothing Then
            Dim index = _jobs.IndexOf(existingJob)
            _jobs(index) = job
            SaveJobs()
            RaiseEvent JobStatusChanged(job)
        End If
    End Sub
    
    Public Sub RemoveJob(job As BatchProcessingJob)
        _jobs.Remove(job)
        SaveJobs()
    End Sub
    
    Public Async Function StartJobAsync(job As BatchProcessingJob) As Task
        If IsProcessing Then
            Throw New InvalidOperationException("Another job is already in progress")
        End If
        
        _currentJob = job
        _cancellationTokenSource = New CancellationTokenSource()
        _pauseTokenSource = New CancellationTokenSource()
        _pauseCompletionSource = New TaskCompletionSource(Of Boolean)
        
        job.Status = BatchJobStatus.Running
        UpdateJob(job)
        
        Try
            ' Get the compression profile if specified
            Dim profile As CompressionProfile = Nothing
            If job.CompressionProfileId.HasValue Then
                profile = _profileService.Profiles.FirstOrDefault(Function(p) p.Id = job.CompressionProfileId.Value)
            End If
            
            ' Process each folder
            Dim remainingFolders = job.FolderPaths.Except(job.CompletedFolders).Except(job.FailedFolders.Keys).ToList()
            
            For i As Integer = 0 To remainingFolders.Count - 1
                Dim folderPath = remainingFolders(i)
                job.CurrentFolder = folderPath
                
                ' Check if cancelled
                If _cancellationTokenSource.Token.IsCancellationRequested Then
                    job.Status = BatchJobStatus.Cancelled
                    UpdateJob(job)
                    Return
                End If
                
                ' Check if paused
                If _pauseTokenSource.Token.IsCancellationRequested Then
                    job.Status = BatchJobStatus.Paused
                    UpdateJob(job)
                    Await _pauseCompletionSource.Task
                    job.Status = BatchJobStatus.Running
                    UpdateJob(job)
                End If
                
                Try
                    ' Create a compressable folder
                    Dim folder As CompressableFolder = CompressableFolderFactory.CreateCompressableFolder(folderPath)
                    
                    ' Apply profile if available
                    If profile IsNot Nothing Then
                        profile.ApplyToFolder(folder)
                    End If
                    
                    ' Analyze the folder
                    Dim startTime = DateTime.Now
                    Await folder.AnalyseFolderAsync()
                    
                    ' Compress the folder
                    Await folder.CompressFolder()
                    
                    ' Calculate statistics
                    Dim endTime = DateTime.Now
                    Dim compressionTime = endTime - startTime
                    
                    ' Create statistics
                    Dim stats = New CompressionStatistics With {
                        .FolderPath = folderPath,
                        .FolderName = folder.DisplayName,
                        .CompressionDate = endTime,
                        .OriginalSize = folder.UncompressedBytes,
                        .CompressedSize = folder.CompressedBytes,
                        .CompressionAlgorithm = folder.CompressionOptions.SelectedCompressionMode,
                        .CompressionTime = compressionTime,
                        .FileCount = folder.FileCount
                    }
                    
                    ' Add file type statistics
                    For Each fileType In folder.FileTypeBreakdown
                        stats.FileTypeStats.Add(New FileTypeStatistic With {
                            .FileExtension = fileType.Key,
                            .FileCount = fileType.Value.Count,
                            .TotalOriginalSize = fileType.Value.TotalSize,
                            .TotalCompressedSize = fileType.Value.TotalSize * (1 - folder.CompressionRatio) ' Estimate
                        })
                    Next
                    
                    ' Save statistics
                    _statisticsService.AddStatistic(stats)
                    
                    ' Add to watcher if needed
                    If folder.CompressionOptions.WatchFolderForChanges Then
                        _watcher.UpdateWatched(folder.FolderName, folder.Analyser, True)
                    End If
                    
                    ' Update job progress
                    job.CompletedFolders.Add(folderPath)
                    job.Progress = (job.CompletedFolders.Count / job.TotalFolders) * 100
                    UpdateJob(job)
                    RaiseEvent JobProgressChanged(job, job.Progress)
                    
                Catch ex As Exception
                    job.FailedFolders.Add(folderPath, ex.Message)
                    UpdateJob(job)
                End Try
            Next
            
            ' Update job status
            If job.FailedFolders.Count > 0 AndAlso job.CompletedFolders.Count = 0 Then
                job.Status = BatchJobStatus.Failed
            Else
                job.Status = BatchJobStatus.Completed
            End If
            
            UpdateJob(job)
            
        Catch ex As Exception
            job.Status = BatchJobStatus.Failed
            UpdateJob(job)
            Debug.WriteLine($"Error processing batch job: {ex.Message}")
        Finally
            _currentJob = Nothing
            _cancellationTokenSource?.Dispose()
            _pauseTokenSource?.Dispose()
        End Try
    End Function
    
    Public Sub PauseCurrentJob()
        If _currentJob IsNot Nothing AndAlso _currentJob.Status = BatchJobStatus.Running Then
            _pauseTokenSource.Cancel()
        End If
    End Sub
    
    Public Sub ResumeCurrentJob()
        If _currentJob IsNot Nothing AndAlso _currentJob.Status = BatchJobStatus.Paused Then
            _pauseTokenSource = New CancellationTokenSource()
            _pauseCompletionSource.SetResult(True)
            _pauseCompletionSource = New TaskCompletionSource(Of Boolean)
        End If
    End Sub
    
    Public Sub CancelCurrentJob()
        If _currentJob IsNot Nothing AndAlso (_currentJob.Status = BatchJobStatus.Running OrElse _currentJob.Status = BatchJobStatus.Paused) Then
            _cancellationTokenSource.Cancel()
            If _currentJob.Status = BatchJobStatus.Paused Then
                _pauseCompletionSource.SetResult(True)
            End If
        End If
    End Sub
    
    Public Sub SaveJobs()
        Try
            Dim options = New JsonSerializerOptions With {
                .WriteIndented = True
            }
            Dim json = JsonSerializer.Serialize(_jobs, options)
            File.WriteAllText(_jobsFilePath, json)
        Catch ex As Exception
            Debug.WriteLine($"Error saving batch jobs: {ex.Message}")
        End Try
    End Sub
    
    Public Sub LoadJobs()
        Try
            If File.Exists(_jobsFilePath) Then
                Dim json = File.ReadAllText(_jobsFilePath)
                Dim loadedJobs = JsonSerializer.Deserialize(Of List(Of BatchProcessingJob))(json)
                _jobs.Clear()
                For Each job In loadedJobs
                    ' Reset any running or paused jobs to pending
                    If job.Status = BatchJobStatus.Running OrElse job.Status = BatchJobStatus.Paused Then
                        job.Status = BatchJobStatus.Pending
                    End If
                    _jobs.Add(job)
                Next
            End If
        Catch ex As Exception
            Debug.WriteLine($"Error loading batch jobs: {ex.Message}")
        End Try
    End Sub
End Class