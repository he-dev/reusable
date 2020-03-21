using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Copies everything from log-entry builder to the log.
    /// </summary>
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
                request.Push(item);
            }

            InvokeNext(request);
        }
    }
}