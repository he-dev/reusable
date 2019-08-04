using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;
using Reusable.OmniLog.v2.Middleware;

namespace Reusable.OmniLog.v2
{
    public interface ILoggerFactory : IDisposable
    {
        [NotNull]
        ILogger CreateLogger([NotNull] SoftString name);
    }

    public class LoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<SoftString, ILogger> _loggers;

        public LoggerFactory()
        {
            _loggers = new ConcurrentDictionary<SoftString, ILogger>();
        }

        public List<ILogRx> Receivers { get; set; } = new List<ILogRx>();

        public List<LoggerMiddleware> Middleware { get; set; } = new List<LoggerMiddleware>();
        

        public List<Type> MiddlewareOrder { get; set; } = new List<Type>
        {
            typeof(v2.Middleware.LoggerPropertySetter),
            typeof(v2.Middleware.LoggerStopwatch),
            typeof(v2.Middleware.LoggerAttachment),
            typeof(v2.Middleware.LoggerLambda),
            typeof(v2.Middleware.LoggerCorrelation),
            typeof(v2.Middleware.LoggerSerializer),
            typeof(v2.Middleware.LoggerFilter),
            typeof(v2.Middleware.LoggerTransaction),
            typeof(v2.Middleware.LoggerEcho),
        };

        #region ILoggerFactory

        public ILogger CreateLogger(SoftString name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _loggers.GetOrAdd(name, n =>
            {
                var positions = MiddlewareOrder.Select((m, i) => (m, i)).ToDictionary(t => t.m, t => t.i);
                var baseMiddleware = new LoggerPropertySetter(("Logger", n)).InsertNext(new LoggerEcho(Receivers));
                foreach (var middleware in Middleware)
                {
                    baseMiddleware.InsertRelative(middleware, positions);
                }

                return new Logger(baseMiddleware, positions);
            });
        }

        public void Dispose() { }

        #endregion

        private static Func<ILog, bool> Any => l => l.Any();
    }
}