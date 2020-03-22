using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<SoftString, ILogger> _loggers = new ConcurrentDictionary<SoftString, ILogger>();

        public LoggerFactory(IEnumerable<ILoggerNode> nodes) => CreateNodes = () => nodes;

        public LoggerFactory() : this(Enumerable.Empty<ILoggerNode>()) { }

        public Func<IEnumerable<ILoggerNode>> CreateNodes { get; set; }

        public static ILoggerFactory Empty() => new LoggerFactory(Enumerable.Empty<ILoggerNode>());

        #region ILoggerFactory

        public ILogger CreateLogger(string name) => _loggers.GetOrAdd(name, n => CreatePipeline(n.ToString()));

        private ILogger CreatePipeline(string loggerName)
        {
            return (ILogger)CreateNodes().Prepend(new Logger { Name = loggerName }).Join().First();
        }

        public void Dispose()
        {
            foreach (var logger in _loggers)
            {
                foreach (var node in ((ILoggerNode)logger.Value).EnumerateNext())
                {
                    node.Dispose();
                }
            }
            _loggers.Clear();
        }

        #endregion
    }

    public static class LoggerFactoryExtensions
    {
        public static ILogger<T> CreateLogger<T>(this ILoggerFactory loggerFactory) => new Logger<T>(loggerFactory);
    }
}