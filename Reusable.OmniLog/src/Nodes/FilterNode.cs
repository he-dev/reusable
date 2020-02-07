using System;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    public class FilterNode : LoggerNode
    {
        private readonly Func<ILogEntry, bool> _canLog;

        public FilterNode(Func<ILogEntry, bool> canLog)
        {
            _canLog = canLog;
        }

        protected override void invoke(ILogEntry request)
        {
            if (_canLog(request))
            {
                invokeNext(request);
            }
        }
    }
}