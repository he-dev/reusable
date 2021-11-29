using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes
{
    /// <summary>
    /// This node renames log properties.
    /// </summary>
    public class RenameProperty : LoggerNode
    {
        public Dictionary<string, string> Mappings { get; set; } = new();

        public override void Invoke(ILogEntry entry)
        {
            foreach (var (key, newName) in Mappings.Select(x => (x.Key, x.Value)))
            {
                if (entry.TryGetProperty(key, out var property) && property is LoggableProperty loggableProperty)
                {
                    entry.Push(new LoggableProperty(newName, loggableProperty.Value));
                }
            }

            InvokeNext(entry);
        }
    }
}