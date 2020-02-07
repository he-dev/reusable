using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Renames log properties.
    /// </summary>
    public class PropertyMapperNode : LoggerNode
    {
        public Dictionary<SoftString, string> Mappings { get; set; } = new Dictionary<SoftString, string>();

        protected override void invoke(ILogEntry request)
        {
            foreach (var (key, value) in Mappings.Select(x => (x.Key, x.Value)))
            {
                if (request.TryGetProperty(key, out var property) && property.CanProcessWith<EchoNode>())
                {
                    request.Add(value!, property.Value, m => m.ProcessWith<EchoNode>());
                }
            }

            invokeNext(request);
        }
    }
}