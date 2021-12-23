using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Services.Properties;

public class Timestamp<T> : PropertyService where T : IDateTime, new()
{
    public Timestamp() => DateTime = new T();

    private IDateTime DateTime { get; }

    public override void Invoke(ILogEntry entry) => entry.Push(new LoggableProperty(nameof(Timestamp), DateTime.Now()));
}