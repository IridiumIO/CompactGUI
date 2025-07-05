Imports System.Collections.ObjectModel

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Public Class BatchProcessingViewModel : Inherits ObservableObject
    Private ReadOnly _batchProcessingService As BatchProcessingService
    Private ReadOnly _profileService As ProfileService
    
    Public Property Jobs As New ObservableCollection(Of BatchProcessingJob)
    Public Property SelectedJob As BatchProcessingJob
    Public Property EditingJob As BatchProcessingJob
    Public Property IsEditing As Boolean = False
    
    Public Property FolderPaths As New ObservableCollection(Of String)
    Public Property SelectedFolder As String
    
    Public Property CompressionProfiles As New ObservableCollection(Of CompressionProfile)
    Public Property SelectedProfile As CompressionProfile
    
    Public ReadOnly Property FolderCountText As String
        Get
            Return $"{FolderPaths.Count} folder(s)"
        End Get
    End Property
    
    Public ReadOnly Property PauseResumeButtonText As String
        Get
            If _batchProcessingService.IsProcessing Then
                Return "Pause"
            Else
                Return "Resume"
            End If
        End Get
    End Property
    
    Public Sub New(batchProcessingService As BatchProcessingService, profileService As ProfileService)
        _batchProcessingService = batchProcessingService
        _profileService = profileService
        
        AddHandler _batchProcessingService.JobStatusChanged, AddressOf OnJobStatusChanged
        AddHandler _batchProcessingService.JobProgressChanged, AddressOf OnJobProgressChanged
        
        LoadJobs()
        LoadProfiles()
    End Sub
    
    Private Sub OnJobStatusChanged(job As BatchProcessingJob)
        Application.Current.Dispatcher.Invoke(Sub()
                                                  OnPropertyChanged(NameOf(PauseResumeButtonText))
                                              End Sub)
    End Sub
    
    Private Sub OnJobProgressChanged(job As BatchProcessingJob, progress As Double)
        Application.Current.Dispatcher.Invoke(Sub()
                                                  ' Update the job in the collection
                                                  Dim index = Jobs.IndexOf(Jobs.FirstOrDefault(Function(j) j.Id = job.Id))
                                                  If index >= 0 Then
                                                      Jobs(index) = job
                                                  End If
                                              End Sub)
    End Sub
    
    Private Sub LoadJobs()
        Jobs.Clear()
        For Each job In _batchProcessingService.Jobs
            Jobs.Add(job)
        Next
    End Sub
    
    Private Sub LoadProfiles()
        CompressionProfiles.Clear()
        
        ' Add "None" option
        CompressionProfiles.Add(New CompressionProfile With {
            .Id = Guid.Empty,
            .Name = "None (Use Default Settings)"
        })
        
        ' Add all profiles
        For Each profile In _profileService.Profiles
            CompressionProfiles.Add(profile)
        Next
        
        ' Select the first profile
        SelectedProfile = CompressionProfiles.FirstOrDefault()
    End Sub
    
    Public ReadOnly Property CreateJobCommand As IRelayCommand = New RelayCommand(AddressOf CreateJob)
    
    Private Sub CreateJob()
        EditingJob = New BatchProcessingJob With {
            .Name = "New Batch Job"
        }
        
        FolderPaths.Clear()
        SelectedProfile = CompressionProfiles.FirstOrDefault()
        
        IsEditing = True
        OnPropertyChanged(NameOf(EditingJob))
    End Sub
    
    Public ReadOnly Property StartJobCommand As IRelayCommand = New RelayCommand(AddressOf StartJob, AddressOf CanStartJob)
    
    Private Function CanStartJob() As Boolean
        Return SelectedJob IsNot Nothing AndAlso 
               (SelectedJob.Status = BatchJobStatus.Pending OrElse 
                SelectedJob.Status = BatchJobStatus.Failed OrElse 
                SelectedJob.Status = BatchJobStatus.Cancelled)
    End Function
    
    Private Async Sub StartJob()
        Try
            Await _batchProcessingService.StartJobAsync(SelectedJob)
        Catch ex As Exception
            Application.GetService(Of CustomSnackBarService)().ShowError("Error starting job", ex.Message)
        End Try
    End Sub
    
    Public ReadOnly Property PauseResumeJobCommand As IRelayCommand = New RelayCommand(AddressOf PauseResumeJob, AddressOf CanPauseResumeJob)
    
    Private Function CanPauseResumeJob() As Boolean
        Return _batchProcessingService.IsProcessing OrElse 
               (SelectedJob IsNot Nothing AndAlso SelectedJob.Status = BatchJobStatus.Paused)
    End Function
    
    Private Sub PauseResumeJob()
        If _batchProcessingService.IsProcessing Then
            _batchProcessingService.PauseCurrentJob()
        Else
            _batchProcessingService.ResumeCurrentJob()
        End If
    End Sub
    
    Public ReadOnly Property CancelJobCommand As IRelayCommand = New RelayCommand(AddressOf CancelJob, AddressOf CanCancelJob)
    
    Private Function CanCancelJob() As Boolean
        Return _batchProcessingService.IsProcessing OrElse 
               (SelectedJob IsNot Nothing AndAlso SelectedJob.Status = BatchJobStatus.Paused)
    End Function
    
    Private Sub CancelJob()
        _batchProcessingService.CancelCurrentJob()
    End Sub
    
    Public ReadOnly Property DeleteJobCommand As IRelayCommand = New RelayCommand(AddressOf DeleteJob, AddressOf CanDeleteJob)
    
    Private Function CanDeleteJob() As Boolean
        Return SelectedJob IsNot Nothing AndAlso Not _batchProcessingService.IsProcessing
    End Function
    
    Private Async Sub DeleteJob()
        Dim confirmed = Await Application.GetService(Of IWindowService)().ShowMessageBox("Delete Job", $"Are you sure you want to delete the job '{SelectedJob.DisplayName}'?")
        
        If confirmed Then
            _batchProcessingService.RemoveJob(SelectedJob)
            LoadJobs()
        End If
    End Sub
    
    Public ReadOnly Property AddFolderCommand As IRelayCommand = New RelayCommand(AddressOf AddFolder)
    
    Private Sub AddFolder()
        Dim folderBrowser = New System.Windows.Forms.FolderBrowserDialog With {
            .Description = "Select folder to compress",
            .ShowNewFolderButton = True
        }
        
        If folderBrowser.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            If Not FolderPaths.Contains(folderBrowser.SelectedPath) Then
                FolderPaths.Add(folderBrowser.SelectedPath)
                OnPropertyChanged(NameOf(FolderCountText))
            End If
        End If
    End Sub
    
    Public ReadOnly Property RemoveFolderCommand As IRelayCommand = New RelayCommand(AddressOf RemoveFolder, AddressOf CanRemoveFolder)
    
    Private Function CanRemoveFolder() As Boolean
        Return SelectedFolder IsNot Nothing
    End Function
    
    Private Sub RemoveFolder()
        FolderPaths.Remove(SelectedFolder)
        OnPropertyChanged(NameOf(FolderCountText))
    End Sub
    
    Public ReadOnly Property SaveJobCommand As IRelayCommand = New RelayCommand(AddressOf SaveJob, AddressOf CanSaveJob)
    
    Private Function CanSaveJob() As Boolean
        Return EditingJob IsNot Nothing AndAlso FolderPaths.Count > 0
    End Function
    
    Private Sub SaveJob()
        ' Update folder paths
        EditingJob.FolderPaths = FolderPaths.ToList()
        
        ' Update compression profile
        If SelectedProfile IsNot Nothing AndAlso SelectedProfile.Id <> Guid.Empty Then
            EditingJob.CompressionProfileId = SelectedProfile.Id
        Else
            EditingJob.CompressionProfileId = Nothing
        End If
        
        ' Add or update job
        If Jobs.Any(Function(j) j.Id = EditingJob.Id) Then
            _batchProcessingService.UpdateJob(EditingJob)
        Else
            _batchProcessingService.AddJob(EditingJob)
        End If
        
        IsEditing = False
        LoadJobs()
    End Sub
    
    Public ReadOnly Property CancelEditCommand As IRelayCommand = New RelayCommand(AddressOf CancelEdit)
    
    Private Sub CancelEdit()
        IsEditing = False
    End Sub
End Class