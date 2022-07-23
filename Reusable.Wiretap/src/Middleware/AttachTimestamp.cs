using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachTimestamp<T> : LoggerMiddleware where T : IDateTime, new()
{
    public AttachTimestamp() => DateTime = new T();

    private IDateTime DateTime { get; }

    public override void Invoke(ILogEntry entry)
    {
        entry.Push(new LogProperty<IRegularProperty>("Timestamp", DateTime.Now()));
        Next?.Invoke(entry);
    }
}