using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class FallbackNode : LoggerNode
    {
        public override bool Enabled => base.Enabled && Defaults?.Any() == true;

        public Dictionary<SoftString, object> Defaults { get; set; } = new Dictionary<SoftString, object>();

        protected override void invoke(LogEntry request)
        {
            foreach (var (key, value) in Defaults.Select(x => (x.Key, x.Value)))
            {
                if (!request.TryGetProperty(key, m => m.ProcessWith<EchoNode>(), out _))
                {
                    request.Add(key, value, m => m.ProcessWith<EchoNode>());
                }
            }

            invokeNext(request);
        }
    }
}