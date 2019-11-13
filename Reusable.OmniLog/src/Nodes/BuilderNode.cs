using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog.Nodes
{
    public class BuilderNode : LoggerNode
    {
//        /// <summary>
//        /// Gets or sets the names of the builders to copy to the log or all if empty.
//        /// </summary>
//        public HashSet<string> Names { get; set; } = new HashSet<string>(SoftString.Comparer);

        protected override void invoke(LogEntry request)
        {
            var builders = 
                request
                    .Action<Copy>()
                    .Select(x => x.Property.ValueOrDefault<ILogEntryBuilder>())
                    .ToList();

            var logEntries =
                from b in builders
                from l in b.Build()
                select l;

            // Copy all items to the main log.
            foreach (var item in logEntries)
            {
                request.Add(item.Key, item.Value);
            }

            invokeNext(request);
        }
    }
}