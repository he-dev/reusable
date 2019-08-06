using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    // Reroutes items from one property to the other: Meta#Dump --> Snapshot#Dump 
    public class RouteNode : LoggerNode
    {
        public RouteNode() : base(true) { }

        public IDictionary<string, string> Routes { get; set; } = new Dictionary<string, string>();

        protected override void InvokeCore(LogEntry request)
        {
            foreach (var route in Routes)
            {
                if (request.TryGetItem<object>(route.Key, default, out var item))
                {
                    request.SetItem(route.Value, default, item);
                    request.RemoveItem(route.Key, default);
                }
            }
            Next?.Invoke(request);
        }
    }
}