using System.Linq;

namespace Reusable.Flexo
{
    public class Count : AggregateExpression
    {
        public Count()
            : base(nameof(Count), ExpressionContext.Empty, items => items.Count()) { }
    }
}