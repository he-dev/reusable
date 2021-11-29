using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Connectors
{
    public class NullConnector : IConnector
    {
        public void Log(ILogEntry logEntry) { }
    }
}