using System;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Nodes
{
    /// <summary>
    /// This node filters log-entries and short-circuits the pipeline.
    /// </summary>
    public class FilterEntries : LoggerNode
    {
        public Func<ILogEntry, bool> CanLog { get; set; } = _ => true;

        public override void Invoke(ILogEntry entry)
        {
            if (CanLog(entry))
            {
                InvokeNext(entry);
            }
        }
    }
}