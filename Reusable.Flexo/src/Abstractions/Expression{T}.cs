using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Wiretap.Abstractions;

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
            return new Constant<TResult>(Id.ToString(), ComputeMany(context), context);
        }

        protected virtual IEnumerable<TResult> ComputeMany(IImmutableContainer context)
        {
            yield return ComputeSingle(context);
        }
        
        protected virtual TResult ComputeSingle(IImmutableContainer context)
        {
            throw new NotImplementedException($"You must override either {nameof(ComputeConstant)} or {nameof(ComputeMany)} method.");
        }

        private class Lambda : Expression<TResult>
        {
            private readonly Func<IImmutableContainer, IEnumerable<TResult>> _invokeAsValue;

            public Lambda(SoftString name, Func<IImmutableContainer, IEnumerable<TResult>> invokeAsValue) : base(default)
            {
                _invokeAsValue = invokeAsValue;
            }

            protected override IEnumerable<TResult> ComputeMany(IImmutableContainer context)
            {
                return _invokeAsValue(context);
            }
        }
    }
}