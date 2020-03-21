using System;
using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CorrelationNode : LoggerNode
    {
        private object? _correlationId;

        /// <summary>
        /// Gets or sets the factory for the default correlation-id. By default it's a Guid.
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

        //[JsonProperty]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public override void Invoke(ILogEntry request) => InvokeNext(request);
    }
}