using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class BuilderNode : LoggerNode
    {
        public override void Invoke(ILogEntry request)
        {
            var builders = request.Where(LogProperty.CanProcess.With(this)).Select(p => p.Value).Cast<ILogEntryBuilder>().ToList();
            var logEntries =
                from b in builders
                from p in b.Build()
                select p;

            // Copy all items to the main log.
            foreach (var item in logEntries)
            {
                request.Add(item);
            }

            InvokeNext(request);
        }
    }
}