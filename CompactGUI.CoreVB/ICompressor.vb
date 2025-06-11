Public Interface ICompressor : Inherits IDisposable
    Function RunAsync(filesList As List(Of String), Optional progressMonitor As IProgress(Of CompressionProgress) = Nothing, Optional maxParallelism As Integer = 1) As Task(Of Boolean)
    Sub Pause()
    Sub [Resume]()
    Sub Cancel()

End Interface
