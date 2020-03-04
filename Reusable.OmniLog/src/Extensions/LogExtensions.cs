using System;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public static class LogExtensions
    {
        #region Log properties

        public static ILogEntry Logger(this ILogEntry logEntry, string value) => logEntry.Add(LogProperty.Names.Logger, value, m => m.ProcessWith<EchoNode>());

        public static ILogEntry Timestamp(this ILogEntry logEntry, DateTime value) => logEntry.Add(LogProperty.Names.Timestamp, value, m => m.ProcessWith<EchoNode>());

        public static ILogEntry Level(this ILogEntry logEntry, LogLevel value) => logEntry.Add(LogProperty.Names.Level, value, m => m.ProcessWith<EchoNode>());

        public static ILogEntry Exception(this ILogEntry logEntry, Exception? value)
        {
            if (value is {})
            {
                logEntry.Add(LogProperty.Names.Exception, value, m => m.ProcessWith<EchoNode>());
                logEntry.Level(LogLevel.Error);
            }

            return logEntry;
        }

        public static ILogEntry Message(this ILogEntry logEntry, string value) => logEntry.Add(LogProperty.Names.Message, value, m => m.ProcessWith<EchoNode>());

        public static ILogEntry Snapshot(this ILogEntry logEntry, object value, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            return logEntry.Add(LogProperty.Names.Snapshot, value, buildMeta);
        }

        #endregion
    }
}