using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes
{
    /// <summary>
    /// Adds computable properties to the log.
    /// </summary>
    public class AttachProperty : LoggerNode
    {
        public override bool Enabled => base.Enabled && Properties.Any();

        public List<IPropertyService> Properties { get; set; } = new();

        public override void Invoke(ILogEntry entry)
        {
            foreach (var computable in Properties.Where(x => x.Enabled))
            {
                if (computable.GetValue(entry) is {} value)
                {
                    entry.Push(new LoggableProperty(computable.Name, value));
                }
            }

            InvokeNext(entry);
        }
    }
}