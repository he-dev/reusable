using System.Linq;
using Reusable.Flexo.Abstractions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Sum : Aggregate
    {
        public Sum(ILogger<Sum> logger) : base(logger, Enumerable.Sum) { }
    }
}