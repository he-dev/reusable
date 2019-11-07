using System.Linq;
using Reusable.Flexo.Abstractions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Max : Aggregate
    {
        public Max(ILogger<Max> logger) : base(logger, nameof(Max), Enumerable.Max) { }
    }
}