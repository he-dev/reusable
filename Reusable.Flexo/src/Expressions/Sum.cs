using System.Linq;

namespace Reusable.Flexo.Expressions
{
    public class Sum : AggregateExpression
    {
        public Sum()
        : base(nameof(Sum), Enumerable.Sum)
        { }
    }
}