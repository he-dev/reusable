using System.Diagnostics;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Connectors;

public class DebugConnector : IConnector
{
    public string Template { get; set; } = "[{Timestamp:HH:mm:ss:fff}] [{Logger}] {Message}";

    public void Log(ILogEntry logEntry)
    {
        Debug.WriteLine(Template.Format(logEntry));
    }
}