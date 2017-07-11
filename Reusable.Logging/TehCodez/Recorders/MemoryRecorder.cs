using System.Collections.Generic;

namespace Reusable.Loggex.Recorders
{
    public class MemoryRecorder : IRecorder
    {
        public CaseInsensitiveString Name { get; set; } = nameof(MemoryRecorder);

        public List<LogEntry> Logs { get; } = new List<LogEntry>();

        public void Log(LogEntry logEntry)
        {
            Logs.Add(logEntry);
        }
    }
}