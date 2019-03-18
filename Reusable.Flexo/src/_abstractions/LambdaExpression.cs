using System;

namespace Reusable.Flexo
{
    public class LambdaExpression : Expression
    {
        private readonly Func<IExpressionContext, IExpression> _invoke;

        public LambdaExpression(string name, IExpressionContext context, Func<IExpressionContext, IExpression> invoke) : base(name, context)
        {
            _invoke = invoke;
        }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            return _invoke(context);
        }

        public static LambdaExpression Predicate(Func<IExpressionContext, CalculateResult<bool>> predicate)
        {
            return new LambdaExpression(nameof(Predicate), ExpressionContext.Empty, context => Constant.FromResult(nameof(Predicate), predicate(context)));
        }

        public static LambdaExpression Double(Func<IExpressionContext, CalculateResult<double>> calculate)
        {
            return new LambdaExpression(nameof(Double), ExpressionContext.Empty, context => Constant.FromResult(nameof(Double), calculate(context)));
        }
    }
}