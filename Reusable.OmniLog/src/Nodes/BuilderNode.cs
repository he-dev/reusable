using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class BuilderNode : LoggerNode
    {
        /// <summary>
        /// Gets or sets the names of the builders to copy to the log or all if empty.
        /// </summary>
        public HashSet<string> Names { get; set; } = new HashSet<string>(SoftString.Comparer);

        protected override void invoke(LogEntry request)
        {
            var builders =
                request
                    .Keys()
                    .Where(k => k.Tag.Equals(LogEntry.Tags.Copyable))
                    .Select(x => request[x])
                    .Cast<ILogEntryBuilder>();

            // Use only specific names.
            if (Names.Any())
            {
                builders =
                    Names
                        .Where(builderName => request.ContainsKey(builderName, LogEntry.Tags.Copyable))
                        .Select(builderName => request[builderName, LogEntry.Tags.Copyable])
                        .Cast<ILogEntryBuilder>();
            }

            builders = builders.ToList();

            if (builders.Any())
            {
                var logEntries =
                    from b in builders
                    from l in b.Build()
                    select l;

                // Copy all items to the main log.
                foreach (var item in logEntries)
                {
                    request.SetItem(item.Key.Name, item.Key.Tag, item.Value);
                }
            }

            Next?.Invoke(request);
        }
    }
}