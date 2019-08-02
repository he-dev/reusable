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

        protected TExtension ThisInner { get; set; }

        protected TExtension ThisOuter => ExpressionScope.Current is null ? default : ThisOuterCore();

        #region IExtension

        object IExtension.ThisOuter => ThisOuter;

        bool IExtension.IsInExtensionMode => ThisInner is null;

        Type IExtension.ExtensionType => typeof(TExtension);

        #endregion

        protected abstract TExtension ThisOuterCore();
    }

    public abstract class ValueExtension<TResult> : ExpressionExtension<IExpression, TResult>
    {
        protected ValueExtension([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        protected override IExpression ThisOuterCore()
        {
            return
                Scope.Context.GetItemOrDefault(ExpressionContext.ThisOuter) is IExpression @this
                    ? @this
                    : default;
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