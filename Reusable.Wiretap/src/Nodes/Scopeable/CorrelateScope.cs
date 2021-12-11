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
public class CorrelateScope : LoggerNode
{
    private object? _correlationId;

    /// <summary>
    /// Gets or sets the factory for the correlation-id. Uses a continuous GUID by default.
    /// </summary>
    public Func<object> NewCorrelationId { get; set; } = () => Guid.NewGuid().ToString("N");

    [JsonProperty]
    public object CorrelationId
    {
        get => _correlationId ??= NewCorrelationId();
        set => _correlationId = value;
    }

    [JsonProperty]
    public object? CorrelationHandle { get; set; }

    public override void Invoke(ILogEntry entry)
    {
        //if (Prev is { } prev)
        if (entry.TryGetProperty<MetaProperty.Scope, ILogScope>(out var scope))
        {
            //var correlations = prev.EnumeratePrev().OfType<ToggleScope>().Single().Current.Select(x => x.First.Node<Correlate>()).ToList();
            var correlations = scope.Select(x => x.First.Node<CorrelateScope>()).ToList();
            entry.Push(new SerializableProperty.Correlation(correlations));
        }

        InvokeNext(entry);
    }
}