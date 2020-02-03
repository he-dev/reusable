using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public static class LoggerFactoryExtensions
    {
        public static IEnumerable<ILogRx> Receivers(this ILoggerFactory loggerFactory)
        {
            return
                from n in loggerFactory.OfType<EchoNode>()
                from r in n.Rx
                select r;
        }
    }
}