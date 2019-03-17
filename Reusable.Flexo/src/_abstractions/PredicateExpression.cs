using JetBrains.Annotations;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class PredicateExpression : Expression
    {
        protected PredicateExpression(string name) : base(name) { }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            //using (context.Scope(this))
            {
                return Constant.FromResult(Name, Calculate(context));
            }
        }

        protected abstract InvokeResult<bool> Calculate(IExpressionContext context);
    }
}