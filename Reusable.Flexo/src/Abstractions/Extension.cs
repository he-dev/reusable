using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo.Abstractions
{
    public interface IExpressionCollection : IEnumerable<IExpression> { }

    public abstract class Extension<TExtension, TResult> : Expression<TResult>, IExtension where TExtension : class
    {
        protected Extension(ILogger? logger) : base(logger) { }

        /// <summary>
        /// Gets or sets the local Arg that overrides the one passed via context.
        /// </summary>
        protected TExtension? Arg { get; set; }

        #region IExtension

        bool IExtension.ArgMustMatch => Arg is null;

        Type IExtension.ExtendsType => typeof(TExtension);

        #endregion

        protected abstract TExtension GetArg(IImmutableContainer context);
    }

    public abstract class ScalarExtension<TResult> : Extension<IExpression, TResult>
    {
        protected ScalarExtension(ILogger? logger) : base(logger) { }

        protected override IExpression GetArg(IImmutableContainer context)
        {
            if (Arg is {})
            {
                return Arg;
            }

            return context.FindItem(ExpressionContext.Arg) is var item && item is IExpression expression ? expression : throw DynamicException.Create("ArgNotFound", $"Could not find {nameof(Arg)} in any context.");
        }

        protected IConstant InvokeArg(IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return GetArg(context).Invoke(context, scope);
        }
    }

    public abstract class CollectionExtension<TResult> : Extension<IEnumerable<IExpression>, TResult>
    {
        protected CollectionExtension(ILogger? logger) : base(logger) { }

        /// <summary>
        /// Gets enabled expressions.
        /// </summary>
        protected override IEnumerable<IExpression> GetArg(IImmutableContainer context)
        {
            if (Arg is {})
            {
                return Arg.Enabled();
            }

            var arg = context.FindItem(ExpressionContext.Arg) is var item && item is IConstant constant ? constant.Value : item;
            return arg is IEnumerable<IExpression> expressions ? expressions.Enabled() : throw DynamicException.Create("ArgNotFound", $"Could not find {nameof(Arg)} in any context.");
        }

        protected IEnumerable<IConstant> InvokeArg(IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return GetArg(context).Select(x => x.Invoke(context, scope));
        }
    }
}