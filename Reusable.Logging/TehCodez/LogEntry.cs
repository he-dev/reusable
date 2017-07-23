using System.Collections.Generic;
using Reusable.Logging.Loggex.ComputedProperties;

namespace Reusable.Logging.Loggex
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
}
