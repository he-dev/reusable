using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// Adds computable properties to the log.
/// </summary>
public class InvokePropertyService : LoggerNode
{
    public override bool Enabled => Services.Any();

    public List<IPropertyService> Services { get; set; } = new();

    public override void Invoke(ILogEntry entry)
    {
        foreach (var propertyService in Services.Where(x => x.Enabled))
        {
            propertyService.Invoke(entry);
        }

        InvokeNext(entry);
    }
}