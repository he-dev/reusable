using Reusable.Flexo.Extensions;

namespace Reusable.Flexo.Expressions
{
    public class Not : PredicateExpression
    {
        public Not() : base(nameof(Not)) { }

        public IExpression Expression { get; set; }

        protected override bool Calculate(IExpressionContext context) => !Expression.InvokeWithValidation(context).Value<bool>();
    }
}