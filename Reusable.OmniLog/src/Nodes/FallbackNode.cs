using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Sets default values for the specified properties when they are not set already. 
    /// </summary>
    public class FallbackNode : LoggerNode
    {
        public override bool Enabled => base.Enabled && Properties.Any();

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public override void Invoke(ILogEntry request)
        {
            foreach (var (key, value) in Properties.Select(x => (x.Key, x.Value)))
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