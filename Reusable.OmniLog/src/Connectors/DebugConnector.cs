using System.Diagnostics;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Connectors
{
    public class DebugConnector : IConnector
    {
        private DebugConnector() { }

        public string Template { get; set; } = @"[{Timestamp:HH:mm:ss:fff}] [{Logger}] {Message}";

        public void Log(ILogEntry logEntry)
        {
            Debug.WriteLine(Template.Format(logEntry));
        }

        public static IConnector Create()
        {
            return new DebugConnector();
        }
    }
}