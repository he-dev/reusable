using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog
{
    using data = Reusable.OmniLog.Abstractions.Data;

    public class NLogRx : ILogRx
    {
        private readonly ConcurrentDictionary<SoftString, NLog.Logger> _cache = new ConcurrentDictionary<SoftString, NLog.Logger>();

        private static readonly IDictionary<LogLevel, NLog.LogLevel> LogLevelMap = new Dictionary<LogLevel, NLog.LogLevel>
        {
            [LogLevel.Trace] = NLog.LogLevel.Trace,
            [LogLevel.Debug] = NLog.LogLevel.Debug,
            [LogLevel.Information] = NLog.LogLevel.Info,
            [LogLevel.Warning] = NLog.LogLevel.Warn,
            [LogLevel.Error] = NLog.LogLevel.Error,
            [LogLevel.Fatal] = NLog.LogLevel.Fatal,
        };

        public void Log(Log log)
        {
            var loggerName = log.GetItemOrDefault<string>((data.Log.PropertyNames.Logger, default));
            var logger = GetLogger(loggerName);
            logger.Log(CreateLogEventInfo(log));
        }

        private static NLog.LogEventInfo CreateLogEventInfo(Log log)
        {
            var logEventInfo = new NLog.LogEventInfo
            {
                Level = LogLevelMap[log.GetItemOrDefault<LogLevel>((data.Log.PropertyNames.Level, default))],
                LoggerName = log.GetItemOrDefault<string>((data.Log.PropertyNames.Logger, default)),
                Message = log.GetItemOrDefault<string>((data.Log.PropertyNames.Message, default)),
                Exception = log.GetItemOrDefault<Exception>((data.Log.PropertyNames.Exception, default)),
                TimeStamp = log.GetItemOrDefault<DateTime>((data.Log.PropertyNames.Timestamp, default)),
            };

            logEventInfo.Properties.AddRangeSafely(log.Select(x => ((object)x.Key.ToString(), x.Value)));

            return logEventInfo;
        }

        private NLog.ILogger GetLogger(string name)
        {
            return _cache.GetOrAdd(name, n => NLog.LogManager.GetLogger(name));
        }

        public static NLogRx Create() => new NLogRx();
    }
}