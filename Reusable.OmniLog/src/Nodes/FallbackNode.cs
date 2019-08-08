using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class FallbackNode : LoggerNode
    {
        public FallbackNode() : base(true) { }

        public override bool Enabled => base.Enabled && Defaults?.Any() == true;

        public Dictionary<SoftString, object> Defaults { get; set; } = new Dictionary<SoftString, object>();

        protected override void InvokeCore(LogEntry request)
        {
            foreach (var item in Defaults)
            {
                if (!request.TryGetItem<object>(item.Key, default, out _))
                {
                    request.SetItem(item.Key, default, item.Value);
                }
            }
            Next?.Invoke(request);
        }
    }
}