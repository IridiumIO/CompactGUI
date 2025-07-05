Imports CommunityToolkit.Mvvm.ComponentModel

Public Class BatchProcessingJob : Inherits ObservableObject
    Public Property Id As Guid = Guid.NewGuid()
    Public Property Name As String
    Public Property FolderPaths As New List(Of String)
    Public Property CompressionProfileId As Guid?
    Public Property CreatedAt As DateTime = DateTime.Now
    Public Property Status As BatchJobStatus = BatchJobStatus.Pending
    Public Property Progress As Double = 0
    Public Property CurrentFolder As String
    Public Property CompletedFolders As New List(Of String)
    Public Property FailedFolders As New Dictionary(Of String, String) ' Path, Error message
    
    Public ReadOnly Property TotalFolders As Integer
        Get
            Return FolderPaths.Count
        End Get
    End Property
    
    Public ReadOnly Property CompletedCount As Integer
        Get
            Return CompletedFolders.Count
        End Get
    End Property
    
    Public ReadOnly Property FailedCount As Integer
        Get
            Return FailedFolders.Count
        End Get
    End Property
    
    Public ReadOnly Property RemainingCount As Integer
        Get
            Return TotalFolders - CompletedCount - FailedCount
        End Get
    End Property
    
    Public ReadOnly Property IsComplete As Boolean
        Get
            Return Status = BatchJobStatus.Completed OrElse Status = BatchJobStatus.Failed OrElse Status = BatchJobStatus.Cancelled
        End Get
    End Property
    
    Public ReadOnly Property DisplayName As String
        Get
            If String.IsNullOrEmpty(Name) Then
                Return $"Batch Job {Id.ToString().Substring(0, 8)}"
            End If
            Return Name
        End Get
    End Property
End Class

Public Enum BatchJobStatus
    Pending
    Running
    Paused
    Completed
    Failed
    Cancelled
End Enum