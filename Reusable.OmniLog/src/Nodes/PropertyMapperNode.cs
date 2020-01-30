using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Renames log properties.
    /// </summary>
    public class PropertyMapperNode : LoggerNode
    {
        public Dictionary<SoftString, string> Mappings { get; set; } = new Dictionary<SoftString, string>();

        protected override void invoke(LogEntry request)
        {
            foreach (var (key, value) in Mappings.Select(x => (x.Key, x.Value)))
            {
                if (request.TryGetProperty<Log>(key, out var property))
                {
                    request.Add<Delete>(key!, default);
                    request.Add<Log>(value!, property.Value);
                }
            }

            invokeNext(request);
        }
    }
}