Imports System.Collections.ObjectModel
Imports System.Threading
Imports CommunityToolkit.Mvvm.ComponentModel

Public Class CompressionScheduler : Inherits ObservableObject
    
    Public Property ScheduledTasks As New ObservableCollection(Of ScheduledCompressionTask)
    Private ReadOnly _timer As Timer
    
    Public Sub New()
        _timer = New Timer(AddressOf CheckScheduledTasks, Nothing, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
    End Sub
    
    Public Sub AddScheduledTask(task As ScheduledCompressionTask)
        ScheduledTasks.Add(task)
    End Sub
    
    Private Sub CheckScheduledTasks(state As Object)
        Dim now = DateTime.Now
        Dim tasksToRun = ScheduledTasks.Where(Function(t) t.ShouldRun(now) AndAlso t.IsEnabled).ToList()
        
        For Each task In tasksToRun
            Task.Run(Async Function() Await ExecuteScheduledTask(task))
        Next
    End Sub
    
    Private Async Function ExecuteScheduledTask(task As ScheduledCompressionTask) As Task
        Try
            task.IsRunning = True
            task.LastRun = DateTime.Now
            
            ' Execute compression based on task type
            Select Case task.TaskType
                Case ScheduledTaskType.CompressFolder
                    Await CompressFolderTask(task)
                Case ScheduledTaskType.CleanupTemp
                    Await CleanupTempTask(task)
                Case ScheduledTaskType.OptimizeGames
                    Await OptimizeGamesTask(task)
            End Select
            
        Finally
            task.IsRunning = False
        End Try
    End Function
    
    Private Async Function CompressFolderTask(task As ScheduledCompressionTask) As Task
        Dim folder = CompressableFolderFactory.CreateCompressableFolder(task.TargetPath)
        folder.CompressionOptions.SelectedCompressionMode = task.CompressionMode
        Await folder.CompressFolder()
    End Function
    
    Private Async Function CleanupTempTask(task As ScheduledCompressionTask) As Task
        ' Clean up temporary files and compress them
        Dim tempFolders = {
            Environment.GetFolderPath(Environment.SpecialFolder.InternetCache),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
            Path.GetTempPath()
        }
        
        For Each tempFolder In tempFolders
            If Directory.Exists(tempFolder) Then
                ' Clean old files and compress remaining
                Await CleanAndCompressFolder(tempFolder, task.Settings)
            End If
        Next
    End Function
    
    Private Async Function OptimizeGamesTask(task As ScheduledCompressionTask) As Task
        ' Find and optimize game installations
        Dim steamFolders = FindSteamLibraries()
        For Each steamFolder In steamFolders
            Await OptimizeSteamLibrary(steamFolder, task.Settings)
        Next
    End Function
    
    Private Function FindSteamLibraries() As List(Of String)
        ' Implementation to find Steam library folders
        Return New List(Of String)
    End Function
    
    Private Async Function CleanAndCompressFolder(folderPath As String, settings As Dictionary(Of String, Object)) As Task
        ' Implementation for cleaning and compressing folders
    End Function
    
    Private Async Function OptimizeSteamLibrary(libraryPath As String, settings As Dictionary(Of String, Object)) As Task
        ' Implementation for optimizing Steam libraries
    End Function
    
End Class

Public Class ScheduledCompressionTask : Inherits ObservableObject
    
    Public Property Name As String
    Public Property TaskType As ScheduledTaskType
    Public Property TargetPath As String
    Public Property CompressionMode As Core.CompressionMode
    Public Property Schedule As CompressionSchedule
    Public Property IsEnabled As Boolean = True
    Public Property IsRunning As Boolean = False
    Public Property LastRun As DateTime = DateTime.MinValue
    Public Property Settings As New Dictionary(Of String, Object)
    
    Public Function ShouldRun(currentTime As DateTime) As Boolean
        If IsRunning Then Return False
        
        Select Case Schedule.Type
            Case ScheduleType.Daily
                Return currentTime.Date > LastRun.Date AndAlso 
                       currentTime.TimeOfDay >= Schedule.Time.TimeOfDay
            Case ScheduleType.Weekly
                Return currentTime.Date > LastRun.Date.AddDays(7) AndAlso
                       currentTime.DayOfWeek = Schedule.DayOfWeek AndAlso
                       currentTime.TimeOfDay >= Schedule.Time.TimeOfDay
            Case ScheduleType.OnSystemIdle
                Return currentTime > LastRun.AddHours(1) ' Minimum 1 hour between runs
            Case Else
                Return False
        End Select
    End Function
    
End Class

Public Class CompressionSchedule
    Public Property Type As ScheduleType
    Public Property Time As DateTime
    Public Property DayOfWeek As DayOfWeek
End Class

Public Enum ScheduledTaskType
    CompressFolder
    CleanupTemp
    OptimizeGames
End Enum

Public Enum ScheduleType
    Daily
    Weekly
    OnSystemIdle
End Enum