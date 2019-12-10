using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo.Abstractions
{
    [PublicAPI]
    public abstract class Expression<TResult> : Expression
    {
        protected Expression(ILogger? logger) : base(logger) { }

        public static IExpression Create(string id, Func<IImmutableContainer, IEnumerable<TResult>> invokeAsValue)
        {
            return new Lambda(id!, invokeAsValue);
        }

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            return new Constant<TResult>(Id.ToString(), ComputeValues(context), context);
        }

        protected virtual IEnumerable<TResult> ComputeValues(IImmutableContainer context)
        {
            //throw new NotImplementedException($"You must override either {nameof(ComputeConstantGeneric)} or {nameof(ComputeValues)} method.");
            yield return ComputeValue(context);
        }
        
        protected virtual TResult ComputeValue(IImmutableContainer context)
        {
            throw new NotImplementedException($"You must override either {nameof(ComputeConstant)} or {nameof(ComputeValues)} method.");
        }

        private class Lambda : Expression<TResult>
        {
            private readonly Func<IImmutableContainer, IEnumerable<TResult>> _invokeAsValue;

            public Lambda(SoftString name, Func<IImmutableContainer, IEnumerable<TResult>> invokeAsValue) : base(default)
            {
                _invokeAsValue = invokeAsValue;
            }

            protected override IEnumerable<TResult> ComputeValues(IImmutableContainer context)
            {
                return _invokeAsValue(context);
            }
        }
    }
}