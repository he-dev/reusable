using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This nodes formats the specified <c>Identifiers</c> as camel-case.
    /// </summary>
    public class FormatAsCamelCase : LoggerNode
    {
        public override bool Enabled => true;

        public HashSet<string> Identifiers { get; set; } = new HashSet<string>(SoftString.Comparer);

        public override void Invoke(ILogEntry request)
        {
            foreach (var propertyName in Identifiers)
            {
                if (request.TryGetProperty(propertyName, out var property) && property.Value is string value)
                {
                    request.Push(property.Name, value.ToCamelCase(), LogProperty.Process.With<Echo>());
                }
            }

            InvokeNext(request);
        }
    }
}