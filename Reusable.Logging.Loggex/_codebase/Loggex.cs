using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Logging.Loggex
{
    public class Loggex : Logger
    {
        private readonly LoggexConfiguration _configuration;

        public Loggex(LoggexConfiguration configuration, string name) : base(name)
        {
            _configuration = configuration;
        }

        protected override void LogCore(LogEntry logEntry)
        {
            throw new NotImplementedException();
        }

        public override bool CanLog(LogEntry logEntry) =>
            !(_configuration.DisabledLogLevels.TryGetValue(Name, out HashSet<LogLevel> logLevels) && logLevels.Contains(logEntry.LogLevel)) &&
            logEntry.LogLevel >= _configuration.LogLevels[Name];
    }
}
