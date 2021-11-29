using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Connectors
{
    public class NullConnector : IConnector
    {
        public void Log(ILogEntry logEntry) { }
    }
}