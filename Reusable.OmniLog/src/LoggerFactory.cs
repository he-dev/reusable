using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<SoftString, ILogger> _loggers;

        public LoggerFactory()
        {
            _loggers = new ConcurrentDictionary<SoftString, ILogger>();
        }

        public List<LoggerNode> Nodes { get; set; } = new List<LoggerNode>();

        //        public List<Type> MiddlewareOrder { get; set; } = new List<Type>
        //        {
        //            typeof(v2.Middleware.LoggerProperty),
        //            typeof(v2.Middleware.LoggerStopwatch),
        //            typeof(v2.Middleware.LoggerAttachment),
        //            typeof(v2.Middleware.LoggerLambda),
        //            typeof(v2.Middleware.LoggerCorrelation),
        //            typeof(v2.Middleware.LoggerSerializer),
        //            typeof(v2.Middleware.LoggerFilter),
        //            typeof(v2.Middleware.LoggerTransaction),
        //            typeof(v2.Middleware.LoggerEcho),
        //        };

        #region ILoggerFactory

        public ILogger CreateLogger(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _loggers.GetOrAdd(name, n => new Logger(CreatePipeline(n.ToString())));
        }

        private LoggerNode CreatePipeline(string logger)
        {
            return Nodes.Aggregate<LoggerNode, LoggerNode>(new ConstantNode { { "Logger", logger } }, (current, next) => current.InsertNext(next)).First();
        }

        public void Dispose() { }

        #endregion
    }

    public static class LoggerFactoryExtensions
    {
        [NotNull]
        public static ILogger<T> CreateLogger<T>([NotNull] this ILoggerFactory loggerFactory, bool includeNamespace = false)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            return new Logger<T>(loggerFactory);
        }

//        public static LoggerFactory Use<T>(this LoggerFactory loggerFactory, T middleware) where T : LoggerNode
//        {
//            //var current = default(LoggerMiddleware);
//            loggerFactory.Nodes.Add(middleware);
//            return loggerFactory;
//        }
//
//        public static LoggerFactory Use<T>(this LoggerFactory loggerFactory) where T : LoggerNode, new()
//        {
//            return loggerFactory.Use(new T());
//        }
    }
}