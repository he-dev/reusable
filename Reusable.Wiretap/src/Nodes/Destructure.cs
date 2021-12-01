using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Utilities;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node turns objects into dictionaries.
/// </summary>
public class Destructure : LoggerNode
{
    public override void Invoke(ILogEntry entry)
    {
        var dictionaries =
            //from property in request.Where(LogProperty.CanProcess.With(this))
            //from property in entry.Where(LogProperty2.Is<DestructibleProperty>())
            from property in entry.Where<DestructibleProperty>()
            select (property, property.Value.ToDictionary());

        foreach (var (property, dictionary) in dictionaries.ToList())
        {
            //request.Push<Destructure>(property.Name, dictionary); //, LogProperty.Process.With<SerializeProperty>());
            //request.Push<SerializeProperty>(property.Name, dictionary); //, LogProperty.Process.With<SerializeProperty>());
            entry.Push(new SerializableProperty(property.Name, dictionary));
        }

        InvokeNext(entry);
    }
}