using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class FilterNode : LoggerNode
    {
        private readonly Func<LogEntry, bool> _canLog;

        public FilterNode(Func<LogEntry, bool> canLog) : base(true)
        {
            _canLog = canLog;
        }

        protected override void InvokeCore(LogEntry request)
        {
            if (_canLog(request))
            {
                Next?.Invoke(request);
            }
        }
    }
}