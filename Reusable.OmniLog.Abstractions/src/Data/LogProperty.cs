using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.OmniLog.Abstractions.Data
{
    [PublicAPI]
    public readonly struct LogProperty
    {
        public LogProperty(SoftString name, object? value, LogPropertyMeta meta)
        {
            Name = name;
            Value = value;
            Meta = meta;
        }

        public SoftString Name { get; }

        public object? Value { get; }

        public LogPropertyMeta Meta { get; }
    }

    public static class LogPropertyExtensions
    {
        public static T ValueOrDefault<T>(this LogProperty? property, T defaultValue = default)
        {
            return property?.Value switch { T t => t, _ => defaultValue };
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