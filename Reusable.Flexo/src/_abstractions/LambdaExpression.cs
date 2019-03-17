using System;

namespace Reusable.Flexo
{
    public class LambdaExpression : Expression
    {
        private readonly Func<IExpressionContext, IExpression> _invoke;

        public LambdaExpression(string name, Func<IExpressionContext, IExpression> invoke) : base(name)
        {
            _invoke = invoke;
        }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            return _invoke(context);
        }

        public static LambdaExpression Predicate(Func<IExpressionContext, InvokeResult<bool>> predicate)
        {
            return new LambdaExpression(nameof(Predicate), context => Constant.FromResult(nameof(Predicate), predicate(context)));
        }

        public static LambdaExpression Double(Func<IExpressionContext, InvokeResult<double>> calculate)
        {
            return new LambdaExpression(nameof(Double), context => Constant.FromResult(nameof(Double), calculate(context)));
        }
    }
}