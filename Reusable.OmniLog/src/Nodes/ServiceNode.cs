using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
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
                request.Add(computable.Name, computable.GetValue(request), m => m.ProcessWith<EchoNode>());
            }

            InvokeNext(request);
        }
    }

    public class CamelCaseNode : LoggerNode
    {
        public override bool Enabled => true;

        public override void Invoke(ILogEntry request)
        {
            foreach (var item in request.Where(x => SoftString.Comparer.Equals(x.Name, LogProperty.Names.SnapshotName)).ToList())
            {
                // Format snapshot-name with camel-case.
                request.Add(item.Name, ((string)item.Value).ToCamelCase(), m => m.ProcessWith<EchoNode>());
            }

            InvokeNext(request);
        }
    }
}