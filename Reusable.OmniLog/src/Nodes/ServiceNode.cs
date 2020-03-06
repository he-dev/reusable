using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Computes a single value and adds to the log.
    /// </summary>
    public class ServiceNode : LoggerNode
    {
        public override bool Enabled => base.Enabled && Services.Any();

        public List<IService> Services { get; set; } = new List<IService>();

        public override void Invoke(ILogEntry request)
        {
            foreach (var computable in Services.Where(x => x.Enabled))
            {
                request.Add(computable.Name, computable.GetValue(request), LogProperty.Process.With<EchoNode>());
            }

            InvokeNext(request);
        }
    }
}