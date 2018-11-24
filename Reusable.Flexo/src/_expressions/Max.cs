using System.Linq;

namespace Reusable.Flexo
{
    public class Max : AggregateExpression
    {
        public Max()
            : base(nameof(Max), Enumerable.Max)
        { }
    }
}