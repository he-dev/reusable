using System;
using System.Collections.Generic;
using System.Text;

namespace Reusable.Logging.Loggex.Recorders.NLogRecorder.Recorders
{
    public class NLogRecorder : IRecorder
    {
        private readonly IDictionary<CaseInsensitiveString, NLog.Logger> loggers = new Dictionary<CaseInsensitiveString, NLog.Logger>();

        public NLogRecorder(CaseInsensitiveString name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public CaseInsensitiveString Name { get; set; }

        public void Log(LogEntry logEntry)
        {
            var logger = GetLogger(logEntry.Name().ToString());
            
            //if (!logger.IsEnabled(NLog.LogLevel.FromOrdinal((int)logEntry.LogLevel)))
            {
               // return;
            }

            var logEventInfo = new NLog.LogEventInfo
            {
                Level = NLog.LogLevel.FromString(logEntry.LogLevel().ToString()),
                LoggerName = logger.Name,
                Message = logEntry.GetValueOrDefault<StringBuilder>(nameof(NLog.LogEventInfo.Message)).ToString(),
                Exception = logEntry.GetValueOrDefault<Exception>(nameof(NLog.LogEventInfo.Exception)),
                //TimeStamp = log.GetValue<DateTime>(nameof(Log.Timestamp)),
            };

            foreach (var property in logEntry)
            {
                logEventInfo.Properties[property.Key.ToString()] = property.Value;
            }

            logger.Log(logEventInfo);
        }

        private NLog.Logger GetLogger(string name)
        {
            if (loggers.TryGetValue(name, out var logger))
            {
                return logger;
            }

            return (loggers[name] = NLog.LogManager.GetLogger(name));
        }
    }
}
