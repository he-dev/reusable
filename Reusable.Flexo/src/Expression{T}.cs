using System;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    
    public abstract class Expression<TResult> : Expression
    {
        protected Expression(ILogger logger) : base(logger) { }

        public static IExpression Create(string name, Func<IImmutableContainer, TResult> invokeAsValue) => new Lambda(name, invokeAsValue);

        protected override IConstant ComputeConstant(IImmutableContainer context) => ComputeConstantGeneric(context);

        protected virtual Constant<TResult> ComputeConstantGeneric(IImmutableContainer context)
        {
            return Constant.FromValue(Id, ComputeValue(context), context);
        }

        protected virtual TResult ComputeValue(IImmutableContainer context)
        {
            throw new NotImplementedException($"You must override either {nameof(ComputeConstantGeneric)} or {nameof(ComputeValue)} method.");
        }

        private class Lambda : Expression<TResult>
        {
            private readonly Func<IImmutableContainer, TResult> _invokeAsValue;

            public Lambda(SoftString name, Func<IImmutableContainer, TResult> invokeAsValue) : base(default)
            {
                _invokeAsValue = invokeAsValue;
            }

            protected override TResult ComputeValue(IImmutableContainer context)
            {
                return _invokeAsValue(context);
            }
        }
    }
}