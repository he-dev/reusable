using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This node sets default values for the specified properties when they are not set already. 
    /// </summary>
    public class Fallback : LoggerNode
    {
        public override bool Enabled => base.Enabled && Properties.Any();

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public override void Invoke(ILogEntry request)
        {
            foreach (var (key, value) in Properties.Select(x => (x.Key, x.Value)))
            {
                if (!request.TryGetProperty(key, out _))
                {
                    request.Push(key, value, m => m.ProcessWith<Echo>());
                }
            }

            InvokeNext(request);
        }
    }
}