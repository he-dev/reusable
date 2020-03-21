using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class FallbackNode : LoggerNode
    {
        public override bool Enabled => base.Enabled && Defaults.Any();

        public Dictionary<string, object> Defaults { get; set; } = new Dictionary<string, object>();

        public override void Invoke(ILogEntry request)
        {
            foreach (var (key, value) in Defaults.Select(x => (x.Key, x.Value)))
            {
                if (!request.TryGetProperty(key, out _))
                {
                    request.Add(key, value, m => m.ProcessWith<EchoNode>());
                }
            }

            InvokeNext(request);
        }
    }

    
}