using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes
{
    /// <summary>
    /// This node formats the values of the specified <c>PropertyNames</c> as camel-case.
    /// </summary>
    public class FormatAsCamelCase : LoggerNode
    {
        public override bool Enabled => true;
        
        public HashSet<string> PropertyNames { get; set; } = new(SoftString.Comparer);

        public override void Invoke(ILogEntry entry)
        {
            foreach (var propertyName in PropertyNames)
            {
                if (entry.TryGetProperty(propertyName, out var property) && property?.Value is string value)
                {
                    entry.Push(new LoggableProperty(property.Name, value.ToCamelCase()));
                }
            }

            InvokeNext(entry);
        }
    }
}