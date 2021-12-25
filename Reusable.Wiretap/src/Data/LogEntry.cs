using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

[PublicAPI]
public class LogEntry : ILogEntry
{
    [DebuggerStepThrough]
    public LogEntry()
    {
        Properties = new Dictionary<string, Stack<ILogProperty>>(SoftString.Comparer);
    }

    private LogEntry(LogEntry other)
    {
        Properties = new Dictionary<string, Stack<ILogProperty>>(other.Properties, SoftString.Comparer);
    }

    private IDictionary<string, Stack<ILogProperty>> Properties { get; }

    public static LogEntry Empty() => new();

    public ILogProperty this[string name] =>
        TryGetProperty(name, out var property)
            ? property!
            : throw DynamicException.Create("PropertyNotFound", $"There is no property with the name '{name}'.");

    public ILogEntry Push(ILogProperty property)
    {
        if (Properties.TryGetValue(property.Name, out var versions))
        {
            versions.Push(property);
        }
        else
        {
            Properties[property.Name] = new Stack<ILogProperty> { property };
        }

        return this;
    }

    public bool TryGetProperty(string name, out ILogProperty property)
    {
        if (Properties.TryGetValue(name, out var versions))
        {
            property = versions.Peek();
            return true;
        }
        else
        {
            property = default!;
            return false;
        }
    }

    bool ITryGetValue<string, object>.TryGetValue(string key, out object value)
    {

        if (TryGetProperty(key, out var property) && property.Value is { } result)
        {
            value = result;
            return true;
        }

        value = default!;
        return false;
    }

    public override string ToString() => @"{Timestamp:HH:mm:ss:fff} | {Logger} | {Message}".Format(this);

    public IEnumerator<ILogProperty> GetEnumerator() => Properties.Values.Select(versions => versions.Peek()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}