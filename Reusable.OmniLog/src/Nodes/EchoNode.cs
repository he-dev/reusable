using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class EchoNode : LoggerNode
    {
        public override bool Enabled => Rx?.Any() == true; 

        public List<ILogRx> Rx { get; set; } = new List<ILogRx>();

        protected override void invoke(LogEntry request)
        {
            foreach (var rx in Rx)
            {
                rx.Log(request);
            }
            Next?.Invoke(request);
        }
    }
}