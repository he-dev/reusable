using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Adds computable properties to the log.
    /// </summary>
    public class AttachProperty : LoggerNode
    {
        public override bool Enabled => base.Enabled && Properties.Any();

        public List<IPropertyService> Properties { get; set; } = new List<IPropertyService>();

        public override void Invoke(ILogEntry request)
        {
            foreach (var computable in Properties.Where(x => x.Enabled))
            {
                if (computable.GetValue(request) is {} value)
                {
                    request.Push(computable.Name, value, LogProperty.Process.With<Echo>());
                }
            }

            InvokeNext(request);
        }
    }
}