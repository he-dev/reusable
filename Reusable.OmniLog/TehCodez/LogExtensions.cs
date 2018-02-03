using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    public static class LogExtensions
    {
        #region Log properties

        public static SoftString Name(this Log log, object value = null) => log.Property<SoftString>(value);

        public static DateTime Timestamp(this Log log, object value = null) => log.Property<DateTime>(value);

        public static TimeSpan Elapsed(this Log log, object value = null) => log.Property<TimeSpan>(value);

        public static LogLevel Level(this Log log, object value = null) => log.Property<LogLevel>(value);

        public static string Message(this Log log, object value = null) => log.Property<string>(value);

        public static MessageFunc MessageFunc(this Log log, object value = null) => log.Property<MessageFunc>(value);

        public static Exception Exception(this Log log, object value = null) => log.Property<Exception>(value);

        public static SoftString Scope(this Log log, object value = null) => log.Property<SoftString>(value);

        public static LogBag Bag(this Log log, object value = null) => log.Property<LogBag>(value, nameof(LogBag));

        public static IEnumerable<SoftString> Scopes(this Log log)
        {
            foreach (var logValue in log.Values)
            {
                if (logValue is Log nestedLog)
                {
                    var scope = nestedLog.Scope();
                    if (scope.IsNotNull())
                    {
                        yield return scope;
                    }
                }
            }
        }

        // 'propertyName' is never null because it is set  by the compiler.
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public static T Property<T>(this Log log, object value = null, [CallerMemberName] string propertyName = null)
        {
            var isGetterMode = value == null;

            if (isGetterMode)
            {
                return log.TryGetValue(propertyName, out var obj) && obj is T result ? result : default(T);
            }
            else
            {
                if (value == LogProperty.Unset)
                {
                    log.Remove(propertyName);
                }
                else
                {
                    log[propertyName] = value;
                }

                return default(T);
            }
        }

        #endregion

        #region With

        public static Log WithCallerInfo(
            this Log log,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null
        )
        {
            log.Add(LogProperty.CallerMemberName, callerMemberName);
            log.Add(LogProperty.CallerLineNumber, callerLineNumber);
            log.Add(LogProperty.CallerFilePath, callerFilePath);

            return log;
        }

        public static Log With<T>(this Log log, (SoftString Name, T Value) item)
        {
            log[item.Name] = item.Value;
            return log;
        }

        public static Log With<T>(this Log log, SoftString name, T value)
        {
            log[name] = value;
            return log;
        }

        //public static Log With(this Log log, [CanBeNull] Func<Log, Log> logFunc)
        //{
        //    return (logFunc ?? (_ => _))(log);
        //}

        #endregion

        // Flattens log by picking the first item from each group. Groups are built on the key.
        public static Log Flatten(this Log log, IDictionary<SoftString, ILogScopeMerge> scopeMerges)
        {
            var items = log.SelectMany(l =>
            {
                switch (l.Value)
                {
                    case ILog sublog: return sublog;
                    default: return new Log { [l.Key] = l.Value };
                }
            });

            // The first item in each group is the most inner scope.
            var innerScope =
                items
                    .GroupBy(i => i.Key)
                    .Select(scope => scopeMerges.TryGetValue(scope.Key, out var merge) ? merge.Merge(scope) : scope.First());

            return new Log().AddRange(innerScope);
        }

        // Finds a value by path. A path is a dot separated string of names.
        public static object FindValue(this Log log, string path)
        {
            var names = path.Split('.');

            var currentLog = log;
            var currentValue = default(object);

            foreach (var name in names)
            {
                if (!currentLog.TryGetValue(name, out currentValue))
                {
                    return null;
                }

                if (currentValue is Log l)
                {
                    currentLog = l;
                }
            }

            return currentValue;
        }
    }
}