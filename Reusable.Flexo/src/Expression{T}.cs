using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    
    public abstract class Expression<TResult> : Expression
    {
        protected Expression(ILogger? logger, SoftString name) : base(logger, name) { }

        public static IExpression Create(string name, Func<IImmutableContainer, TResult> invokeAsValue) => new Lambda(name, invokeAsValue);

        protected override IConstant ComputeConstant(IImmutableContainer context) => ComputeConstantGeneric(context);

        protected virtual Constant<TResult> ComputeConstantGeneric(IImmutableContainer context)
        {
            return Constant.FromValue(Name, ComputeValue(context), context);
        }

        protected virtual TResult ComputeValue(IImmutableContainer context)
        {
            throw new NotImplementedException($"You must override either {nameof(ComputeConstantGeneric)} or {nameof(ComputeValue)} method.");
        }

        private class Lambda : Expression<TResult>
        {
            private readonly Func<IImmutableContainer, TResult> _invokeAsValue;

            public Lambda(SoftString name, Func<IImmutableContainer, TResult> invokeAsValue) : base(default, name)
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