using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class NLogRx : LogRx
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

        protected override void Log(ILog log)
        {
            GetLogger(log.Name()).Log(CreateLogEventInfo(log));
        }

        private static NLog.LogEventInfo CreateLogEventInfo(ILog log)
        {
            log = log.Flatten();
            var logEventInfo = new NLog.LogEventInfo
            {
                Level = LogLevelMap[log.Level()],
                LoggerName = log.Name().ToString(),
                Message = log.Message(),
                Exception = log.Exception(),
                TimeStamp = log.Timestamp(),
            };

            logEventInfo.Properties.AddRangeSafely(log.Select(x => ((object)x.Key.ToString(), x.Value)));

            return logEventInfo;
        }

        private NLog.ILogger GetLogger(SoftString name)
        {
            return _cache.GetOrAdd(name, n => NLog.LogManager.GetLogger(name.ToString()));
        }

        public static NLogRx Create() => new NLogRx();
    }
}