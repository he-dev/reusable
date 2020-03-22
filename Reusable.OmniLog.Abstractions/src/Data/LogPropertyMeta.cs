using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
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

            public LogPropertyMetaBuilder LogWith<T>() where T : IConnector
            {
                _meta.Loggers.Add(typeof(T));
                return this;
            }

            public LogPropertyMetaBuilder LogWith<T>(T rx) where T : IConnector
            {
                return LogWith<T>();
            }

            public LogPropertyMeta Build() => _meta;

            public static implicit operator LogPropertyMeta(LogPropertyMetaBuilder builder) => builder.Build();
        }
    }
}