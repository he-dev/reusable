using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Computes a single value and adds to the log.
    /// </summary>
    public class ServiceNode : LoggerNode
    {
        public override bool Enabled => base.Enabled && Services.Any();

        public List<IService> Services { get; set; } = new List<IService>();

        protected override void invoke(LogEntry request)
        {
            foreach (var computable in Services.Where(x => x.Enabled))
            {
                request.Add<Log>(computable.Name, computable.GetValue(request));
            }

            invokeNext(request);
        }
    }
}