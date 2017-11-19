using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Reusable.Collections;
using Reusable.OmniLog.Collections;

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

        private readonly IDictionary<SoftString, ILogScopeMerge> _scopeMerges;

        private NLogRx(IEnumerable<ILogScopeMerge> scopeMerges)
        {
             _scopeMerges = scopeMerges.ToDictionary(m => m.Name, m => m);
        }

        protected override IObserver<Log> Initialize()
        {
            return Observer.Create<Log>(Log);
        }

        private void Log(Log log)
        {
            GetLogger(log.Name()).Log(CreateLogEventInfo(log, _scopeMerges));
        }

        private static NLog.LogEventInfo CreateLogEventInfo(Log log, IDictionary<SoftString, ILogScopeMerge> scopeMerges)
        {
            log = log.Flatten(scopeMerges);
            var logEventInfo = new NLog.LogEventInfo
            {
                Level = LogLevelMap[log.LogLevel()],
                LoggerName = log.Name().ToString(),
                Message = log.Message(),
                Exception = log.Exception(),
                TimeStamp = log.Timestamp(),
            };

            logEventInfo.Properties.AddRange(log.Select(x => ((object) x.Key.ToString(), x.Value)));

            return logEventInfo;
        }

        private NLog.ILogger GetLogger(SoftString name)
        {
            return _cache.GetOrAdd(name, n => NLog.LogManager.GetLogger(name.ToString()));
        }

        public static IObserver<Log> Create(IEnumerable<ILogScopeMerge> scopeMerges) => new NLogRx(scopeMerges);        
    }
}