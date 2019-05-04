using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Min : Aggregate
    {
        public Min(ILogger<Min> logger) : base(logger, nameof(Min), Enumerable.Min) { }
    }
}