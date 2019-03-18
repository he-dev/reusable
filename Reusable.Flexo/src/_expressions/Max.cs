using System.Linq;

namespace Reusable.Flexo
{
    public class Max : AggregateExpression
    {
        public Max()
            : base(nameof(Max), ExpressionContext.Empty, Enumerable.Max)
        { }
    }
}