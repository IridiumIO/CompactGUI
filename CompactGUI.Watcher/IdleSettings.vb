Public Class IdleSettings
    Public Property IdleCheckIntervalSeconds As Integer = 5 ' How often to check for idle state
    Public Property IdleThresholdSeconds As Integer = 120 ' Minimum seconds of inactivity to be considered idle
    Public Property IdleRepeatTimeSeconds As Integer = 60 ' How often to repeat firing the idle event after the initial idle state is detected
    Public Property LastSystemModifiedTimeThresholdSeconds As Integer = 300 ' How long to wait after the last system modification before considering a folder for analysis / compaction
End Class
