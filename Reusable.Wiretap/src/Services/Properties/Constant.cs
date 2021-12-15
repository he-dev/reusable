using Reusable.Data;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Services.Properties;

public class Constant : PropertyService
{
    public Constant(string name, object value) => Property = new LoggableProperty(name, value);
    
    public Constant(ILogProperty property) => Property = property;

    private ILogProperty Property { get; }

    public override void Invoke(ILogEntry entry) => entry.Push(Property);
}