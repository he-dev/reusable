using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class EchoNode : LoggerNode
    {
        public override bool Enabled => Rx?.Any() == true;

        public List<ILogRx> Rx { get; set; } = new List<ILogRx>();

        public Func<ILogEntry, ILogEntry> CreateLogEntryView { get; set; } = entry => new LogEntryView<EchoNode>(entry);

        protected override void invoke(ILogEntry request)
        {
            var view = CreateLogEntryView(request);

            foreach (var rx in Rx)
            {
                rx.Log(view);
            }

            Next?.Invoke(request);
        }
    }
}