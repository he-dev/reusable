using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This node renames log properties.
    /// </summary>
    public class RenameProperty : LoggerNode
    {
        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();

        public override void Invoke(ILogEntry request)
        {
            foreach (var (key, value) in Mappings.Select(x => (x.Key, x.Value)))
            {
                if (request.TryGetProperty(key, out var property) && property.CanProcessWith<Echo>())
                {
                    request.Push(value, property.Value, m => m.ProcessWith<Echo>());
                }
            }

            InvokeNext(request);
        }
    }
}