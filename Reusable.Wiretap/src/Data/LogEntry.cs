using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

[PublicAPI]
public class LogEntry : ILogEntry
{
    private readonly IDictionary<string, IImmutableStack<ILogProperty>> _properties;

    [DebuggerStepThrough]
    public LogEntry()
    {
        _properties = new Dictionary<string, IImmutableStack<ILogProperty>>(SoftString.Comparer);
    }

    private LogEntry(LogEntry other)
    {
        _properties = new Dictionary<string, IImmutableStack<ILogProperty>>(other._properties, SoftString.Comparer);
    }

    public static LogEntry Empty() => new();

    public ILogProperty this[string name] => 
        TryGetProperty(name, out var property)
            ? property!
            : throw DynamicException.Create("PropertyNotFound", $"There is no property with the name '{name}'.");

    public ILogEntry Push(ILogProperty property)
    {
        var current = _properties.TryGetValue(property.Name, out var versions) ? versions : ImmutableStack<ILogProperty>.Empty;
        _properties[property.Name] = current.Push(property);
        return this;
    }

    public bool TryGetProperty(string name, out ILogProperty? property)
    {
        if (_properties.TryGetValue(name, out var versions))
        {
            property = versions.Peek();
            return true;
        }
        else
        {
            property = default;
            return false;
        }
    }

    public override string ToString()
    {
        return @"[{Timestamp:HH:mm:ss:fff}] [{Logger}] {Message}".Format(this);
    }

    public IEnumerator<ILogProperty> GetEnumerator() => _properties.Values.Select(versions => versions.Peek()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}