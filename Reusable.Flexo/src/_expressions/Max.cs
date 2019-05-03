using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Max : AggregateExpression
    {
        public Max(ILogger<Max> logger) : base(logger, nameof(Max), Enumerable.Max) { }
    }
}