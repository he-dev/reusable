using System.Linq;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo.Expressions
{
    public class Min : AggregateExpression
    {
        public Min()
        : base(nameof(Min), Enumerable.Min)
        { }
    }
}