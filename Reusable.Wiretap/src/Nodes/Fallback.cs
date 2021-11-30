using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes
{
    /// <summary>
    /// This node sets default values for the specified properties when they are not set already. 
    /// </summary>
    public class Fallback : LoggerNode
    {
        public override bool Enabled => base.Enabled && Properties.Any();

        public Dictionary<string, object> Properties { get; set; } = new();

        public override void Invoke(ILogEntry entry)
        {
            foreach (var (key, value) in Properties)
            {
                if (!entry.TryGetProperty(key, out _))
                {
                    entry.Push(new LoggableProperty(key, value));
                }
            }

            InvokeNext(entry);
        }
    }
}