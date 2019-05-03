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
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Namespace("Flexo")]
    public abstract class Expression<TResult> : Expression
    {
        // ReSharper disable once NotNullMemberIsNotInitialized - Only Constant expression is allowed to not use a logger.
        protected Expression([NotNull] SoftString name) : base(name) { }

        protected Expression([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        private bool IsExtension => !(GetType().GetInterface(typeof(IExtension<>).Name) is null);

        public override IConstant Invoke()
        {
            if (this is IConstant constant)
            {
                //return constant;
            }

            var @this = default(object);

            // Extensions require additional handling.
            if (IsExtension)
            {
                // When "This" property is not null then we assume it's not used as an extension
                // and push this on the stack instead of the value of the previous expression.
                var thisValue = GetType().GetProperty(nameof(IExtension<object>.This)).GetValue(this);
                if (!(thisValue is null))
                {
                    switch (thisValue)
                    {
                        case IExpression e:
                            @this = e;
                            break;

                        case IEnumerable<IExpression> c:
                            //@this = Constant.Create("This", c);
                            @this = c;
                            break;
                    }
                }
            }

            var parentView = Scope.Context.Get(Namespace, x => x.DebugView);
            var thisView = parentView.Add(CreateDebugView(this));

            // Avoid making the tree deeper when this is already a Constant.
            using (@this is null ? Disposable.Empty : BeginScope(ctx => ctx.Set(Namespace, x => x.This, @this).Set(Namespace, x => x.DebugView, thisView)))
            {
                var thisResult = InvokeCore();

                thisView.Value.Result = thisResult.Value;

                // Invoke extension when used.

                if (Extension is null)
                {
                    return thisResult;
                }

                var extension = Extension;

                // Resolve the actual expression.
                while (extension is Ref @ref)
                {
                    extension = @ref.Invoke().Value<IExpression>();
                }

                var thisValue = extension.GetType().GetProperty(nameof(IExtension<object>.This)).GetValue(extension);
                if (!(thisValue is null))
                {
                    throw DynamicException.Create
                    (
                        $"AmbiguousExpressionUsage",
                        $"Expression '{extension.GetType().ToPrettyString()}/{extension.Name.ToString()}' is used as an extension and must not use the 'This' property explicitly."
                    );
                }

                var extensionType = extension.GetType().GetInterface(typeof(IExtension<>).Name)?.GetGenericArguments().Single();
                var thisType =
                    thisResult.Value is IEnumerable<IExpression> collection
                        ? collection.GetType()
                        : thisResult.GetType();

                if (extensionType?.IsAssignableFrom(thisType) == false)
                {
                    throw DynamicException.Create
                    (
                        $"ExtensionTypeMismatch",
                        $"Extension's '{extension.GetType().ToPrettyString()}' type '{extensionType.ToPrettyString()}' does not match the expression it is extending which is '{GetType().ToPrettyString()}'."
                    );
                }

                var extensionView = thisView.Add(CreateDebugView(extension));
                using (BeginScope(ctx => ctx.Set(Namespace, x => x.This, thisResult).Set(Namespace, x => x.DebugView, extensionView)))
                {
                    var extensionResult = extension.Invoke();
                    extensionView.Value.Result = extensionResult;
                    return extensionResult;
                }
            }
        }

        protected abstract Constant<TResult> InvokeCore();

        //private static bool IsExtension<T>(T obj) where T : IExpression => !(typeof(T).GetInterface(typeof(IExtension<>).Name) is null);

        protected static TreeNode<ExpressionDebugView> CreateDebugView(IExpression expression)
        {
            return TreeNode.Create(new ExpressionDebugView
            {
                Type = expression.GetType().ToPrettyString(),
                Name = expression.Name.ToString(),
                Description = expression.Description,
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
            var obj = Scope.Context.Get(Namespace, x => x.This);
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

        // This property needs to be abstract because it might be renamed so the JsonPropertyAttribute is necessary.
        public abstract IEnumerable<IExpression> This { get; set; }

        protected override Constant<TResult> InvokeCore()
        {
            var obj = Scope.Context.Get(Namespace, x => x.This);
            if (obj is IConstant constant)
            {
                obj = constant.Value;
            }
            var @this =
                obj is IEnumerable<IExpression> collection
                    ? collection
                    : throw DynamicException.Create("InvalidThisType", "Expected collection.");
            return InvokeCore(@this);
        }

        protected abstract Constant<TResult> InvokeCore(IEnumerable<IExpression> @this);
    }

    public interface IExpressionSession : ISession
    {
        object This { get; }

        IImmutableDictionary<SoftString, IEqualityComparer<object>> Comparers { get; }

        IImmutableDictionary<SoftString, IExpression> References { get; }

        TreeNode<ExpressionDebugView> DebugView { get; }
    }
}