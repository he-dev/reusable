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
        private LogEntry() { }

        public static LogEntry Create(LogLevel logLevel = LogLevel.Info)
        {
            return
                new LogEntry()
                    .LogLevel(logLevel)
                    .SetProperty(new Now())
                    .SetProperty(new UtcNow())
                    .SetProperty(new ThreadId());
        }
    }

    //public class AutoLogEntry : LogEntry, IDisposable
    //{
    //    private readonly ILogger _logger;

    //    public AutoLogEntry(LogEntry logEntry, ILogger logger) : base(logEntry) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    //    public void Dispose() => this.Log(_logger);
    //}
}
