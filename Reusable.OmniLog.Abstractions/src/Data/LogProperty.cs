using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.OmniLog.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public readonly struct LogProperty
    {
        public LogProperty(string name, object value, LogPropertyMeta meta)
        {
            Name = name;
            Value = value;
            Meta = meta;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(b => b.DisplaySingle(x => x.Name).DisplaySingle(x => x.Value));

        public string Name { get; }

        public object Value { get; }

        public LogPropertyMeta Meta { get; }

        public static class CanProcess
        {
            public static Func<LogProperty, bool> With<T>(T? node = default) where T : class, ILoggerNode
            {
                return property => property.Meta.Processors.Count == 0 || property.Meta.Processors.Contains(typeof(T));
            }
        }

        public static class CanLog
        {
            public static Func<LogProperty, bool> With<T>() where T : IConnector
            {
                return property => property.Meta.Loggers.Count == 0 || property.Meta.Loggers.Contains(typeof(T));
            }
        }

        public static class Process
        {
            public static Action<LogPropertyMeta.LogPropertyMetaBuilder> With<T>() where T : ILoggerNode
            {
                return m => m.ProcessWith<T>();
            }
        }
    }
    
    public abstract class Names
    {
        public abstract class Properties
        {
            public const string Timestamp = nameof(Timestamp);
            public const string Correlation = nameof(Correlation);
            public const string Layer = nameof(Layer);
            public const string Logger = nameof(Logger);
            public const string Level = nameof(Level);
            public const string Category = nameof(Category);
            public const string SnapshotName = nameof(SnapshotName);
            public const string Snapshot = nameof(Snapshot);
            public const string Elapsed = nameof(Elapsed);
            public const string Message = nameof(Message);
            public const string Exception = nameof(Exception);
            public const string CallerMemberName = nameof(CallerMemberName);
            public const string CallerLineNumber = nameof(CallerLineNumber);
            public const string CallerFilePath = nameof(CallerFilePath);
        }

        public abstract class Categories
        {
            public const string WorkItem = nameof(WorkItem);
            public const string Routine = nameof(Routine);
        }
    }

    public static class LogPropertyExtensions
    {
        public static T ValueOrDefault<T>(this LogProperty? property, T defaultValue = default)
        {
            return property?.Value switch { T t => t, _ => defaultValue };
        }

        public static bool CanProcessWith<T>(this LogProperty property) where T : ILoggerNode
        {
            return property.Meta.Processors.Contains(typeof(T));
        }

        public static bool CanLogWith<T>(this LogProperty property) where T : ILoggerNode
        {
            return property.Meta.Loggers.Contains(typeof(T));
        }
    }
}