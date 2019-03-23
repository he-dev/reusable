using System;

namespace Reusable.Flexo
{
    public class LambdaExpression<TResult> : Expression<TResult>
    {
        private readonly Func<IExpressionContext, (TResult Value, IExpressionContext Context)> _invoke;

        public LambdaExpression(string name, Func<IExpressionContext, (TResult Value, IExpressionContext Context)> invoke) : base(name)
        {
            _invoke = invoke;
        }

        protected override Constant<TResult> InvokeCore(IExpressionContext context)
        {
            var result = _invoke(context);
            return (Name, result.Value, result.Context);
        }
    }

    public static class LambdaExpression
    {
        public static LambdaExpression<bool> Predicate(Func<IExpressionContext, (bool Value, IExpressionContext Context)> predicate)
        {
            return new LambdaExpression<bool>(nameof(Predicate), predicate);
        }

        public static LambdaExpression<double> Double(Func<IExpressionContext, (double Value, IExpressionContext Context)> calculate)
        {
            return new LambdaExpression<double>(nameof(Double), calculate);
        }
    }
}