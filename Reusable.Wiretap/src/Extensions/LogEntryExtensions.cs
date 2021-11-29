using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Nodes;

namespace Reusable.Wiretap.Extensions
{
    public static class LogEntryExtensions
    {
        public static ILogEntry Logger(this ILogEntry logEntry, string value) => logEntry.Push(new LoggableProperty.Logger(value));

        public static ILogEntry Timestamp(this ILogEntry logEntry, DateTime value) => logEntry.Push(new LoggableProperty.Timestamp(value));

        public static ILogEntry Level(this ILogEntry logEntry, LogLevel value) => logEntry.Push(new LoggableProperty.Level(value));

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
                logEntry.Push(new LoggableProperty.Exception(value));
                logEntry.Push(new LoggableProperty.Level(value));
            }

            return logEntry;
        }

        public static ILogEntry Message(this ILogEntry logEntry, string? value)
        {
            return
                value is {}
                    ? logEntry.Push(new LoggableProperty.Message(value))
                    : logEntry;
        }

        public static ILogEntry Layer(this ILogEntry log, string name)
        {
            return log.Also(x => x.Push(new LogProperty(Names.Properties.Layer, name, LogPropertyMeta.Builder.ProcessWith<Echo>())));
        }

        public static ILogEntry Category(this ILogEntry log, string name)
        {
            return log.Also(x => x.Push(new LogProperty(Names.Properties.Category, name, LogPropertyMeta.Builder.ProcessWith<Echo>())));
        }

        public static ILogEntry Unit(this ILogEntry log, string name, object? value = default)
        {
            return log.Also(x =>
            {
                x.Push(new LogProperty(Names.Properties.Unit, name, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                if (value is {})
                {
                    x.Push(new LogProperty(Names.Properties.Snapshot, value, LogPropertyMeta.Builder.ProcessWith<SerializeProperty>()));
                }
            });
        }

        public static ILogEntry CallSite(this ILogEntry log, Caller? callSite)
        {
            return log.Also(x =>
            {
                if (callSite is {})
                {
                    x.Push(new LogProperty(Names.Properties.CallerMemberName, callSite.MemberName, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                    x.Push(new LogProperty(Names.Properties.CallerLineNumber, callSite.LineNumber, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                    x.Push(new LogProperty(Names.Properties.CallerFilePath, callSite.FilePath, LogPropertyMeta.Builder.ProcessWith<Echo>()));
                }
            });
        }
        
        public static ILogEntry Priority(this ILogEntry logEntry, LogEntryPriority priority) => logEntry.Push(Names.Properties.Priority, priority, m => m.ProcessWith<Echo>());
        
        public static ILogEntry Important(this ILogEntry logEntry) => logEntry.Priority(LogEntryPriority.High);

        public static IEnumerable<ILogProperty> Where<T>(this ILogEntry entry) where T : ILogProperty
        {
            return entry.Where(property => property is T);
        }
    }
}