using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Reusable.Extensions;
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
        
        public static ILogEntry Snapshot(this ILogEntry logEntry, object value) => logEntry.Push(new SerializableProperty.Snapshot(value));

        public static ILogEntry Exception(this ILogEntry logEntry, Exception? value, LogLevel level = LogLevel.Error)
        {
            if (value is { })
            {
                logEntry.Push(new LoggableProperty.Exception(value));
                logEntry.Push(new LoggableProperty.Level(value));
            }

            return logEntry;
        }

        public static ILogEntry Message(this ILogEntry logEntry, string? value)
        {
            return
                value is { }
                    ? logEntry.Push(new LoggableProperty.Message(value))
                    : logEntry;
        }

        public static ILogEntry Layer(this ILogEntry log, string name)
        {
            return log.Also(x => x.Push(new LoggableProperty.Layer(name)));
        }

        public static ILogEntry Category(this ILogEntry log, string name)
        {
            return log.Also(x => x.Push(new LoggableProperty.Category(name)));
        }

        public static ILogEntry Unit(this ILogEntry log, string name, object? value = default)
        {
            return log.Also(x =>
            {
                x.Push(new LoggableProperty.Unit(name));
                if (value is { })
                {
                    x.Push(new SerializableProperty.Snapshot(value));
                }
            });
        }

        public static ILogEntry Caller(this ILogEntry log, LogCaller? caller)
        {
            return log.Also(x =>
            {
                if (caller is { })
                {
                    x.Push(new LoggableProperty.CallerMemberName(caller.MemberName));
                    x.Push(new LoggableProperty.CallerLineNumber(caller.LineNumber));
                    x.Push(new LoggableProperty.CallerFilePath(caller.FilePath));
                }
            });
        }
        
        public static ILogEntry OverrideBuffer(this ILogEntry logEntry) => logEntry.Push(new MetaProperty.OverrideBuffer());

        public static IEnumerable<ILogProperty> Where<T>(this ILogEntry entry) where T : ILogProperty
        {
            return entry.Where(property => property is T);
        }
        
        public static bool TryGetProperty<T>(this ILogEntry entry, string name, out T result) where T : ILogProperty
        {
            if (entry.TryGetProperty(name, out var property) && property is T casted)
            {
                result = casted;
                return true;
            }
            else
            {
                result = default!;
                return false;
            }
        }

        public static bool TryGetProperty<T>(this ILogEntry entry, out ILogProperty property) where T : ILogProperty
        {
            return entry.TryGetProperty(typeof(T).Name, out property);
        }
        
        public static bool TryGetProperty<TProperty, TValue>(this ILogEntry entry, out TValue result) where TProperty : ILogProperty
        {
            if(entry.TryGetProperty(typeof(TProperty).Name, out var property))
            {
                if (property?.Value is TValue value)
                {
                    result = value;
                    return true;
                }
            }

            result = default!;
            return false;
        }

        public static ILogEntry Merge(this ILogEntry entry, ILogEntry other)
        {
            foreach (var property in other)
            {
                entry.Push(property);
            }

            return entry;
        }

        public static DataTable ToDataTable(this IEnumerable<ILogEntry> entries, Action<DataRow>? dataRowAction = default)
        {
            var dataTable = new DataTable();
            foreach (var entry in entries)
            {
                var dataRow = dataTable.NewRow();
                foreach (var item in entry.Where<LoggableProperty>())
                {
                    Add(dataTable.Columns, item.Name, item.Value.GetType());
                    dataRow[item.Name] = item.Value;
                }

                dataTable.Rows.Add(dataRow.Also(dataRowAction));
            }

            return dataTable;
        }
        
        // Adds data-column it if does not exists.
        private static DataColumnCollection Add(this DataColumnCollection columns, string name, Type dataType)
        {
            return columns.Also(c =>
            {
                if (!c.Contains(name))
                {
                    c.Add(name, dataType);
                }
            });
        }
    }
}