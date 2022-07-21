using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

public abstract class AttachProperty : LoggerNode { }

public class Attach<T> : AttachProperty where T : ILogPropertyTag
{
    public Attach(ILogProperty<T> property) => Property = property;

    public Attach(string name, object value) : this(new LogProperty<T>(name, value)) { }

    private ILogProperty<T> Property { get; }

    public bool AllowOverwrite { get; set; } = false;

    public override void Invoke(ILogEntry entry)
    {
        if (!entry.ContainsProperty(Property.Name) || AllowOverwrite)
        {
            entry.Push(Property);
        }

        Next?.Invoke(entry);
    }
}