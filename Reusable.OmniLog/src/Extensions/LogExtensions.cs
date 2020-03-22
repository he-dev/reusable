using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    public static class LogExtensions
    {
        #region Log properties

        public static ILogEntry Logger(this ILogEntry logEntry, string value) => logEntry.Push(Names.Properties.Logger, value, m => m.ProcessWith<Echo>());

        public static ILogEntry Timestamp(this ILogEntry logEntry, DateTime value) => logEntry.Push(Names.Properties.Timestamp, value, m => m.ProcessWith<Echo>());

        public static ILogEntry Level(this ILogEntry logEntry, LogLevel value) => logEntry.Push(Names.Properties.Level, value, m => m.ProcessWith<Echo>());

        public static ILogEntry Exception(this ILogEntry logEntry, Exception? value)
        {
            if (value is {})
            {
                logEntry.Push(Names.Properties.Exception, value, LogProperty.Process.With<Echo>());
                logEntry.Level(LogLevel.Error);
            }

            return logEntry;
        }

        public static ILogEntry Message(this ILogEntry logEntry, string value) => logEntry.Push(Names.Properties.Message, value, m => m.ProcessWith<Echo>());

        public static ILogEntry Snapshot(this ILogEntry logEntry, object value, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            return logEntry.Push(Names.Properties.Snapshot, value, buildMeta);
        }

        #endregion
    }
}