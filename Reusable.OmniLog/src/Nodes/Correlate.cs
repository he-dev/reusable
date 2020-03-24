using System;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog.Nodes
{
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

        public override void Invoke(ILogEntry request)
        {
            var correlations = Prev.EnumeratePrev().OfType<ToggleScope>().Single().Current.Select(x => x.First.Node<Correlate>()).ToList();
            request.Push(Names.Properties.Correlation, correlations, m => m.ProcessWith<SerializeProperty>());
            InvokeNext(request);
        }
    }
}