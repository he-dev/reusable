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

        public static ILog Name(this ILog log, string value) => log.SetItem(Log.PropertyNames.Logger, value);
        
        public static ILog Timestamp(this ILog log, DateTime value) => log.SetItem(Log.PropertyNames.Timestamp, value);
        
        public static ILog Level(this ILog log, LogLevel value) => log.SetItem(Log.PropertyNames.Level, value);
        
        public static ILog Exception(this ILog log, Exception value) => log.SetItem(Log.PropertyNames.Exception, value);
        
        public static ILog Message(this ILog log, string value) => log.SetItem(Log.PropertyNames.Message, value);
        
        public static ILog OverrideTransaction(this ILog log) => log.SetItem(Log.PropertyNames.OverridesTransaction, true);

        public static ILog Transform(this ILog log, Func<ILog, ILog> transform) => transform(log);
        
        public static TLog SetItem<TLog>(this TLog log, SoftString name, object value) where TLog : ILog
        {
            if (value == Log.PropertyNames.Unset)
            {
                log.Remove(name);
            }
            else
            {
                log[name] = value;
            }

            return log;
        }

        public static T GetItemOrDefault<T>(this ILog log, SoftString name, T defaultValue = default)
        {
            return log.TryGetValue(name, out var obj) && obj is T result ? result : defaultValue;
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
            log.Add(Log.PropertyNames.CallerMemberName, callerMemberName);
            log.Add(Log.PropertyNames.CallerLineNumber, callerLineNumber);
            log.Add(Log.PropertyNames.CallerFilePath, callerFilePath);

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