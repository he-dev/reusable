using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class BuilderNode : LoggerNode
    {
        public BuilderNode() : base(true) { }

        public HashSet<string> BuilderItems { get; set; } = new HashSet<string>(SoftString.Comparer);

        protected override void InvokeCore(LogEntry request)
        {
            var builders = 
                request
                    .Keys()
                    .Where(k => k.Tag.Equals(LogEntry.Tags.Copyable))
                    .Select(x => request[x])
                    .Cast<ILogEntryBuilder>();

            if (BuilderItems.Any())
            {
                builders = 
                    BuilderItems
                        .Where(builderName => request.ContainsKey(builderName, LogEntry.Tags.Copyable))
                        .Select(builderName => request[builderName, LogEntry.Tags.Copyable])
                        .Cast<ILogEntryBuilder>();
            }

            // Do we have any log-entry-builders?
            foreach (var builder in builders.ToList())
            {
                var logEntry = builder.Build();

                // Copy all items to the main log.
                foreach (var item in logEntry)
                {
                    request.SetItem(item.Key.Name, item.Key.Tag, item.Value);
                }
            }

            Next?.Invoke(request);
        }
    }
}