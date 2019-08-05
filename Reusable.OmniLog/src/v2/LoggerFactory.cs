using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.v2.Middleware;

namespace Reusable.OmniLog.v2
{
    public class LoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<SoftString, ILogger> _loggers;

        public LoggerFactory()
        {
            _loggers = new ConcurrentDictionary<SoftString, ILogger>();
        }

        public List<ILogRx> Receivers { get; set; } = new List<ILogRx>();

        public List<LoggerMiddleware> Middleware { get; set; } = new List<LoggerMiddleware>();


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

            return _loggers.GetOrAdd(name, n =>
            {
                //var positions = MiddlewareOrder.Select((m, i) => (m, i)).ToDictionary(t => t.m, t => t.i);
                //var baseMiddleware = new LoggerProperty((LogPropertyNames.Logger.ToString(), n.ToString())).InsertNext(new LoggerEcho(Receivers));
                var middleware = (LoggerMiddleware)new LoggerProperty((Reusable.OmniLog.Log.PropertyNames.Logger.ToString(), n.ToString()));
                foreach (var current in Middleware)
                {
                   middleware = middleware.InsertNext(current);
                }

                middleware = middleware.InsertNext(new LoggerEcho(Receivers));

                return new Logger(middleware.First(), default);
            });
        }

        public void Dispose() { }

        #endregion

        private static Func<ILog, bool> Any => l => l.Any();
    }
    
    public static class LoggerFactoryExtensions
    {
        public static LoggerFactory Use<T>(this LoggerFactory loggerFactory, T middleware) where T : LoggerMiddleware
        {
            //var current = default(LoggerMiddleware);
            loggerFactory.Middleware.Add(middleware);
            return loggerFactory;
        }
        
        public static LoggerFactory Use<T>(this LoggerFactory loggerFactory) where T : LoggerMiddleware, new()
        {
            return loggerFactory.Use(new T());
        }
    }
}