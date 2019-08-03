using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog.Experimental
{
    public static class Demo
    {
        public static void Main()
        {
            var logger = new Logger(new LoggerPropertySetter(("logger", "demo")).InsertNext(new LoggerEcho(Enumerable.Empty<ILogRx>())));
            // Include to filter certain messages out.
            //logger.UseFilter(l => !l["message"].Equals("tran-2-commit"));

            logger.Log(l => l.Message("begin"));

            using (var tran = logger.UseTransaction())
            using (logger.UseStopwatch())
            {
                logger.Log(l => l.Message("tran-1-commit"));
                logger.Log(l => l.Message("tran-2-commit"));
                tran.Commit();
            }

            using (var tran = logger.UseTransaction())
            {
                logger.Log(l => l.Message("tran-1-rollback"));
                logger.Log(l => l.Message("tran-2-rollback"));
                tran.Rollback();
            }

            logger.Log(l => l.Message("end"));
        }
    }

    public class LoggerFactory : IDisposable
    {
        private readonly ConcurrentDictionary<SoftString, ILogger> _loggers;

        public LoggerFactory()
        {
            _loggers = new ConcurrentDictionary<SoftString, ILogger>();
        }

        //public static LoggerFactory Empty => new LoggerFactory();

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x._loggers.Count);
            //builder.DisplayValue(x => x.Configuration.Attachments.Count);
        });

        [NotNull]
        public HashSet<ILogAttachment> Attachments { get; set; } = new HashSet<ILogAttachment>();

        public List<ILogRx> Receivers { get; set; } = new List<ILogRx>();

        public IEnumerable<LoggerMiddleware> Middleware { get; set; }

        #region ILoggerFactory

        public ILogger CreateLogger([NotNull] SoftString name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _loggers.GetOrAdd(name, n =>
                new Logger(
                    new LoggerPropertySetter(("Logger", name))
                        .InsertNext(new LoggerAttachment(Attachments))
                        .Pipe(m => Middleware?.Aggregate((LoggerMiddleware)m, (current, next) => current.InsertNext(next)) ?? m)
                        .InsertNext(new LoggerEcho(Receivers)))
            );
        }

        public void Dispose()
        {
            foreach (var attachment in Attachments)
            {
                if (attachment is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        #endregion

        private static Func<ILog, bool> Any => l => l.Any();
    }


    public interface ILogger
    {
        T Use<T>(T next) where T : LoggerMiddleware;

        void Log(Log log);
    }

    public class Logger : ILogger
    {
        private readonly LoggerMiddleware _middleware;

        public Logger(LoggerMiddleware middleware)
        {
            // Start with the first middleware in case this is already a chain.
            _middleware = middleware.First();
        }

        public T Use<T>(T next) where T : LoggerMiddleware
        {
            // The last middleware is Echo so put the new one before.
            return _middleware.Last().Previous.InsertNext(next);
        }

        public void Log(Log log)
        {
            _middleware.Invoke(log);
        }
    }


    public abstract class LoggerMiddleware : IDisposable
    {
        public LoggerMiddleware Previous { get; private set; }

        public LoggerMiddleware Next { get; private set; }

        // Inserts a new middleware after this one and returns the new one.
        public T InsertNext<T>(T next) where T : LoggerMiddleware
        {
            (next.Previous, next.Next, Next) = (this, Next, next);
            return next;
        }

        public virtual void Invoke(Log request) => Next?.Invoke(request);

        // Removes itself from the middleware chain.
        public virtual void Dispose()
        {
            if (Previous is null)
            {
                return;
            }

            (Previous.Next, Previous, Next) = (Next, null, null);
        }
    }

    public class LoggerPropertySetter : LoggerMiddleware
    {
        private readonly IEnumerable<(string Name, object Value)> _properties;

        public LoggerPropertySetter(IEnumerable<(string Name, object Value)> properties)
        {
            _properties = properties;
        }

        public LoggerPropertySetter(params (string Name, object Value)[] properties)
        {
            _properties = properties;
        }

        public override void Invoke(Log request)
        {
            foreach (var property in _properties)
            {
                request[property.Name] = property.Value;
            }

            Next?.Invoke(request);
        }
    }


    public class LoggerStopwatch : LoggerMiddleware
    {
        private readonly Stopwatch _stopwatch;

        public LoggerStopwatch()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public void Reset() => _stopwatch.Reset();

        public override void Invoke(Log request)
        {
            request["elapsed"] = _stopwatch.Elapsed;
            Next?.Invoke(request);
        }
    }


    public class LoggerLambda : LoggerMiddleware
    {
        private readonly Action<Log> _transform;

        public LoggerLambda(Action<Log> transform)
        {
            _transform = transform;
        }

        public override void Invoke(Log request)
        {
            _transform(request);
            Next?.Invoke(request);
        }
    }


    public class LoggerTransaction : LoggerMiddleware
    {
        private readonly Queue<Log> _buffer = new Queue<Log>();

        public override void Invoke(Log request)
        {
            _buffer.Enqueue(request);
            //Next?.Invoke(request); // <-- don't call Next until Commit
        }

        public void Commit()
        {
            while (_buffer.Any())
            {
                Next?.Invoke(_buffer.Dequeue());
            }
        }

        public void Rollback()
        {
            _buffer.Clear();
        }
    }


    public class LoggerFilter : LoggerMiddleware
    {
        public Func<Log, bool> CanLog { get; set; } = _ => true;

        public override void Invoke(Log request)
        {
            if (CanLog(request))
            {
                Next?.Invoke(request);
            }
        }
    }

    public class LoggerAttachment : LoggerMiddleware
    {
        private readonly IEnumerable<ILogAttachment> _attachments;

        public LoggerAttachment(IEnumerable<ILogAttachment> attachments)
        {
            _attachments = attachments;
        }

        public override void Invoke(Log request)
        {
            foreach (var attachment in _attachments)
            {
                request[attachment.Name] = attachment.Compute(request);
            }

            base.Invoke(request);
        }
    }

    public class LoggerSerializer : LoggerMiddleware
    {
        private readonly ISerializer _serializer;
        private readonly string _propertyName;

        public LoggerSerializer(ISerializer serializer, string propertyName)
        {
            _serializer = serializer;
            _propertyName = propertyName;
        }

        public override void Invoke(Log request)
        {
            var dataKey = CreateDataKey(_propertyName);
            if (request.TryGetValue(dataKey, out var obj))
            {
                request[_propertyName] = _serializer.Serialize(obj);
                request.Remove(dataKey); // Make sure we won't try to do it again
            }

            base.Invoke(request);
        }

        public static string CreateDataKey(string propertyName) => $"{propertyName}.Serializable";
    }

    public class LoggerScope : LoggerMiddleware
    {
        private static readonly AsyncLocal<Stack<object>> _current = new AsyncLocal<Stack<object>>
        {
            Value = new Stack<object>()
        };

        public LoggerScope()
        {
            Current.Push(new { CorrelationId = Guid.NewGuid() });
        }

        private static Stack<object> Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }

        public override void Invoke(Log request)
        {
            request["Scope"] = Current;
            Next?.Invoke(request);
        }

        public override void Dispose()
        {
            //if (Current.Any())
            {
                Current.Pop();
            }

            base.Dispose();
        }
    }


    public class LoggerEcho : LoggerMiddleware
    {
        private readonly IEnumerable<ILogRx> _receivers;

        public LoggerEcho(IEnumerable<ILogRx> receivers)
        {
            _receivers = receivers;
        }

        public override void Invoke(Log request)
        {
            foreach (var rx in _receivers)
            {
                rx.Log(request);
            }
        }
    }

    // Helpers

    public static class LoggerMiddlewareExtensions
    {
        public static LoggerMiddleware First(this LoggerMiddleware middleware)
        {
            return middleware.Enumerate(m => m.Previous).Last();
        }

        public static LoggerMiddleware Last(this LoggerMiddleware middleware)
        {
            return middleware.Enumerate(m => m.Next).Last();
        }

        private static IEnumerable<LoggerMiddleware> Enumerate(this LoggerMiddleware middleware, Func<LoggerMiddleware, LoggerMiddleware> direction)
        {
            do
            {
                yield return middleware;
            } while (!((middleware = direction(middleware)) is null));
        }
    }

    public static class LogExtensions
    {
        public static Log Message(this Log log, string message)
        {
            log["message"] = message;
            return log;
        }
    }

    public static class LoggerExtensions
    {
        public static void Log(this ILogger logger, Action<Log> transform)
        {
            using (logger.UseLambda(transform))
            {
                logger.Log(new Log());
            }
        }
    }

    public static class LoggerStopwatchHelper
    {
        public static LoggerStopwatch UseStopwatch(this ILogger logger)
        {
            return logger.Use(new LoggerStopwatch());
        }
    }


    public static class LoggerLambdaHelper
    {
        public static LoggerLambda UseLambda(this ILogger logger, Action<Log> transform)
        {
            return logger.Use(new LoggerLambda(transform));
        }
    }

    public static class LoggerTransactionHelper
    {
        public static LoggerTransaction UseTransaction(this Logger logger)
        {
            return logger.Use(new LoggerTransaction());
        }
    }

    public static class LoggerFilterHelper
    {
        public static LoggerFilter UseFilter(this Logger logger, Func<Log, bool> canLog)
        {
            return logger.Use(new LoggerFilter { CanLog = canLog });
        }
    }

    public static class LoggerScopeHelper
    {
        public static LoggerScope UseScope(this Logger logger)
        {
            return logger.Use(new LoggerScope());
        }
    }

    public static class LoggerSerializerHelper
    {
        public static ILog AttachSerializable(this ILog log, string propertyName, object obj)
        {
            return log.SetItem(LoggerSerializer.CreateDataKey(propertyName), obj);
        }
    }
}