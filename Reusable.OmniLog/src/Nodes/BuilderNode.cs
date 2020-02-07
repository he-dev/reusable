using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class BuilderNode : LoggerNode
    {
        protected override void invoke(LogEntry request)
        {
            var builders = request.Properties(m => m.ProcessWith(this)).Select(p => p.Value).Cast<ILogEntryBuilder>().ToList();
            var logEntries =
                from b in builders
                from l in b.Build()
                from v in l.Value
                select v;

            // Copy all items to the main log.
            foreach (var item in logEntries)
            {
                request.Add(item);
            }

            invokeNext(request);
        }
    }
}