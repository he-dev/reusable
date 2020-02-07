using System;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public static class LogExtensions
    {
        #region Log properties

        public static LogEntry Logger(this LogEntry logEntry, string value) => logEntry.Add(LogEntry.Names.Logger, value, m => m.ProcessWith<EchoNode>());

        public static LogEntry Timestamp(this LogEntry logEntry, DateTime value) => logEntry.Add(LogEntry.Names.Timestamp, value, m => m.ProcessWith<EchoNode>());

        public static LogEntry Level(this LogEntry logEntry, Option<LogLevel> value) => logEntry.Add(LogEntry.Names.Level, value, m => m.ProcessWith<EchoNode>());

        public static LogEntry Exception(this LogEntry logEntry, Exception? value)
        {
            logEntry.Add(LogEntry.Names.Exception, value, m => m.ProcessWith<EchoNode>());
            if (value is {})
            {
                logEntry.Level(LogLevel.Error);
            }

            return logEntry;
        }

        public static LogEntry Message(this LogEntry logEntry, string value) => logEntry.Add(LogEntry.Names.Message, value, m => m.ProcessWith<EchoNode>());

        public static LogEntry Snapshot(this LogEntry logEntry, object value, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            return logEntry.Add(LogEntry.Names.Snapshot, value, buildMeta);
        }

        #endregion
    }
}