using System.Collections;
using System.Collections.Generic;
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

    /// <summary>
    /// Copy constructor that copies only the last version of each property.
    /// </summary>
    public LogEntry(ILogEntry other)
    {
        Properties = other.ToDictionary(x => x.Name, x => new Stack<ILogProperty> { x }, SoftString.Comparer);
    }

    private IDictionary<string, Stack<ILogProperty>> Properties { get; }

    /// <summary>
    /// Creates a new empty entry.
    /// </summary>
    public static LogEntry Empty() => new();

    public ILogProperty this[string name] => TryPeek(name, out var property) ? property : LogProperty.Null.Instance;

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

    public bool TryPeek(string name, out ILogProperty property)
    {
        if (Properties.TryGetValue(name, out var versions) && versions.Peek() is { } latest and not LogProperty.Obsolete)
        {
            property = latest;
            return true;
        }

        property = LogProperty.Null.Instance;
        return false;
    }

    bool ITryGetValue<string, object>.TryGetValue(string key, out object value)
    {
        if (TryPeek(key, out var property) && property.Value is { } result)
        {
            value = result;
            return true;
        }

        value = default!;
        return false;
    }

    public IEnumerable<ILogProperty> Versions(string name)
    {
        return
            Properties.TryGetValue(name, out var versions)
                ? versions
                : Enumerable.Empty<ILogProperty>();
    }

    public override string ToString() => @"{Timestamp:HH:mm:ss:fff} | {Logger} | {Message}".Format(this);

    public IEnumerator<ILogProperty> GetEnumerator()
    {
        return
            Properties
                .Values
                .Select(versions => versions.Peek())
                .Where(latest => latest is not LogProperty.Obsolete)
                .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}