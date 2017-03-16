using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Logging.Tests.Mocks
{
    class MockLogger : Logger
    {
        public MockLogger() : base(nameof(MockLogger)) { }

        public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

        public override bool CanLog(LogEntry logEntry)
        {
            return true;
        }

        protected override void LogCore(LogEntry logEntry)
        {
            LogEntries.Add(logEntry);
        }
    }
}
