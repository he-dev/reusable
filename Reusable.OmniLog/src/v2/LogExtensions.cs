using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.v2
{
    public static class LogExtensions
    {
        #region Log properties

        public static ILog Logger(this ILog log, string value) => log.SetItem(nameof(Logger), value);

        public static ILog Timestamp(this ILog log, DateTime value) => log.SetItem(nameof(Timestamp), value);

        public static ILog Level(this ILog log, LogLevel value) => log.SetItem(nameof(Level), value);

        public static ILog Exception(this ILog log, Exception value) => log.SetItem(nameof(Exception), value);

        public static ILog Message(this ILog log, string value) => log.SetItem(nameof(Message), value);

        #endregion

        public static TLog SetItem<TLog>(this TLog log, SoftString name, object value) where TLog : ILog
        {
            if (value == Reusable.OmniLog.Log.PropertyNames.Unset)
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
    }
}