using System;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes.Scopeable;

/// <summary>
/// This nodes provides properties for log-entry correlation.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Correlation : LoggerMiddleware
{
    private object? _id;

    /// <summary>
    /// Gets or sets the factory for the correlation-id. Uses a continuous GUID by default.
    /// </summary>
    public Func<object> NewId { get; set; } = () => Guid.NewGuid().ToString("N");

    [JsonProperty]
    public object Id
    {
        get => _id ??= NewId();
        set => _id = value;
    }

    [JsonProperty]
    public object? Name { get; set; }

    public override void Invoke(ILogEntry entry)
    {
        //if (entry.TryPeek<IMetaProperty, ILoggerScope>(nameof(LoggerScope), out var loggerScope))
        //if (entry.TryPeek(nameof(LoggerScope), out var property) && property.TryCast<IMetaProperty, ILoggerScope>(out var loggerScope))
        if (entry[LogProperty.Names.LoggerScope()].Value is ILoggerScope loggerScope)
        {
            var correlations = loggerScope.Parents.Select(x => x.Correlation()).ToList();
            entry.Push(new LogProperty<ITransientProperty>(LogProperty.Names.Correlation(), correlations));
        }

        Next?.Invoke(entry);
    }
}