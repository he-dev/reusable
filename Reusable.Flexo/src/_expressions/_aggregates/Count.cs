using System.Linq;

namespace Reusable.Flexo
{
    public class Count : AggregateExpression
    {
        public Count()
            // The aggregate works with doubles and it does need it for counting too.
            : base(nameof(Count), items => items.Select(_ => 0.0).Count())
        { }
    }
}