using System.Linq;
using Reusable.Flexo.Abstractions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Count : Aggregate
    {
        // The aggregate works with doubles and it does need it for counting too.
        public Count(ILogger<Count> logger) : base(logger, nameof(Count), items => items.Select(_ => 0.0).Count()) { }        
    }
}