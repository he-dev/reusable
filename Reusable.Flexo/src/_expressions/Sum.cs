using System.Linq;

namespace Reusable.Flexo
{
    public class Sum : AggregateExpression
    {
        public Sum()
        : base(nameof(Sum), Enumerable.Sum)
        { }
    }
}