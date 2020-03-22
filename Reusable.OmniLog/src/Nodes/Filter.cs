using System;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Filters log-entries and short-circuits the pipeline.
    /// </summary>
    public class Filter : LoggerNode
    {
        public Func<ILogEntry, bool> CanLog { get; set; } = _ => true;

        public override void Invoke(ILogEntry request)
        {
            if (CanLog(request))
            {
                InvokeNext(request);
            }
        }
    }
}