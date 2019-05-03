using System;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class LambdaExpression<TResult> : Expression<TResult>
    {
        private readonly Func<TResult> _invoke;

        public LambdaExpression(string name, Func<TResult> invoke) : base(name)
        {
            _invoke = invoke;
        }

        protected override Constant<TResult> InvokeCore()
        {
            var result = _invoke();
            return (Name, result);
        }
    }

    public static class LambdaExpression
    {
        public static LambdaExpression<bool> Predicate(Func<bool> predicate)
        {
            return new LambdaExpression<bool>(nameof(Predicate), predicate);
        }

        public static LambdaExpression<double> Double(Func<double> calculate)
        {
            return new LambdaExpression<double>(nameof(Double), calculate);
        }
    }
}