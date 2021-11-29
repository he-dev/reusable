using System;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Collections.Generic;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes
{
    /// <summary>
    /// This nodes provides properties for log-entry correlation.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Correlate : LoggerNode
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
            if (Prev is { } prev)
            {
                var correlations = prev.EnumeratePrev().OfType<ToggleScope>().Single().Current.Select(x => x.First.Node<Correlate>()).ToList();
                entry.Push(new SerializableProperty.Correlation(correlations));
            }
            InvokeNext(entry);
        }
    }
}