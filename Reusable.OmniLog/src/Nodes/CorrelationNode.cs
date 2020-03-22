using System;
using System.Linq;
using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;

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

        public override void Invoke(ILogEntry request)
        {
            var branch = BranchNode.Scope!;
            var scopes = branch.Enumerate().Select(x => x.Value.First.Node<CorrelationNode>()).ToList();
            request.Push(Names.Default.Correlation, scopes, m => m.ProcessWith<SerializerNode>());
            InvokeNext(request);
        }
    }
}