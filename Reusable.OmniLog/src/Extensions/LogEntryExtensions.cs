using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    public static class LogEntryExtensions
    {
        public static ILogEntry Logger(this ILogEntry logEntry, string value) => logEntry.Push(Names.Properties.Logger, value, m => m.ProcessWith<Echo>());

        public static ILogEntry Timestamp(this ILogEntry logEntry, DateTime value) => logEntry.Push(Names.Properties.Timestamp, value, m => m.ProcessWith<Echo>());

        public static ILogEntry Level(this ILogEntry logEntry, LogLevel value) => logEntry.Push(Names.Properties.Level, value, m => m.ProcessWith<Echo>());

        public static ILogEntry Snapshot(this ILogEntry logEntry, object value, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            return logEntry.Push(Names.Properties.Snapshot, value, buildMeta);
        }

        public static ILogEntry Snapshot(this ILogEntry logEntry, object value)
        {
            return logEntry.Push(Names.Properties.Snapshot, value, m => m.ProcessWith<SerializeProperty>());
        }

        public static ILogEntry Exception(this ILogEntry logEntry, Exception? value, LogLevel level = LogLevel.Error)
        {
            if (value is {})
            {
                logEntry.Push(Names.Properties.Exception, value, LogProperty.Process.With<Echo>());
                logEntry.Level(level);
            }

            return logEntry;
        }

        public static ILogEntry Message(this ILogEntry logEntry, string? value)
        {
            return
                value is {}
                    ? logEntry.Push(Names.Properties.Message, value, m => m.ProcessWith<Echo>())
                    : logEntry;
        }

        public static ILogEntry Layer(this ILogEntry log, string name)
        {
            return log.Pipe(x => x.Push(new LogProperty(Names.Properties.Layer, name, LogPropertyMeta.Builder.ProcessWith<Echo>())));
        }

        public static ILogEntry Category(this ILogEntry log, string name)
        {
            return log.Pipe(x => x.Push(new LogProperty(Names.Properties.Category, name, LogPropertyMeta.Builder.ProcessWith<Echo>())));
        }

        public static ILogEntry Unit(this ILogEntry log, string name, object? value = default)
        {
            return log.Pipe(x =>
            {
                x.Push(new LogProperty(Names.Properties.Unit, name, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                if (value is {})
                {
                    x.Push(new LogProperty(Names.Properties.Snapshot, value, LogPropertyMeta.Builder.ProcessWith<SerializeProperty>()));
                }
            });
        }

        public static ILogEntry CallSite(this ILogEntry log, Data.CallSite? callSite)
        {
            return log.Pipe(x =>
            {
                if (callSite is {})
                {
                    x.Push(new LogProperty(Names.Properties.CallerMemberName, callSite.CallerMemberName, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                    x.Push(new LogProperty(Names.Properties.CallerLineNumber, callSite.CallerLineNumber, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                    x.Push(new LogProperty(Names.Properties.CallerFilePath, callSite.CallerFilePath, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                }
            });
        }
    }
}