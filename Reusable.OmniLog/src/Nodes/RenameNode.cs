using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    // Reroutes items from one property to the other: Meta#Dump --> Snapshot#Dump 
    public class RenameNode : LoggerNode
    {
        public RenameNode() : base(true) { }

        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();

        protected override void InvokeCore(LogEntry request)
        {
            foreach (var route in Mappings.Where(x => !SoftString.Comparer.Equals(x.Key, x.Value)))
            {
                if (request.TryGetItem<object>(route.Key, default, out var item))
                {
                    request.RemoveItem(route.Key, default);
                    request.SetItem(route.Value, default, item);
                }
            }
            Next?.Invoke(request);
        }
    }
}