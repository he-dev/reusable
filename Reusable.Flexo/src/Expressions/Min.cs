using System.Linq;

namespace Reusable.Flexo.Expressions
{
    public class Min : AggregateExpression
    {
        public Min()
        : base(nameof(Min), Enumerable.Min)
        { }
    }
}