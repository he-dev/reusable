using System.Collections.Generic;

namespace Reusable.Logging.Loggex.Recorders
{
    public class MemoryRecorder : IRecorder
    {
        public CaseInsensitiveString Name { get; set; } = "Memory";

        public List<LogEntry> Logs { get; } = new List<LogEntry>();

        public void Log(LogEntry logEntry)
        {
            Logs.Add(logEntry);
        }
    }
}