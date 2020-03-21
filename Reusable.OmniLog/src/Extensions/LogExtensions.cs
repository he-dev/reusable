using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public static class LogExtensions
    {
        #region Log properties

        public static ILogEntry Logger(this ILogEntry logEntry, string value) => logEntry.Add(Names.Default.Logger, value, m => m.ProcessWith<EchoNode>());

        public static ILogEntry Timestamp(this ILogEntry logEntry, DateTime value) => logEntry.Add(Names.Default.Timestamp, value, m => m.ProcessWith<EchoNode>());

        public static ILogEntry Level(this ILogEntry logEntry, LogLevel value) => logEntry.Add(Names.Default.Level, value, m => m.ProcessWith<EchoNode>());

        public static ILogEntry Exception(this ILogEntry logEntry, Exception? value)
        {
            if (value is {})
            {
                logEntry.Add(Names.Default.Exception, value, LogProperty.Process.With<EchoNode>());
                logEntry.Level(LogLevel.Error);
            }

            return logEntry;
        }

        public static ILogEntry Message(this ILogEntry logEntry, string value) => logEntry.Add(Names.Default.Message, value, m => m.ProcessWith<EchoNode>());

        public static ILogEntry Snapshot(this ILogEntry logEntry, object value, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            return logEntry.Add(Names.Default.Snapshot, value, buildMeta);
        }

        #endregion
    }
}