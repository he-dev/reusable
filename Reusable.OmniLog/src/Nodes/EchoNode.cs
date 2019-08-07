using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class EchoNode : LoggerNode
    {
        public EchoNode() : base(true) { }

        public override bool Enabled => Rx?.Any() == true; 

        public List<ILogRx> Rx { get; set; } = new List<ILogRx>();

        protected override void InvokeCore(LogEntry request)
        {
            // todo - this isn't probably the best place for it
            //            if (!request.ContainsKey("Level"))
            //            {
            //                request.SetItem("Level", LogLevel.Information);
            //            }

            foreach (var rx in Rx)
            {
                rx.Log(request);
            }
        }
    }
}