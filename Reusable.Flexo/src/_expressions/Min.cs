using System.Linq;

namespace Reusable.Flexo
{
    public class Min : AggregateExpression
    {
        public Min()
        : base(nameof(Min), ExpressionContext.Empty, Enumerable.Min)
        { }
    }
}