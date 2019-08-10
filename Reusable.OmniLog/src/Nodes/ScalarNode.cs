using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Computes a single value and adds to the log.
    /// </summary>
    public class ScalarNode : LoggerNode
    {
        public ScalarNode() : base(true) { }

        public override bool Enabled => base.Enabled && Functions.Any();

        public List<IScalar> Functions { get; set; } = new List<IScalar>();

        protected override void InvokeCore(LogEntry request)
        {
            foreach (var computable in Functions.Where(x => x.Enabled))
            {
                request.SetItem(computable.Name, default, computable.Compute(request));
            }

            Next?.Invoke(request);
        }
    }
}