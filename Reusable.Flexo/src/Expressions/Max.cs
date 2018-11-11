using System.Linq;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo.Expressions
{
    public class Max : AggregateExpression
    {
        public Max()
            : base(nameof(Max), Enumerable.Max)
        { }
    }
}