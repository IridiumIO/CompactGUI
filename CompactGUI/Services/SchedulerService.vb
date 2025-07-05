Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.Json
Imports System.Timers

Public Class SchedulerService
    Private ReadOnly _tasks As New ObservableCollection(Of ScheduledTask)
    Private ReadOnly _timer As New Timer(60000) ' Check every minute
    Private ReadOnly _taskFilePath As String
    Private ReadOnly _watcher As Watcher.Watcher
    
    Public ReadOnly Property Tasks As ObservableCollection(Of ScheduledTask)
        Get
            Return _tasks
        End Get
    End Property
    
    Public Sub New(watcher As Watcher.Watcher)
        _watcher = watcher
        _taskFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CompactGUI", "scheduled_tasks.json")
        
        ' Create directory if it doesn't exist
        Dim directory = Path.GetDirectoryName(_taskFilePath)
        If Not Directory.Exists(directory) Then
            Directory.CreateDirectory(directory)
        End If
        
        LoadTasks()
        
        AddHandler _timer.Elapsed, AddressOf CheckScheduledTasks
        _timer.Start()
    End Sub
    
    Private Sub CheckScheduledTasks(sender As Object, e As ElapsedEventArgs)
        For Each task In _tasks.Where(Function(t) t.IsEnabled AndAlso t.IsOverdue)
            ExecuteTask(task)
        Next
    End Sub
    
    Public Async Function ExecuteTask(task As ScheduledTask) As Task
        Try
            ' Create a compressable folder
            Dim folder As CompressableFolder = CompressableFolderFactory.CreateCompressableFolder(task.FolderPath)
            
            ' Set compression options
            folder.CompressionOptions.SelectedCompressionMode = task.CompressionMode
            folder.CompressionOptions.SkipPoorlyCompressedFileTypes = task.SkipNonCompressable
            folder.CompressionOptions.SkipUserSubmittedFiletypes = task.SkipUserNonCompressable
            
            ' Analyze the folder
            Await folder.AnalyseFolderAsync()
            
            ' Compress the folder
            Await folder.CompressFolder()
            
            ' Update the task
            task.LastRunTime = DateTime.Now
            SaveTasks()
            
            ' Show notification if enabled
            If SettingsHandler.AppSettings.ShowNotifications Then
                Application.GetService(Of TrayNotifierService).Notify_ScheduledTaskCompleted(task.DisplayName)
            End If
            
            ' Add to watcher if needed
            If folder.CompressionOptions.WatchFolderForChanges Then
                _watcher.UpdateWatched(folder.FolderName, folder.Analyser, True)
            End If
            
        Catch ex As Exception
            Debug.WriteLine($"Error executing scheduled task: {ex.Message}")
            ' Show error notification
            If SettingsHandler.AppSettings.ShowNotifications Then
                Application.GetService(Of TrayNotifierService).Notify_ScheduledTaskFailed(task.DisplayName, ex.Message)
            End If
        End Try
    End Function
    
    Public Sub AddTask(task As ScheduledTask)
        _tasks.Add(task)
        SaveTasks()
    End Sub
    
    Public Sub UpdateTask(task As ScheduledTask)
        Dim existingTask = _tasks.FirstOrDefault(Function(t) t.Id = task.Id)
        If existingTask IsNot Nothing Then
            Dim index = _tasks.IndexOf(existingTask)
            _tasks(index) = task
            SaveTasks()
        End If
    End Sub
    
    Public Sub RemoveTask(task As ScheduledTask)
        _tasks.Remove(task)
        SaveTasks()
    End Sub
    
    Public Sub SaveTasks()
        Try
            Dim options = New JsonSerializerOptions With {
                .WriteIndented = True
            }
            Dim json = JsonSerializer.Serialize(_tasks, options)
            File.WriteAllText(_taskFilePath, json)
        Catch ex As Exception
            Debug.WriteLine($"Error saving scheduled tasks: {ex.Message}")
        End Try
    End Sub
    
    Public Sub LoadTasks()
        Try
            If File.Exists(_taskFilePath) Then
                Dim json = File.ReadAllText(_taskFilePath)
                Dim loadedTasks = JsonSerializer.Deserialize(Of List(Of ScheduledTask))(json)
                _tasks.Clear()
                For Each task In loadedTasks
                    _tasks.Add(task)
                Next
            End If
        Catch ex As Exception
            Debug.WriteLine($"Error loading scheduled tasks: {ex.Message}")
        End Try
    End Sub
    
    Public Sub Dispose()
        _timer.Stop()
        _timer.Dispose()
    End Sub
End Class