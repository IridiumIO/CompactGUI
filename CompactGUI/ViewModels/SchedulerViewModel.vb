Imports System.Collections.ObjectModel

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Public Class SchedulerViewModel : Inherits ObservableObject
    Private ReadOnly _schedulerService As SchedulerService
    
    Public Property Tasks As New ObservableCollection(Of ScheduledTask)
    Public Property SelectedTask As ScheduledTask
    Public Property EditingTask As ScheduledTask
    Public Property IsEditing As Boolean = False
    Public Property IsAdding As Boolean = False
    
    Public Property ScheduledDate As DateTime = DateTime.Now
    Public Property ScheduledTime As TimeSpan = DateTime.Now.TimeOfDay
    
    Public Property RecurrencePatternIndex As Integer = 0
    
    Public ReadOnly Property RecurrenceIntervalText As String
        Get
            If EditingTask Is Nothing Then Return String.Empty
            
            Select Case EditingTask.RecurrencePattern
                Case RecurrencePattern.Daily
                    Return If(EditingTask.RecurrenceInterval = 1, "day", "days")
                Case RecurrencePattern.Weekly
                    Return If(EditingTask.RecurrenceInterval = 1, "week", "weeks")
                Case RecurrencePattern.Monthly
                    Return If(EditingTask.RecurrenceInterval = 1, "month", "months")
                Case Else
                    Return String.Empty
            End Select
        End Get
    End Property
    
    Public Sub New(schedulerService As SchedulerService)
        _schedulerService = schedulerService
        
        LoadTasks()
    End Sub
    
    Private Sub LoadTasks()
        Tasks.Clear()
        For Each task In _schedulerService.Tasks
            Tasks.Add(task)
        Next
    End Sub
    
    Public ReadOnly Property AddTaskCommand As IRelayCommand = New RelayCommand(AddressOf AddTask)
    
    Private Sub AddTask()
        EditingTask = New ScheduledTask With {
            .ScheduledTime = DateTime.Now.AddHours(1)
        }
        
        ScheduledDate = EditingTask.ScheduledTime.Date
        ScheduledTime = EditingTask.ScheduledTime.TimeOfDay
        
        IsAdding = True
        IsEditing = True
        OnPropertyChanged(NameOf(EditingTask))
    End Sub
    
    Public ReadOnly Property EditTaskCommand As IRelayCommand = New RelayCommand(AddressOf EditTask, AddressOf CanEditTask)
    
    Private Function CanEditTask() As Boolean
        Return SelectedTask IsNot Nothing
    End Function
    
    Private Sub EditTask()
        EditingTask = New ScheduledTask With {
            .Id = SelectedTask.Id,
            .Name = SelectedTask.Name,
            .FolderPath = SelectedTask.FolderPath,
            .CompressionMode = SelectedTask.CompressionMode,
            .SkipNonCompressable = SelectedTask.SkipNonCompressable,
            .SkipUserNonCompressable = SelectedTask.SkipUserNonCompressable,
            .IsRecurring = SelectedTask.IsRecurring,
            .RecurrencePattern = SelectedTask.RecurrencePattern,
            .RecurrenceInterval = SelectedTask.RecurrenceInterval,
            .ScheduledTime = SelectedTask.ScheduledTime,
            .LastRunTime = SelectedTask.LastRunTime,
            .IsEnabled = SelectedTask.IsEnabled,
            .CreatedAt = SelectedTask.CreatedAt
        }
        
        ScheduledDate = EditingTask.ScheduledTime.Date
        ScheduledTime = EditingTask.ScheduledTime.TimeOfDay
        RecurrencePatternIndex = CInt(EditingTask.RecurrencePattern)
        
        IsAdding = False
        IsEditing = True
        OnPropertyChanged(NameOf(EditingTask))
        OnPropertyChanged(NameOf(RecurrencePatternIndex))
    End Sub
    
    Public ReadOnly Property DeleteTaskCommand As IRelayCommand = New RelayCommand(AddressOf DeleteTask, AddressOf CanEditTask)
    
    Private Async Sub DeleteTask()
        Dim confirmed = Await Application.GetService(Of IWindowService)().ShowMessageBox("Delete Task", $"Are you sure you want to delete the task '{SelectedTask.DisplayName}'?")
        
        If confirmed Then
            _schedulerService.RemoveTask(SelectedTask)
            LoadTasks()
        End If
    End Sub
    
    Public ReadOnly Property RunSelectedTaskCommand As IRelayCommand = New RelayCommand(AddressOf RunSelectedTask, AddressOf CanEditTask)
    
    Private Async Sub RunSelectedTask()
        Try
            Await _schedulerService.ExecuteTask(SelectedTask)
            Application.GetService(Of CustomSnackBarService)().ShowMessage("Task executed", $"Task '{SelectedTask.DisplayName}' has been executed successfully.")
        Catch ex As Exception
            Application.GetService(Of CustomSnackBarService)().ShowError("Error executing task", ex.Message)
        End Try
    End Sub
    
    Public ReadOnly Property SaveTaskCommand As IRelayCommand = New RelayCommand(AddressOf SaveTask)
    
    Private Sub SaveTask()
        ' Update scheduled time from date and time components
        EditingTask.ScheduledTime = ScheduledDate.Date.Add(ScheduledTime)
        
        ' Update recurrence pattern from index
        EditingTask.RecurrencePattern = CType(RecurrencePatternIndex, RecurrencePattern)
        
        If IsAdding Then
            _schedulerService.AddTask(EditingTask)
        Else
            _schedulerService.UpdateTask(EditingTask)
        End If
        
        IsEditing = False
        IsAdding = False
        LoadTasks()
    End Sub
    
    Public ReadOnly Property CancelEditCommand As IRelayCommand = New RelayCommand(AddressOf CancelEdit)
    
    Private Sub CancelEdit()
        IsEditing = False
        IsAdding = False
    End Sub
    
    Public ReadOnly Property BrowseFolderCommand As IRelayCommand = New RelayCommand(AddressOf BrowseFolder)
    
    Private Sub BrowseFolder()
        Dim folderBrowser = New System.Windows.Forms.FolderBrowserDialog With {
            .Description = "Select folder to compress",
            .ShowNewFolderButton = True
        }
        
        If folderBrowser.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            EditingTask.FolderPath = folderBrowser.SelectedPath
            OnPropertyChanged(NameOf(EditingTask))
        End If
    End Sub
End Class