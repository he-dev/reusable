using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node turns objects into dictionaries.
/// </summary>
public class Destructure : LoggerNode
{
    public override void Invoke(ILogEntry entry)
    {
        var properties = LogEntry.Empty();
        var dictionaries =
            from property in entry.Where<DestructibleProperty>()
            select (property, property.Value.GetProperties().ToDictionary(x => x.Name, x => x.Value));

        foreach (var (property, dictionary) in dictionaries)
        {
            properties.Push(new SerializableProperty(property.Name, dictionary));
        }
        
        entry.Merge(properties);
        InvokeNext(entry);
    }
}