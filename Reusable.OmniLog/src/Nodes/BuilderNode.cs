using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class BuilderNode : LoggerNode
    {
        public static class DefaultLogEntryItemNames
        {
            public static readonly string Builder = nameof(Builder);
        }

        public static class LogEntryItemTags
        {
            public static readonly string Builder = nameof(Builder);
        }

        public BuilderNode() : base(true) { }

        protected override void InvokeCore(LogEntry request)
        {
            // Do we have any log-entry-builders?
            foreach (var (key, builder) in request.Where(le => le.Key.Tag.Equals(LogEntryItemTags.Builder)).Select(x => (x.Key, (ILogEntryBuilder)x.Value)))
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