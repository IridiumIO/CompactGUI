Imports System.ComponentModel
Imports CommunityToolkit.Mvvm.ComponentModel

Public Class ScheduledTask : Inherits ObservableObject
    Public Property Id As Guid = Guid.NewGuid()
    Public Property Name As String
    Public Property FolderPath As String
    Public Property CompressionMode As Integer = 0
    Public Property SkipNonCompressable As Boolean = False
    Public Property SkipUserNonCompressable As Boolean = False
    Public Property IsRecurring As Boolean = False
    Public Property RecurrencePattern As RecurrencePattern = RecurrencePattern.Daily
    Public Property RecurrenceInterval As Integer = 1
    Public Property ScheduledTime As DateTime
    Public Property LastRunTime As DateTime?
    Public Property IsEnabled As Boolean = True
    Public Property CreatedAt As DateTime = DateTime.Now
    
    Public ReadOnly Property NextRunTime As DateTime
        Get
            If Not IsRecurring OrElse LastRunTime Is Nothing Then
                Return ScheduledTime
            End If
            
            Select Case RecurrencePattern
                Case RecurrencePattern.Daily
                    Return LastRunTime.Value.AddDays(RecurrenceInterval)
                Case RecurrencePattern.Weekly
                    Return LastRunTime.Value.AddDays(RecurrenceInterval * 7)
                Case RecurrencePattern.Monthly
                    Return LastRunTime.Value.AddMonths(RecurrenceInterval)
                Case Else
                    Return ScheduledTime
            End Select
        End Get
    End Property
    
    Public ReadOnly Property IsOverdue As Boolean
        Get
            Return IsEnabled AndAlso NextRunTime < DateTime.Now
        End Get
    End Property
    
    Public ReadOnly Property DisplayName As String
        Get
            If String.IsNullOrEmpty(Name) Then
                Return System.IO.Path.GetFileName(FolderPath)
            End If
            Return Name
        End Get
    End Property
End Class

Public Enum RecurrencePattern
    Daily
    Weekly
    Monthly
End Enum