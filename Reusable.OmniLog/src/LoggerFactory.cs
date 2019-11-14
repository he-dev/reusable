using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<SoftString, ILogger> _loggers = new ConcurrentDictionary<SoftString, ILogger>();

        public List<ILoggerNode> Nodes { get; set; } = new List<ILoggerNode>();

        #region ILoggerFactory

        public ILogger CreateLogger(string name) => _loggers.GetOrAdd(name, n => (ILogger)CreatePipeline(n.ToString()));

        private ILoggerNode CreatePipeline(string loggerName)
        {
            var logger = new Logger
            {
                Next = new ConstantNode { Values = { [nameof(Logger)] = loggerName } }
            };
            var current = logger.Next;
            
            foreach (var node in Nodes)
            {
                current = current.AddAfter(node);
            }

            return logger;
        }

        public void Dispose() { }

        #endregion
    }

    public static class LoggerFactoryExtensions
    {
        [NotNull]
        public static ILogger<T> CreateLogger<T>(this ILoggerFactory loggerFactory)
        {
            return new Logger<T>(loggerFactory);
        }

        public static LoggerFactory Use<T>(this LoggerFactory loggerFactory, Action<T>? configure = default) where T : ILoggerNode, new()
        {
            var node = new T();
            configure?.Invoke(node);
            return loggerFactory.Use(node);
        }


        public static LoggerFactory Use<T>(this LoggerFactory loggerFactory, T node) where T : ILoggerNode
        {
            loggerFactory.Nodes.Add(node);
            return loggerFactory;
        }
    }
}