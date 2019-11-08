using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public abstract class ExpressionExtension<TExtension, TResult> : Expression<TResult>, IExtension where TExtension : class
    {
        protected ExpressionExtension(ILogger logger) : base(logger) { }

        /// <summary>
        /// Gets or sets expression's own value that overrides 'ThisOuter'.
        /// </summary>
        protected TExtension ThisInner { get; set; }

        #region IExtension

        bool IExtension.IsInExtensionMode => ThisInner is null;

        Type IExtension.ExtensionType => typeof(TExtension);

        #endregion

        protected TExtension This(IImmutableContainer context) => ThisInner ?? ThisOuter(context.FindItem(ExpressionContext.ThisOuter));

        //protected abstract TResult InvokeThis(IImmutableContainer context, IImmutableContainer? scope = default);

        protected abstract TExtension ThisOuter(object thisOuter);
    }

    public abstract class ScalarExtension<TResult> : ExpressionExtension<IExpression, TResult>
    {
        protected ScalarExtension(ILogger logger) : base(logger) { }

        protected override IExpression ThisOuter(object thisOuter)
        {
            return thisOuter switch { IExpression e => e, _ => default };
        }

        protected IConstant InvokeThis(IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return This(context).Invoke(context, scope);
        }
    }

    public abstract class CollectionExtension<TResult> : ExpressionExtension<IEnumerable<IExpression>, TResult>
    {
        protected CollectionExtension(ILogger? logger) : base(logger) { }

        protected override IEnumerable<IExpression> ThisOuter(object thisOuter)
        {
            return thisOuter switch { IConstant c => c.Value, _ => thisOuter } switch
            {
                IEnumerable<IExpression> collection => collection, _ => default
            };
        }

        protected IEnumerable<IConstant> InvokeThis(IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return This(context).Enabled().Select(x => x.Invoke(context, scope));
        }
    }
}