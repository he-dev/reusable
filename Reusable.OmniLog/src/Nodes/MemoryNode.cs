using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This node caches logs in memory.
    /// </summary>
    public class MemoryNode : LoggerNode, IEnumerable<LogEntry>
    {
        private readonly LinkedList<LogEntry> _entries = new LinkedList<LogEntry>();

        public int Capacity { get; set; } = 10_000;

        protected override void invoke(LogEntry request)
        {
            lock (_entries)
            {
                _entries.AddLast(request);

                if (_entries.Count > Capacity)
                {
                    _entries.RemoveFirst();
                }
            }

            invokeNext(request);
        }

        public IEnumerator<LogEntry> GetEnumerator() => _entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_entries).GetEnumerator();
    }

    public static class MemoryNodeHelper
    {
        /// <summary>
        /// Activates a new MemoryNode.
        /// </summary>
        public static ILoggerScope UseMemory(this ILoggerScope scope, int capacity = 10_000) => scope.AddNode(new MemoryNode { Capacity = capacity });

        /// <summary>
        /// Gets the MemoryNode in current scope.
        /// </summary>
        public static MemoryNode? Memory(this ScopeNode.FirstNode scope) => scope.EnumerateNext().OfType<MemoryNode>().FirstOrDefault();

        public static DataTable ToDataTable(this MemoryNode memoryNode)
        {
            var dt = new DataTable();
            foreach (var logEntry in memoryNode)
            {
                var row = dt.NewRow();
                foreach (var item in logEntry.Action<Log>())
                {
                    if (!dt.Columns.Contains(item.Name.ToString()))
                    {
                        dt.Columns.Add(item.Name.ToString(), item.Value?.GetType() ?? typeof(object));
                    }

                    row[item.Name.ToString()] = item.Value;
                }

                dt.Rows.Add(row);
            }

            return dt;
        }
    }
}