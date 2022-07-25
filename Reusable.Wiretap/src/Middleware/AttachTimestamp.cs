using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachTimestamp : LoggerMiddleware
{
    public AttachTimestamp(IDateTime dateTime, string propertyName = "Timestamp")
    {
        DateTime = dateTime;
        PropertyName = propertyName;
    }

    private IDateTime DateTime { get; }

    private string PropertyName { get; }

    public override void Invoke(ILogEntry entry)
    {
        entry.Push(new LogProperty<IRegularProperty>(PropertyName, DateTime.Now()));
        Next?.Invoke(entry);
    }
}