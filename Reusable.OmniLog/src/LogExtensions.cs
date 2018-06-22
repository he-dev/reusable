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

        public static SoftString Name(this ILog log, object value = null) => log.Property<SoftString>(value);

        public static DateTime Timestamp(this ILog log, object value = null) => log.Property<DateTime>(value);

        public static TimeSpan Elapsed(this ILog log, object value = null) => log.Property<TimeSpan>(value);

        public static LogLevel Level(this ILog log, object value = null) => log.Property<LogLevel>(value);

        public static string Message(this ILog log, object value = null) => log.Property<string>(value);

        public static MessageFunc MessageFunc(this ILog log, object value = null) => log.Property<MessageFunc>(value);

        public static Exception Exception(this ILog log, object value = null) => log.Property<Exception>(value);

        //public static SoftString Scope(this ILog log, object value = null) => log.Property<SoftString>(value);

        public static IEnumerable<ILogScope> Scopes(this ILog log)
        {
            return
                LogScope
                    .Current
                    .Flatten();

            //foreach (var logValue in log.Values)
            //{
            //    if (logValue is ILog nestedLog)
            //    {
            //        var scope = nestedLog.Scope();
            //        if (scope.IsNotNull())
            //        {
            //            yield return scope;
            //        }
            //    }
            //}
        }

        // 'propertyName' is never null because it is set  by the compiler.
        //[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public static T Property<T>(this ILog log, object value = null, [CallerMemberName] string propertyName = null)
        {
            var isGetterMode = value == null;

            if (isGetterMode)
            {
                return log.TryGetValue(propertyName, out var obj) && obj is T result ? result : default;
            }
            else
            {
                if (value == LogProperties.Unset)
                {
                    log.Remove(propertyName);
                }
                else
                {
                    log[propertyName] = value;
                }

                return default;
            }
        }

        #endregion

        #region With

        public static ILog WithCallerInfo(
            this ILog log,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null
        )
        {
            log.Add(LogProperties.CallerMemberName, callerMemberName);
            log.Add(LogProperties.CallerLineNumber, callerLineNumber);
            log.Add(LogProperties.CallerFilePath, callerFilePath);

            return log;
        }

        public static ILog With<T>(this ILog log, (SoftString Name, T Value) item)
        {
            log[item.Name] = item.Value;
            return log;
        }

        //public static ILog With<T>(this ILog log, SoftString name, T value)
        //{
        //    log[name] = value;
        //    return log;
        //}

        public static TLog With<TLog, T>(this TLog log, SoftString name, T value) where TLog : ILog
        {
            log[name] = value;
            return log;
        }

        public static TLog With<TAttachement, TLog>(this TLog log, TAttachement attachement)
            where TAttachement : ILogAttachement
            where TLog : ILog
        {
            log[attachement.Name] = attachement;
            return log;
        }

        //public static Log With(this Log log, [CanBeNull] Func<Log, Log> logFunc)
        //{
        //    return (logFunc ?? (_ => _))(log);
        //}

        #endregion

        // Flattens log by picking the first item from each group. Groups are built on the key.
        public static Log Flatten(this ILog log) //, IDictionary<SoftString, ILogScopeMerge> scopeMerges)
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
                    .Select(scope => scope.First());

            return new Log().AddRangeSafely(innerScope);
        }

        // Finds a value by path. A path is a dot separated string of names.
        public static object FindValue(this ILog log, string path)
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