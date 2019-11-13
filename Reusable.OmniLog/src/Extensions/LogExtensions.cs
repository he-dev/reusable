using System;
using Reusable.Data;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog
{
    public static class LogExtensions
    {
        #region Log properties

        public static LogEntry Logger(this LogEntry logEntry, string value) => logEntry.Add<Log>(nameof(Logger), value);

        public static LogEntry Timestamp(this LogEntry logEntry, DateTime value) => logEntry.Add<Log>(nameof(Timestamp), value);

        public static LogEntry Level(this LogEntry logEntry, Option<LogLevel> value) => logEntry.Add<Log>(nameof(Level), value);

        public static LogEntry Exception(this LogEntry logEntry, Exception value) => logEntry.Add<Log>(nameof(Exception), value);

        public static LogEntry Message(this LogEntry logEntry, string value) => logEntry.Add<Log>(nameof(Message), value);

        public static LogEntry Snapshot<T>(this LogEntry logEntry, object value) where T : struct, ILogPropertyAction => logEntry.Add<T>(nameof(Snapshot), value);

        #endregion
    }
}