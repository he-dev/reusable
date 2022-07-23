using System;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Middleware;

/// <summary>
/// This nodes takes care of serializable properties.
/// </summary>
[PublicAPI]
public class SerializeProperties : LoggerMiddleware
{
    public ISerialize Serialize { get; set; } = new SerializeToJson();
    
    public Func<object,  bool> TrySerialize { get; }

    public override void Invoke(ILogEntry entry)
    {
        var update = LogEntry.Empty();
        foreach (var property in entry.WhereTag<ITransientProperty>())
        {
            var obj = Serialize.Invoke(property.Value);
            update.Push(new LogProperty<IRegularProperty>(property.Name, obj));
        }

        entry.Push(update);
        Next?.Invoke(entry);
    }
}