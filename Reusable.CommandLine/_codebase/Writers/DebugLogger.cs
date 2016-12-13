using System.Diagnostics;

namespace Reusable.Shelly.Writers
{
    public class DebugLogger : Logger
    {
        protected override void Write(LogEntry logEntry)
        {
            Debug.WriteLine($"{logEntry.LogLevel.ToString().ToUpper()} » {logEntry.Message}");
        }
    }
}
