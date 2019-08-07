using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Adds computed properties to the log.
    /// </summary>
    public class ComputableNode : LoggerNode
    {
        public ComputableNode() : base(true) { }

        public override bool Enabled => base.Enabled && Computables.Any();

        public List<IComputable> Computables { get; set; } = new List<IComputable>();

        protected override void InvokeCore(LogEntry request)
        {
            foreach (var computable in Computables.Where(x => x.Enabled))
            {
                // todo - to catch or not to catch?
                request.SetItem(computable.Name, default, computable.Compute(request));
            }

            Next?.Invoke(request);
        }
    }
}