using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Logging;

namespace Reusable.Logging.Adapters
{
    public class NLogAdapter : Logger
    {
        private readonly NLog.Logger _logger;

        public NLogAdapter(string name) : base(name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            _logger = NLog.LogManager.GetLogger(name);
        }

        public override bool CanLog(LogEntry logEntry) => _logger.IsEnabled(NLog.LogLevel.FromOrdinal((int)logEntry.LogLevel));

        protected override void LogCore(LogEntry logEntry)
        {
            var logEventInfo = new NLog.LogEventInfo
            {
                Level = NLog.LogLevel.FromString(logEntry.LogLevel.ToString()),
                LoggerName = _logger.Name,
                Message = logEntry.GetValue<StringBuilder>(nameof(NLog.LogEventInfo.Message)).ToString(),
                Exception = logEntry.GetValue<Exception>(nameof(NLog.LogEventInfo.Exception)),
                //TimeStamp = log.GetValue<DateTime>(nameof(Log.Timestamp)),
            };

            foreach (var property in logEntry) logEventInfo.Properties[property.Key] = property.Value;

            _logger.Log(logEventInfo);
        }
    }

    public class NLogFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string name) => new NLogAdapter(name);
    }
}
