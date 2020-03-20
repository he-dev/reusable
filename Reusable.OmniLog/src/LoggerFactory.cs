using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Services;

namespace Reusable.OmniLog
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly IEnumerable<ILoggerNode> _nodes;

        private readonly ConcurrentDictionary<SoftString, ILogger> _loggers = new ConcurrentDictionary<SoftString, ILogger>();

        public LoggerFactory(IEnumerable<ILoggerNode> nodes) => _nodes = nodes.ToList();

        public static ILoggerFactory Empty() => new LoggerFactory(Enumerable.Empty<ILoggerNode>());

        public static LoggerFactoryBuilder Builder() => new LoggerFactoryBuilder();

        #region ILoggerFactory

        public ILogger CreateLogger(string name) => _loggers.GetOrAdd(name!, n => CreatePipeline(n.ToString()));

        private ILogger CreatePipeline(string loggerName)
        {
            var loggerNode = new ServiceNode { Services = { new Constant(nameof(Logger), loggerName) } };

            return (ILogger)new ILoggerNode[] { new Logger(), loggerNode }.Concat(this).Chain().First();
        }

        public void Dispose() { }

        #endregion

        public IEnumerator<ILoggerNode> GetEnumerator() => _nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_nodes).GetEnumerator();
    }

    public class LoggerFactoryBuilder : List<ILoggerNode>
    {
        public LoggerFactoryBuilder Use<T>(LoggerFactoryBuilder builder, Action<T>? configure = default) where T : ILoggerNode, new()
        {
            Add(new T().Pipe(configure));
            return this;
        }

        public ILoggerFactory Build()
        {
            return new LoggerFactory(this);
        }
    }

    public static class LoggerFactoryBuilderExtensions
    {
        public static ILogger<T> CreateLogger<T>(this ILoggerFactory loggerFactory)
        {
            return new Logger<T>(loggerFactory);
        }

        public static LoggerFactoryBuilder Use<T>(this LoggerFactoryBuilder builder, Action<T>? configure = default) where T : ILoggerNode, new()
        {
            var node = new T();
            configure?.Invoke(node);
            return builder.Use(node);
        }


        public static LoggerFactoryBuilder Use<T>(this LoggerFactoryBuilder builder, T node) where T : ILoggerNode
        {
            builder.Add(node);
            return builder;
        }
    }
}