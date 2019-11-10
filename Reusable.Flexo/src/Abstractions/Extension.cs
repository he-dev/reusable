using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo.Abstractions
{
    public interface IExpressionCollection : IEnumerable<IExpression> { }

    public abstract class Extension<TExtension, TResult> : Expression<TResult>, IExtension where TExtension : class
    {
        protected Extension(ILogger logger) : base(logger) { }

        /// <summary>
        /// Gets or sets the local Arg that overrides the one passed via context.
        /// </summary>
        protected TExtension Arg { get; set; }

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
            return Arg switch
            {
                {} a => a, _ => context.FindItem(ExpressionContext.Arg) switch
                {
                    IExpression e => e, _ => default
                }
            };
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
            return (Arg switch
            {
                {} a => a, _ => context.FindItem(ExpressionContext.Arg) switch { IConstant c => c.Value, {} x => x, _ => default } switch
                {
                    IEnumerable<IExpression> collection => collection, _ => default
                }
            })?.Enabled();
        }

        protected IEnumerable<IConstant> InvokeArg(IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return GetArg(context).Select(x => x.Invoke(context, scope));
        }
    }
}