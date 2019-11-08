using System.Linq;
using Reusable.Flexo.Abstractions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Min : Aggregate
    {
        public Min(ILogger<Min> logger) : base(logger, Enumerable.Min) { }
    }
}