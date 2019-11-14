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
    public class ScalarNode : LoggerNode
    {
        public override bool Enabled => base.Enabled && Functions.Any();

        public List<IComputable> Functions { get; set; } = new List<IComputable>();

        protected override void invoke(LogEntry request)
        {
            foreach (var computable in Functions.Where(x => x.Enabled))
            {
                request.Add<Log>(computable.Name, computable.Compute(request));
            }

            invokeNext(request);
        }
    }
}