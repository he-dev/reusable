using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This node passes the received entry to the specified <c>Connectors</c>. It's usually the last node.
    /// </summary>
    public class Echo : LoggerNode
    {
        public override bool Enabled => Connectors?.Any() == true;

        public List<IConnector> Connectors { get; set; } = new List<IConnector>();

        public Func<ILogEntry, ILogEntry> CreateLogEntryView { get; set; } = entry => new LogEntryView<Echo>(entry);

        public override void Invoke(ILogEntry request)
        {
            var view = CreateLogEntryView(request);

            foreach (var rx in Connectors)
            {
                rx.Log(view);
            }

            InvokeNext(request);
        }
    }
}