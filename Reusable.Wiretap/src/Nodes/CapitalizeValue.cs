using System.Collections.Generic;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes;

public class CapitalizeValue : LoggerNode
{
    public override bool Enabled => true;
        
    public HashSet<string> PropertyNames { get; set; } = new(SoftString.Comparer);

    public override void Invoke(ILogEntry entry)
    {
        foreach (var propertyName in PropertyNames)
        {
            if (entry.TryGetProperty(propertyName, out var property) && property?.Value is string value)
            {
                entry.Push(new LoggableProperty(property.Name, value.Capitalize()));
            }
        }

        InvokeNext(entry);
    }
}