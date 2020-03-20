using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public readonly struct LogProperty
    {
        public LogProperty(string name, object? value, LogPropertyMeta meta)
        {
            Name = name;
            Value = value;
            Meta = meta;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(b => b.DisplaySingle(x => x.Name).DisplaySingle(x => x.Value));

        public string Name { get; }

        public object? Value { get; }

        public LogPropertyMeta Meta { get; }

        public static class CanProcess
        {
            public static Func<LogProperty, bool> With<T>(T node = default) where T : ILoggerNode
            {
                return property => property.Meta.Processors.Count == 0 || property.Meta.Processors.Contains(typeof(T));
            }
        }
        
        public static class CanLog
        {
            public static Func<LogProperty, bool> With<T>() where T : ILogRx
            {
                return property => property.Meta.Loggers.Count == 0 || property.Meta.Loggers.Contains(typeof(T));
            }
        }
        
        public static class ValueIs
        {
            public static Func<LogProperty, bool> NotNull() => property => property.Value is {};
        }
        
        [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
        public static class Names
        {
            public static readonly string Timestamp = nameof(Timestamp)!;
            public static readonly string Logger = nameof(Logger)!;
            public static readonly string Level = nameof(Level)!;
            public static readonly string Message = nameof(Message)!;
            public static readonly string Exception = nameof(Exception)!;
            public static readonly string CallerMemberName = nameof(CallerMemberName)!;
            public static readonly string CallerLineNumber = nameof(CallerLineNumber)!;
            public static readonly string CallerFilePath = nameof(CallerFilePath)!;

            public static readonly string SnapshotName = nameof(SnapshotName)!;
            public static readonly string Snapshot = nameof(Snapshot)!;

            public static readonly string Correlation = nameof(Correlation)!;
            public static readonly string Elapsed = nameof(Stopwatch.Elapsed)!;
        }
        
        public static class Process
        {
            public static Action<LogPropertyMeta.LogPropertyMetaBuilder> With<T>() where T : ILoggerNode
            {
                return m => m.ProcessWith<T>();
            }
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

    public class LogPropertyMeta
    {
        public ISet<Type> Processors { get; } = new HashSet<Type>();

        public ISet<Type> Loggers { get; } = new HashSet<Type>();

        public bool Contains(LogPropertyMeta other)
        {
            var processorsOverlapOrEmpty = Processors.Count == 0 || other.Processors.Count == 0 || other.Processors.Overlaps(Processors);
            var loggersOverlapOrEmpty = Loggers.Count == 0 || other.Loggers.Count == 0 || other.Loggers.Overlaps(Loggers);
            return processorsOverlapOrEmpty && loggersOverlapOrEmpty;
        }

        public static LogPropertyMetaBuilder Builder => new LogPropertyMetaBuilder();

        public static LogPropertyMeta From(Action<LogPropertyMetaBuilder> build)
        {
            return Builder.Pipe(build);
        }

        public class LogPropertyMetaBuilder
        {
            private readonly LogPropertyMeta _meta = new LogPropertyMeta();

            public LogPropertyMetaBuilder ProcessWith<T>() where T : ILoggerNode
            {
                _meta.Processors.Add(typeof(T));
                return this;
            }

            public LogPropertyMetaBuilder ProcessWith<T>(T node) where T : ILoggerNode
            {
                return ProcessWith<T>();
            }

            public LogPropertyMetaBuilder LogWith<T>() where T : ILogRx
            {
                _meta.Loggers.Add(typeof(T));
                return this;
            }
            
            public LogPropertyMetaBuilder LogWith<T>(T rx) where T : ILogRx
            {
                return LogWith<T>();
            }

            public LogPropertyMeta Build() => _meta;

            public static implicit operator LogPropertyMeta(LogPropertyMetaBuilder builder) => builder.Build();
        }
    }
}