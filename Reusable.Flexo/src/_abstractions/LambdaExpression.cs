using System;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class LambdaExpression<TResult> : Expression<TResult>
    {
        private readonly Func<IImmutableSession, (TResult Value, IImmutableSession Context)> _invoke;

        public LambdaExpression(string name, Func<IImmutableSession, (TResult Value, IImmutableSession Context)> invoke) : base(name)
        {
            _invoke = invoke;
        }

        protected override Constant<TResult> InvokeCore(IImmutableSession context)
        {
            var result = _invoke(context);
            return (Name, result.Value, result.Context);
        }
    }

    public static class LambdaExpression
    {
        public static LambdaExpression<bool> Predicate(Func<IImmutableSession, (bool Value, IImmutableSession Context)> predicate)
        {
            return new LambdaExpression<bool>(nameof(Predicate), predicate);
        }

        public static LambdaExpression<double> Double(Func<IImmutableSession, (double Value, IImmutableSession Context)> calculate)
        {
            return new LambdaExpression<double>(nameof(Double), calculate);
        }
    }
}