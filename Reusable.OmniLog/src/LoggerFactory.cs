using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Services;

namespace Reusable.OmniLog
{
    public class LoggerFactory : ILoggerFactory, IEnumerable<ILoggerNode>
    {
        private readonly IEnumerable<ILoggerNode> _nodes;
        private readonly ConcurrentDictionary<SoftString, ILogger> _loggers = new ConcurrentDictionary<SoftString, ILogger>();

        public LoggerFactory(IEnumerable<ILoggerNode> nodes) => _nodes = nodes;
        
        public static LoggerFactoryBuilder Builder() => new LoggerFactoryBuilder();

        #region ILoggerFactory

        public ILogger CreateLogger(string name) => _loggers.GetOrAdd(name!, n => (ILogger)CreatePipeline(n.ToString()));

        private ILoggerNode CreatePipeline(string loggerName)
        {
            var logger = new Logger
            {
                Next = new ServiceNode { Services = { new Constant(nameof(Logger), loggerName) } }
            };
            
            var current = logger.Next;

            foreach (var node in this)
            {
                current = current.AddAfter(node);
            }

            return logger;
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