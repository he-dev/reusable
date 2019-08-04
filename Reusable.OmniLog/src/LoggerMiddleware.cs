using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog.Experimental
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
            typeof(LoggerPropertySetter),
            typeof(LoggerStopwatch),
            typeof(LoggerAttachment),
            typeof(LoggerLambda),
            typeof(LoggerScope),
            typeof(LoggerSerializer),
            typeof(LoggerFilter),
            typeof(LoggerTransaction),
            typeof(LoggerEcho),
        };

        #region ILoggerFactory

        public ILogger CreateLogger(SoftString name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _loggers.GetOrAdd(name, n =>
                {
                    var positions = MiddlewareOrder.Select((m, i) => (m, i)).ToDictionary(t => t.m, t => t.i);
                    var baseSetup = new LoggerPropertySetter(("Logger", name)).InsertNext(new LoggerEcho(Receivers));
                    foreach (var middleware in Middleware)
                    {
                        baseSetup.InsertRelative(middleware, positions);
                    }

                    return new Logger(baseSetup, positions);
                }
            );
        }

        public void Dispose()
        {
//            foreach (var attachment in Attachments)
//            {
//                if (attachment is IDisposable disposable)
//                {
//                    disposable.Dispose();
//                }
//            }
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
        private readonly IDictionary<Type, int> _middlewarePositions;

        public Logger(LoggerMiddleware middleware, IDictionary<Type, int> middlewarePositions)
        {
            // Always start with the first middleware.
            _middleware = middleware.First();
            _middlewarePositions = middlewarePositions;
        }

        public T Use<T>(T next) where T : LoggerMiddleware
        {
            return _middleware.InsertRelative(next, _middlewarePositions);
        }

        public void Log(Log log)
        {
            _middleware.Invoke(log);
        }
    }


    public abstract class LoggerMiddleware : IDisposable
    {
        [JsonIgnore]
        public LoggerMiddleware Previous { get; private set; }

        [JsonIgnore]
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
        private readonly string _propertyName;
        private readonly Stopwatch _stopwatch;

        public LoggerStopwatch(string propertyName = nameof(Stopwatch.Elapsed))
        {
            _propertyName = propertyName;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Reset() => _stopwatch.Reset();

        public override void Invoke(Log request)
        {
            request[_propertyName] = _stopwatch.Elapsed;
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
            base.Invoke(request);
        }
    }


    public class LoggerTransaction : LoggerMiddleware
    {
        private readonly Queue<Log> _buffer = new Queue<Log>();

        public override void Invoke(Log request)
        {
            _buffer.Enqueue(request);
            // Don't call Next until Commit.
        }

        public void Commit()
        {
            while (_buffer.Any())
            {
                base.Invoke(_buffer.Dequeue());
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
                base.Invoke(request);
            }
        }
    }

    public class LoggerAttachment : LoggerMiddleware, IEnumerable<ILogAttachment>
    {
        private readonly IList<ILogAttachment> _attachments = new List<ILogAttachment>();


        public override void Invoke(Log request)
        {
            foreach (var attachment in _attachments)
            {
                request[attachment.Name] = attachment.Compute(request);
            }

            base.Invoke(request);
        }

        public void Add(ILogAttachment attachment) => _attachments.Add(attachment);

        public IEnumerator<ILogAttachment> GetEnumerator() => _attachments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_attachments).GetEnumerator();
    }

    public class LoggerSerializer : LoggerMiddleware
    {
        public static readonly string SerializableSuffix = ".Serializable";

        private readonly ISerializer _serializer;
        private readonly IList<string> _propertyNames;

        public LoggerSerializer(ISerializer serializer, params string[] propertyNames)
        {
            _serializer = serializer;
            _propertyNames = propertyNames;
        }

        public override void Invoke(Log request)
        {
            var propertyNames = _propertyNames.Any() ? _propertyNames : request.Keys.Select(k => k.ToString()).ToList();

            foreach (var propertyName in propertyNames)
            {
                var dataKey = propertyName.EndsWith(SerializableSuffix) ? propertyName : CreateDataKey(propertyName);
                if (request.TryGetValue(dataKey, out var obj))
                {
                    var actualPropertyName = Regex.Replace(propertyName, $"{Regex.Escape(SerializableSuffix)}$", string.Empty);
                    request[actualPropertyName] = _serializer.Serialize(obj);
                    request.Remove(dataKey); // Make sure we won't try to do it again
                }
            }


            base.Invoke(request);
        }

        public static string CreateDataKey(string propertyName) => $"{propertyName}{SerializableSuffix}";
    }

    public class LoggerScope : LoggerMiddleware
    {
        private static readonly AsyncLocal<Stack<LoggerScope>> _current = new AsyncLocal<Stack<LoggerScope>>
        {
            Value = new Stack<LoggerScope>()
        };

        public LoggerScope(object correlationId, object correlationHandle)
        {
            CorrelationId = correlationId ?? NextCorrelationId();
            correlationHandle = correlationHandle;
            Current.Push(this);
        }

        public object CorrelationId { get; }

        public object CorrelationHandle { get; }

        /// <summary>
        /// Gets or sets the factory for the default correlation-id. By default it's a Guid.
        /// </summary>
        public static Func<object> NextCorrelationId { get; set; } = () => Guid.NewGuid().ToString("N");

        private static Stack<LoggerScope> Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }

        public override void Invoke(Log request)
        {
            request.AttachSerializable("Scope", Current);
            base.Invoke(request);
        }

        public override void Dispose()
        {
            if (!Current.Any())
            {
                throw new InvalidOperationException("This should not have occured. The scope seems to be disposed too many times");
            }

            Current.Pop();

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
        public static T2 InsertRelative<T1, T2>(this T1 middleware, T2 insert, IDictionary<Type, int> order)
            where T1 : LoggerMiddleware
            where T2 : LoggerMiddleware
        {
            if (middleware.Previous is null && middleware.Next is null)
            {
                throw new ArgumentException("There need to be at least two middlewares.");
            }

            var first = middleware.First();
            var zip = first.Enumerate(m => m.Next).Zip(first.Enumerate(m => m.Next).Skip(1), (current, next) => (current, next));

            foreach (var (current, next) in zip)
            {
                var canInsert =
                    order[insert.GetType()] >= order[current.GetType()] &&
                    order[insert.GetType()] <= order[next.GetType()];
                if (canInsert)
                {
                    return current.InsertNext(insert);
                }
            }

            return default; // This should not never be reached.
        }

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
        public static ILog Message(this ILog log, string message) => log.SetItem(nameof(Message), message);
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
        public static LoggerTransaction UseTransaction(this ILogger logger)
        {
            return logger.Use(new LoggerTransaction());
        }
    }

    public static class LoggerFilterHelper
    {
        public static LoggerFilter UseFilter(this ILogger logger, Func<Log, bool> canLog)
        {
            return logger.Use(new LoggerFilter { CanLog = canLog });
        }
    }

    public static class LoggerScopeHelper
    {
        public static LoggerScope UseScope(this ILogger logger, object correlationId = default, object correlationHandle = default)
        {
            return logger.Use(new LoggerScope(correlationId, correlationHandle));
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