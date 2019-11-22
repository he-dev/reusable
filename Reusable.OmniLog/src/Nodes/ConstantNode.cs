using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog.Nodes
{
    public class ConstantNode : LoggerNode
    {
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        protected override void invoke(LogEntry request)
        {
            foreach (var (key, value) in Values.Select(x => (x.Key, x.Value)))
            {
                request.Add<Log>(key!, value);
            }

            invokeNext(request);
        }
    }
}