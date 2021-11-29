using System.Linq;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This nodes takes care of serializable properties.
/// </summary>
[PublicAPI]
public class SerializeProperty : LoggerNode
{
    public ISerialize Serialize { get; set; } = new SerializeToJson();

    public override void Invoke(ILogEntry entry)
    {
        foreach (var property in entry.Where<SerializableProperty>().ToList())
        {
            var obj = Serialize.Invoke(property.Value);
            entry.Push(new LoggableProperty(property.Name, obj));
        }

        InvokeNext(entry);
    }
}