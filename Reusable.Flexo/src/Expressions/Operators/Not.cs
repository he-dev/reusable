using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Extensions;

namespace Reusable.Flexo.Expressions.Operators
{
    public class Not : PredicateExpression
    {
        public Not() : base(nameof(Not)) { }

        public IExpression Expression { get; set; }

        protected override bool Calculate(IExpressionContext context) => !Expression.SafeInvoke(context).Value<bool>();
    }
}