using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public abstract class ExpressionExtension<TExtension, TResult> : Expression<TResult>, IExtension where TExtension : class
    {
        protected ExpressionExtension([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        /// <summary>
        /// Gets or sets expression's own value that overrides 'ThisOuter'.
        /// </summary>
        protected TExtension ThisInner { get; set; }

        /// <summary>
        /// Gets or sets 'This' passed to this expression when used as an extension.
        /// </summary>
        protected TExtension ThisOuter => ExpressionScope.Current is null ? default : ThisOuterCore();
        
        protected TExtension This
        {
            get => ThisInner ?? ThisOuter;
            set => ThisInner = value;
        }

        #region IExtension

        object IExtension.ThisOuter => ThisOuter;

        bool IExtension.IsInExtensionMode => ThisInner is null;

        Type IExtension.ExtensionType => typeof(TExtension);

        #endregion

        protected abstract TExtension ThisOuterCore();
    }

    public abstract class ScalarExtension<TResult> : ExpressionExtension<IExpression, TResult>
    {
        protected ScalarExtension([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        protected override IExpression ThisOuterCore()
        {
            return
                Scope.Context.GetItemOrDefault(ExpressionContext.ThisOuter) is IExpression @this
                    ? @this
                    : default;
        }

        protected IExpression ThisOrDefault(IImmutableContainer context)
        {
            return context.GetItemOrDefault(ExpressionContext.ThisOuter) switch
            {
                IExpression e => e,
                _ => default
            };
        }
    }

    public abstract class CollectionExtension<TResult> : ExpressionExtension<IEnumerable<IExpression>, TResult>
    {
        protected CollectionExtension([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        protected override IEnumerable<IExpression> ThisOuterCore()
        {
            var @this = Scope.Context.GetItemOrDefault(ExpressionContext.ThisOuter) is var obj && obj is IConstant constant ? constant.Value : obj;

            return
                @this is IEnumerable<IExpression> collection
                    ? collection
                    : default;
        }
    }
}