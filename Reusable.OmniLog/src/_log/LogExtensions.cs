using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public static class LogExtensions
    {
        #region Log properties

        public static SoftString Name(this ILog log, object value = default) => log.Property<SoftString>(value);

        public static DateTime Timestamp(this ILog log, object value = default) => log.Property<DateTime>(value);

        public static TimeSpan Elapsed(this ILog log, object value = default) => log.Property<TimeSpan>(value);

        public static LogLevel Level(this ILog log, object value = default) => log.Property<LogLevel>(value);

        public static string Message(this ILog log, object value = default) => log.Property<string>(value);

        public static MessageFunc MessageFunc(this ILog log, object value = default) => log.Property<MessageFunc>(value);

        public static Exception Exception(this ILog log, object value = default) => log.Property<Exception>(value);

        public static ILog OverrideTransaction(this ILog log) => log.With((LogProperties.OverrideTransaction, true));
        
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")] // 'propertyName' is never null because it is set by the compiler.
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

        public static ILog WithCallerInfo
        (
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
            return log.With(item.Name, item.Value);
        }

        public static TLog With<TLog, T>(this TLog log, SoftString name, T value) where TLog : ILog
        {
            name = Regex.Replace((string)name, "^With", string.Empty);
            log[name] = value;
            return log;
        }

        public static TLog With<TAttachment, TLog>(this TLog log, TAttachment attachment)
            where TAttachment : ILogAttachment
            where TLog : ILog
        {
            log[attachment.Name] = attachment;
            return log;
        }

        #endregion

        // Flattens log by picking the first item from each group. Groups are built on the key.
        public static Log Flatten(this ILog log)
        {
            var items = log.SelectMany(l =>
            {
                switch (l.Value)
                {
                    case ILog innerLog: return innerLog;
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