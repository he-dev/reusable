using System.Diagnostics;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog.Rx
{
    public class DebugRx : ILogRx
    {
        private DebugRx() { }

        public string Template { get; set; } = @"[{Timestamp:HH:mm:ss:fff}] [{Logger:u}] {Message}";

        public void Log(LogEntry logEntry)
        {
            Debug.WriteLine(Template.Format(logEntry));
        }

        public static ILogRx Create()
        {
            return new DebugRx();
        }
    }
}