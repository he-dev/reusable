using Reusable.Logging.ComputedProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Logging
{
    public class LogEntry : Dictionary<string, object>
    {
        private LogLevel _logLevel;

        private LogEntry(LogEntry logEntry) : base(logEntry)
        {
            _logLevel = logEntry._logLevel;
        }

        public LogEntry(LogLevel logLevel) : base(StringComparer.OrdinalIgnoreCase)
        {
            LogLevel = logLevel;
        }

        public LogEntry(IDictionary<string, object> logEntry) : base(logEntry, StringComparer.OrdinalIgnoreCase) { }

        // This needs to be a property for performance reasons.
        public LogLevel LogLevel
        {
            get => _logLevel;
            set { _logLevel = value; this[nameof(LogLevel)] = value; }
        }        

        public static LogEntry New() => CreateLogEntry(LogLevel.Info).Info();

        private static LogEntry CreateLogEntry(LogLevel logLevel) => new LogEntry(logLevel)
            .SetValue(new Now())
            .SetValue(new UtcNow())
            .SetValue(new ThreadId());
    }

    public class AutoLogEntry : LogEntry, IDisposable
    {
        private readonly ILogger _logger;

        public AutoLogEntry(LogEntry logEntry, ILogger logger) : base(logEntry) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void Dispose() => this.Log(_logger);
    }
}
