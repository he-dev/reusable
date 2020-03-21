using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Computes a single value and adds to the log.
    /// </summary>
    public class PropertyNode : LoggerNode
    {
        public override bool Enabled => base.Enabled && Properties.Any();

        public List<IService> Properties { get; set; } = new List<IService>();

        public override void Invoke(ILogEntry request)
        {
            foreach (var computable in Properties.Where(x => x.Enabled))
            {
                if (computable.GetValue(request) is {} value)
                {
                    request.Add(computable.Name, value, LogProperty.Process.With<EchoNode>());
                }
            }

            InvokeNext(request);
        }
    }
}