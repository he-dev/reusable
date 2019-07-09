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
        protected Expression([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        public override IConstant Invoke()
        {
            var parentView = Scope.Context.GetItemOrDefault(ExpressionContext.DebugView);
            var thisView = parentView.Add(CreateDebugView(this));

            // Take a shortcut when this is a constant without an extension. This helps to avoid another debug-view.
            if (this is IConstant constant && Extension is null)
            {
                thisView.Value.Result = constant.Value;
                return constant;
            }

            var @this = GetThisOrDefault(this);

            var thisScope = BeginScope(ctx =>
            {
                ctx = ctx.SetItem(ExpressionContext.DebugView, thisView);
                return
                    @this is null
                        ? ctx
                        : ctx.SetItem(ExpressionContext.This, @this);
            });

            using (thisScope)
            {
                var thisResult = InvokeCore();
                thisView.Value.Result = thisResult.Value;
                @this = thisResult;

                if (Extension is null)
                {
                    return thisResult;
                }

                var extension = GetExtension();

                // Block is transparent so skip any special extension handling.
                if (!(extension is Block))
                {
                    // Validate @this only when this is used as an extension.
                    if (GetThisOrDefault(extension) is var t && t is null)
                    {
                        var extensionType = extension.GetType().GetInterface(typeof(IExtension<>).Name)?.GetGenericArguments().Single();
                        var thisType =
                            thisResult.Value is IEnumerable<IExpression> collection
                                ? collection.GetType()
                                : @this.GetType();

                        if (extensionType?.IsAssignableFrom(thisType) == false)
                        {
                            throw DynamicException.Create
                            (
                                $"ExtensionTypeMismatch",
                                $"Extension's '{extension.GetType().ToPrettyString()}' type '{extensionType.ToPrettyString()}' does not match the expression it is extending which is '{GetType().ToPrettyString()}'."
                            );
                        }
                    }
                    // @this is overriden an this is not used as an extension.
                    else
                    {
                        @this = t;
                    }
                }

                using (BeginScope(ctx => ctx.SetItem(ExpressionContext.DebugView, thisView).SetItem(ExpressionContext.This, @this)))
                {
                    return extension.Invoke();
                }
            }
        }

        [CanBeNull]
        private static object GetThisOrDefault(IExpression expression)
        {
            var isExtension = !(expression.GetType().GetInterface(typeof(IExtension<>).Name) is null);

            var thisValue =
                isExtension
                    ? expression.GetType().GetProperty(nameof(IExtension<object>.This)).GetValue(expression)
                    : default;

            switch (thisValue)
            {
                case null: return default;
                case IExpression e: return e;
                case IEnumerable<IExpression> c: return c;
                default:
                    throw new ArgumentOutOfRangeException
                    (
                        paramName: nameof(IExtension<object>.This),
                        message: $"'This' value is of type '{thisValue.GetType().ToPrettyString()}' " +
                                 $"but it must be either an '{typeof(IExpression).ToPrettyString()}' " +
                                 $"or an '{typeof(IEnumerable<IExpression>).ToPrettyString()}'"
                    );
            }
        }

        private IExpression GetExtension()
        {
            var extension = Extension;

            // Resolve the actual expression.
            while (extension is Ref @ref)
            {
                extension = @ref.Invoke().Value<IExpression>();
            }

            return extension;
        }

        protected abstract Constant<TResult> InvokeCore();

        private static TreeNode<ExpressionDebugView> CreateDebugView(IExpression expression)
        {
            return TreeNode.Create(new ExpressionDebugView
            {
                Type = expression.GetType().ToPrettyString(),
                Name = expression.Name.ToString(),
                Description = expression.Description ?? new ExpressionDebugView().Description,
            });
        }
    }

    public abstract class ValueExtension<TResult> : Expression<TResult>, IExtension<IExpression>
    {
        protected ValueExtension([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        // This property needs to be abstract because it might be renamed so the JsonPropertyAttribute is necessary.
        public abstract IExpression This { get; set; }

        protected override Constant<TResult> InvokeCore()
        {
            var obj = Scope.Context.GetItemOrDefault(ExpressionContext.This);
            var @this =
                obj is IExpression expression
                    ? expression
                    : throw DynamicException.Create("InvalidThisType", "Expected expression.");
            return InvokeCore(@this);
        }

        protected abstract Constant<TResult> InvokeCore(IExpression @this);
    }

    public abstract class CollectionExtension<TResult> : Expression<TResult>, IExtension<IEnumerable<IExpression>>
    {
        protected CollectionExtension([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        // This property needs to be abstract because it might be renamed and needs to be decorated with the JsonPropertyAttribute.
        public abstract IEnumerable<IExpression> This { get; set; }

        protected override Constant<TResult> InvokeCore()
        {
            var obj = Scope.Context.GetItemOrDefault(ExpressionContext.This);
            if (obj is IConstant constant)
            {
                obj = constant.Value;
            }

            var @this =
                obj is IEnumerable<IExpression> collection
                    ? collection
                    : throw DynamicException.Create("InvalidThisType", "Expected collection.");
            return InvokeCore(@this.Enabled());
        }

        protected abstract Constant<TResult> InvokeCore(IEnumerable<IExpression> @this);
    }
}