Imports System.Collections.ObjectModel
Imports System.Runtime
Imports System.Threading

Public Class BackgroundCompactor

    Public Property isCompactorActive As Boolean = False

    Private cancellationTokenSource As New CancellationTokenSource()
    Private isCompacting As Boolean = False
    Private isCompactingPaused As Boolean = False ' Track if compacting is paused

    Private isSystemIdle As Boolean = False

    Private _compactor As Core.Compactor

    Private _excludedFileTypes As String()

    Public Sub New(excludedFileTypes As String())

        _excludedFileTypes = excludedFileTypes

        AddHandler IdleDetector.IsIdle, AddressOf OnSystemIdle
        AddHandler IdleDetector.IsNotIdle, AddressOf OnSystemNotIdle

    End Sub

    Private Sub OnSystemIdle(sender As Object, e As EventArgs)
        Debug.WriteLine("  SYSTEM IDLE!")
        isSystemIdle = True
        ' Attempt to resume only if compacting was paused due to system activity
        If isCompactingPaused AndAlso Not isCompacting Then
            ResumeCompacting()
        End If
    End Sub

    Private Sub OnSystemNotIdle(sender As Object, e As EventArgs)
        Debug.WriteLine("  SYSTEM NOT IDLE!")
        isSystemIdle = False
        ' Attempt to pause only if compacting is currently active and not already paused
        If isCompacting AndAlso Not isCompactingPaused Then
            PauseCompacting()
        End If
    End Sub

    Public Function BeginCompacting(folder As String, compressionLevel As Core.CompressionAlgorithm) As Task(Of Boolean)

        If compressionLevel = Core.CompressionAlgorithm.NO_COMPRESSION Then Return Task(Of Boolean).FromResult(False)

        _compactor = New Core.Compactor(folder, compressionLevel, _excludedFileTypes)

        Return _compactor.RunCompactAsync()

    End Function

    Public Async Function StartCompactingAsync(folders As ObservableCollection(Of WatchedFolder)) As Task(Of Boolean)
        Debug.WriteLine("Background Compacting Started")
        Dim cancellationToken As CancellationToken = cancellationTokenSource.Token

        isCompactorActive = True

        Dim currentProcess As Process = Process.GetCurrentProcess()
        currentProcess.PriorityClass = ProcessPriorityClass.Idle

        Dim foldersCopy As List(Of WatchedFolder) = folders.ToList()

        For Each folder In foldersCopy
            If folder.DecayPercentage = 0 Then
                Continue For
            End If
            Debug.WriteLine("    Compacting " & folder.DisplayName)
            Dim compactingTask = BeginCompacting(folder.Folder, folder.CompressionLevel)
            isCompacting = True

            While Not cancellationToken.IsCancellationRequested
                Dim completedTask = Await Task.WhenAny(compactingTask, Task.Delay(1000, cancellationToken))
                If completedTask Is compactingTask Then
                    Exit While
                End If

                ' Check the idle state and adjust compacting status accordingly
                If Not isSystemIdle AndAlso Not isCompactingPaused Then
                    PauseCompacting()
                ElseIf isSystemIdle AndAlso isCompactingPaused Then
                    ResumeCompacting()
                End If
            End While

            Dim result = Await compactingTask
            If result Then
                ' Ensure the folder is still in the original collection before updating
                If folders.Contains(folder) Then
                    folder.LastCompressedDate = DateTime.Now
                End If
            End If
            Debug.WriteLine("    Finished Compacting " & folder.DisplayName)
        Next

        isCompactorActive = False
        isCompacting = False ' Ensure compacting status is reset after operation
        Debug.WriteLine("Background Compacting Finished")
        currentProcess.PriorityClass = ProcessPriorityClass.Normal
        Return True
    End Function

    Public Sub PauseCompacting()
        Debug.WriteLine(" - Pausing Background!")
        isCompactingPaused = True ' Indicate compacting is paused
        _compactor.PauseCompression()
    End Sub

    Public Sub ResumeCompacting()
        Debug.WriteLine(" - Resuming Background!")
        isCompactingPaused = False ' Indicate compacting is no longer paused
        _compactor.ResumeCompression()
    End Sub

End Class
