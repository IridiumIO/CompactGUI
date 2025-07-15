Imports CompactGUI.Core.Settings
Imports CompactGUI.Logging
Imports CompactGUI.Watcher

Imports Coravel.Scheduling.Schedule
Imports Coravel.Scheduling.Schedule.Interfaces

Imports Microsoft.Extensions.Logging

Public Class SchedulerService
    Private ReadOnly settings As Settings
    Private ReadOnly idleDetector As IdleDetector
    Private ReadOnly logger As ILogger(Of SchedulerService)

    Public Sub New(settingsService As ISettingsService, idleDetector As IdleDetector, logger As ILogger(Of SchedulerService))
        Me.settings = settingsService.AppSettings
        Me.idleDetector = idleDetector
        Me.logger = logger

    End Sub

    Friend Sub RegenerateSchedule()
        Dim scheduler = CType(Application.GetService(Of IScheduler), Scheduler)
        If Not scheduler.TryUnschedule(NameOf(Watcher)) Then Return

        scheduler.ScheduleAsync(Async Function() Await Application.GetService(Of Watcher.Watcher).RunWatcher()).
                                Cron($"{settings.ScheduledBackgroundMinute} {settings.ScheduledBackgroundHour} * * *").Zoned(TimeZoneInfo.Local).
                                When(Function() Task.FromResult(IsSchedulerRunnable)).
                                PreventOverlapping(NameOf(Watcher))

    End Sub

    Public Function IsSchedulerRunnable() As Boolean

        SchedulerServiceLog.CheckingSchedulerRunnable(logger)

        If Not settings.EnableBackgroundWatcher Then
            SchedulerServiceLog.SchedulerDisabled(logger)
            Return False
        End If

        If settings.NextScheduledBackgroundRun.Date > Date.Now.Date Then
            SchedulerServiceLog.SchedulerNextRunInFuture(logger, settings.NextScheduledBackgroundRun)
            Return False
        End If

        Select Case settings.BackgroundModeSelection
            Case BackgroundMode.ScheduledAndIdle
                If idleDetector.State = IdleState.Idle Then
                    SchedulerServiceLog.SchedulerRunningIdle(logger)
                    Return True
                Else
                    SchedulerServiceLog.SchedulerNotIdle(logger)
                    Return False
                End If
            Case BackgroundMode.Scheduled
                SchedulerServiceLog.SchedulerRunningScheduled(logger)
                Return True
            Case Else
                SchedulerServiceLog.SchedulerModeDisabled(logger)
                Return False
        End Select

    End Function

End Class
