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
    [Namespace("Flexo", Alias = "F")]
    public abstract class Expression<TResult> : Expression
    {
        protected Expression(ILogger logger, SoftString name) : base(logger, name) { }

        public override IConstant Invoke(IImmutableContainer context)
        {
            var parentView = context.Find(ExpressionContext.DebugView);
            var thisView = parentView.Add(this.CreateDebugView());

            // Take a shortcut when this is a constant without an extension. This helps to avoid another debug-view.
            if (this is IConstant constant && Next is null)
            {
                thisView.Value.Result = constant.Value;
                return constant;
            }

            var thisContext = context.BeginScope
            (
                ImmutableContainer
                    .Empty
                    .SetItem(ExpressionContext.DebugView, thisView)
                //.SetItem(ExpressionContext.ThisOuter, this.ThisOuterOrDefault(), (_, value) => value.IsNotNull())
            );

            var thisResult = InvokeAsConstant(thisContext);
            thisView.Value.Result = thisResult.Value;

            if (Next is IExtension extension && extension.IsInExtensionMode)
            {
                // Check whether result and extension match; do it only for extension expressions.
                var thisType =
                    thisResult.Value is IEnumerable<IExpression> collection
                        ? collection.GetType()
                        : thisResult.GetType();

                if (!extension.ExtensionType.IsAssignableFrom(thisType))
                {
                    throw DynamicException.Create
                    (
                        $"PipeTypeMismatch",
                        $"Extension '{extension.GetType().ToPrettyString()}<{extension.ExtensionType.ToPrettyString()}>' does not match the expression it is extending: '{thisResult.Value.GetType().ToPrettyString()}'."
                    );
                }
            }

            return Next?.Invoke
                   (
                       thisContext,
                       ImmutableContainer
                           .Empty
                           .SetItem(ExpressionContext.DebugView, thisView)
                           .SetItem(ExpressionContext.ThisOuter, thisResult)
                   ) ?? thisResult;
        }

        protected virtual Constant<TResult> InvokeAsConstant(IImmutableContainer context)
        {
            return (Name, InvokeAsValue(context), context);
        }

        protected virtual TResult InvokeAsValue(IImmutableContainer context)
        {
            throw new NotImplementedException($"You must override either {nameof(InvokeAsConstant)} or {nameof(InvokeAsValue)} method.");
        }
    }
}