using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public abstract class ExpressionExtension<TExtension, TResult> : Expression<TResult>, IExtension where TExtension : class
    {
        protected ExpressionExtension(ILogger logger, SoftString name) : base(logger, name) { }

        /// <summary>
        /// Gets or sets expression's own value that overrides 'ThisOuter'.
        /// </summary>
        protected TExtension ThisInner { get; set; }

        #region IExtension

        bool IExtension.IsInExtensionMode => ThisInner is null;

        Type IExtension.ExtensionType => typeof(TExtension);

        #endregion

        protected TExtension This(IImmutableContainer context) => ThisInner ?? ThisOuter(context.GetItemOrDefault(ExpressionContext.ThisOuter));

        protected abstract TExtension ThisOuter(object thisOuter);
    }

    public abstract class ScalarExtension<TResult> : ExpressionExtension<IExpression, TResult>
    {
        protected ScalarExtension(ILogger? logger, SoftString name) : base(logger, name) { }

        protected override IExpression ThisOuter(object thisOuter)
        {
            return thisOuter switch
            {
                IExpression e => e,
                _ => default
            };
        }
    }

    public abstract class CollectionExtension<TResult> : ExpressionExtension<IEnumerable<IExpression>, TResult>
    {
        protected CollectionExtension(ILogger? logger, SoftString name) : base(logger, name) { }

        protected override IEnumerable<IExpression> ThisOuter(object thisOuter)
        {
            thisOuter = thisOuter switch { IConstant c => c.Value, _ => thisOuter };
            return thisOuter switch { IEnumerable<IExpression> collection => collection, _ => default };
        }
    }
}