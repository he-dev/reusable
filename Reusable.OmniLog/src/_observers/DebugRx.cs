using System.Diagnostics;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class DebugRx : ILogRx
    {
        private DebugRx() { }

        public void Log(LogEntry logEntry)
        {
            Debug.WriteLine(logEntry);
        }

        public static ILogRx Create()
        {
            return new DebugRx();
        }
    }
}