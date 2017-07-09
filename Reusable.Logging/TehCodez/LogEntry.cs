using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Loggex.ComputedProperties;

namespace Reusable.Loggex
{
    public class LogEntry : Dictionary<CaseInsensitiveString, object>
    {
        public LogEntry(LogLevel logLevel)
        {
            this[nameof(LogLevel)] = logLevel;
        }

        public LogEntry(LogEntry logEntry) : base(logEntry) { }

        public LogLevel LogLevel => (LogLevel)this[nameof(LogLevel)];

        public CaseInsensitiveString Name => (CaseInsensitiveString)this[nameof(ILogger.Name)];

        public string Message => this.GetValue<StringBuilder>(nameof(Message)).ToString();

        //public static Func<DateTime>

        public static LogEntry New() => CreateLogEntry(LogLevel.Info);

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
