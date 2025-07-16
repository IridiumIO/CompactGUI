using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactGUI.Logging
{
    public static partial class SchedulerServiceLog
    {

        [LoggerMessage(Level = LogLevel.Debug, Message = "Checking if scheduler should run...")]
        public static partial void CheckingSchedulerRunnable(ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Scheduler not running: background watcher is disabled.")]
        public static partial void SchedulerDisabled(ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Scheduler not running: next scheduled run is in the future. Next run: {NextRun}")]
        public static partial void SchedulerNextRunInFuture(ILogger logger, DateTime nextRun);

        [LoggerMessage(Level = LogLevel.Information, Message = "Scheduler running: system is idle.")]
        public static partial void SchedulerRunningIdle(ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Scheduler not running: system is not idle.")]
        public static partial void SchedulerNotIdle(ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Scheduler running: scheduled mode, idle not required.")]
        public static partial void SchedulerRunningScheduled(ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Scheduler not running: scheduler is disabled.")]
        public static partial void SchedulerModeDisabled(ILogger logger);

        [LoggerMessage(Level = LogLevel.Error, Message = "Scheduler error: {ExceptionMessage}")]
        public static partial void SchedulerError(ILogger logger, string exceptionMessage, Exception ex);

    }
}
