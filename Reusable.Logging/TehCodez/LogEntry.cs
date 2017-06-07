using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Logging.ComputedProperties;

namespace Reusable.Logging
{
    public class LogEntry : Dictionary<string, object>
    {
        public LogEntry(LogLevel logLevel) : base(StringComparer.OrdinalIgnoreCase)
        {
            this[nameof(LogLevel)] = logLevel;
        }

        public LogEntry(LogEntry logEntry) : base(logEntry, StringComparer.OrdinalIgnoreCase)
        { }

        public LogLevel LogLevel => (LogLevel)this[nameof(LogLevel)];

        //public static Func<DateTime>

        public static LogEntry New() => CreateLogEntry(LogLevel.Info).Info();

        private static LogEntry CreateLogEntry(LogLevel logLevel)
        {
            return 
                new LogEntry(logLevel)
                    .SetValue(new Now())
                    .SetValue(new UtcNow())
                    .SetValue(new ThreadId());
        }
    }

    public class AutoLogEntry : LogEntry, IDisposable
    {
        private readonly ILogger _logger;

        public AutoLogEntry(LogEntry logEntry, ILogger logger) : base(logEntry) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void Dispose() => this.Log(_logger);
    }
}
