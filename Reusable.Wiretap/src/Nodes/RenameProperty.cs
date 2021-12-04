using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node renames log properties.
/// </summary>
public class RenameProperty : LoggerNode
{
    public Dictionary<string, string> Mappings { get; set; } = new();

    public override void Invoke(ILogEntry entry)
    {
        foreach (var (key, newName) in Mappings.Select(x => (x.Key, x.Value)))
        {
            if (entry.TryGetProperty<LoggableProperty>(key, out var property))
            {
                entry.Push(new LoggableProperty(newName, property.Value));
            }
        }

        InvokeNext(entry);
    }
}