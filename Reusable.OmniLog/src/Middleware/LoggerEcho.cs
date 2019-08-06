using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Middleware
{
    public class LoggerEcho : LoggerMiddleware
    {
        public LoggerEcho() : base(true) { }

        public override bool IsActive => Receivers?.Any() == true; 

        public List<ILogRx> Receivers { get; set; } = new List<ILogRx>();

        protected override void InvokeCore(LogEntry request)
        {
            // todo - this isn't probably the best place for it
            //            if (!request.ContainsKey("Level"))
            //            {
            //                request.SetItem("Level", LogLevel.Information);
            //            }

            foreach (var rx in Receivers)
            {
                rx.Log(request);
            }
        }
    }
}