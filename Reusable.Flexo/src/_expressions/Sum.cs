using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Sum : AggregateExpression
    {
        public Sum(ILogger<Sum> logger) : base(logger, nameof(Sum), Enumerable.Sum) { }
    }
}