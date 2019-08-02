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
    [Namespace("Flexo", Alias = "F")]
    public abstract class Expression<TResult> : Expression
    {
        protected Expression([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        public override IConstant Invoke()
        {
            var parentView = Scope.Context.GetItemOrDefault(ExpressionContext.DebugView);
            var thisView = parentView.Add(this.CreateDebugView());

            // Take a shortcut when this is a constant without an extension. This helps to avoid another debug-view.
            if (this is IConstant constant && Extension is null)
            {
                thisView.Value.Result = constant.Value;
                return constant;
            }

            using (BeginScope(ctx => ctx
                .SetItem(ExpressionContext.DebugView, thisView)
                .SetItemWhen(ExpressionContext.This, this.ThisOuterOrDefault(), (_, value) => value.IsNotNull()))
            )
            {
                var thisResult = InvokeCore();
                thisView.Value.Result = thisResult.Value;

                if (Extension is null)
                {
                    return thisResult;
                }
                else
                {
                    var extension = Extension.Resolve();

                    // Check whether result and extension match; do it only for extension expressions.
                    if (extension is IExtension ext && ext.IsInExtensionMode)
                    {
                        var thisType =
                            thisResult.Value is IEnumerable<IExpression> collection
                                ? collection.GetType()
                                : thisResult.GetType();

                        if (!ext.ExtensionType.IsAssignableFrom(thisType))
                        {
                            throw DynamicException.Create
                            (
                                $"ExtensionTypeMismatch",
                                $"Extension '{ext.GetType().ToPrettyString()}<{ext.ExtensionType.ToPrettyString()}>' does not match the expression it is extending: '{thisResult.Value.GetType().ToPrettyString()}'."
                            );
                        }
                    }

                    using (BeginScope(ctx => ctx
                        .SetItem(ExpressionContext.DebugView, thisView)
                        .SetItem(ExpressionContext.This, thisResult)))
                    {
                        return extension.Invoke();
                    }
                }
            }
        }

        protected abstract Constant<TResult> InvokeCore();
    }
}