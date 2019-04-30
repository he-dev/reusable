using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    public interface ISwitchable
    {
        [DefaultValue(true)]
        bool Enabled { get; }
    }

    public interface IExtendable
    {
        List<IExpression> Extensions { get; }
    }

    [UsedImplicitly]
    [PublicAPI]
    public interface IExpression : ISwitchable, IExtendable
    {
        [NotNull]
        SoftString Name { get; }

        string Description { get; }

        [NotNull]
        IConstant Invoke([NotNull] IImmutableSession context);
    }

    public interface IExtension<out T>
    {
        T This { get; }

        //IConstant Invoke([NotNull] T @this, [NotNull] IImmutableSession context);
    }

    [Namespace("Flexo")]
    public abstract class Expression<TResult> : IExpression
    {
        private SoftString _name;

        [Obsolete("Use the other overload with a logger.")]
        protected Expression([NotNull] SoftString name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        protected Expression([NotNull] ILogger logger, SoftString name)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        [NotNull]
        protected ILogger Logger { get; }

        public virtual SoftString Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Name));
        }

        public string Description { get; set; }

        public bool Enabled { get; set; } = true;

        [JsonProperty("This")]
        public List<IExpression> Extensions { get; set; }

        private bool IsExtension => !(GetType().GetInterface(typeof(IExtension<>).Name) is null);

        public virtual IConstant Invoke(IImmutableSession context)
        {
            var scope = Use<IExpressionSession>.Scope;

            // Extensions require additional handling.
            if (IsExtension)
            {
                // When "This" property is not null then we assume it's not used as an extension
                // and push this on the stack instead of the value of the previous expression.
                var thisValue = GetType().GetProperty(nameof(IExtension<object>.This)).GetValue(this);
                if (!(thisValue is null))
                {
                    var @this = default(IConstant);
                    switch (thisValue)
                    {
                        case IConstant e:
                            @this = e;
                            break;

                        case IEnumerable<IExpression> c:
                            @this = Constant.FromNameAndValue("This", c);
                            break;
                    }

                    context.PushThis(@this);
                }
            }

            var parentNode = context.Get(scope, x => x.DebugView);
            var thisView = new ExpressionDebugView
            {
                Type = GetType().ToPrettyString(),
                Name = Name.ToString(),
                Description = Description,
            };
            var thisNode = TreeNode.Create(thisView);
            parentNode.Add(thisNode);
            var thisResult = (IConstant)InvokeCore(context.Set(scope, x => x.DebugView, thisNode));
            thisView.Result = thisResult.Value;

            var seed = (IConstant)Constant.FromNameAndValue
            (
                thisResult.Name,
                thisResult.Value,
                thisResult.Context.Set(scope, x => x.DebugView, thisNode)
            );

            var enabledExtensions = (Extensions ?? Enumerable.Empty<IExpression>()).Enabled();
            var extensionsResult = enabledExtensions.Aggregate(seed, (previous, next) =>
            {
                // Resolve the actual expression.
                while (next is Ref @ref)
                {
                    next = @ref.Invoke(previous.Context).Value<IExpression>();
                }

                var thisValue = next.GetType().GetProperty(nameof(IExtension<object>.This)).GetValue(next);
                if (!(thisValue is null))
                {
                    throw DynamicException.Create
                    (
                        $"AmbiguousExpressionUsage",
                        $"Expression '{next.GetType().ToPrettyString()}/{next.Name.ToString()}' is used as an extension and must not use the 'This' property explicitly."
                    );
                }

                var extensionType = next.GetType().GetInterface(typeof(IExtension<>).Name)?.GetGenericArguments().Single();
                var thisType =
                    previous.Value is IEnumerable<IExpression> collection
                        ? collection.GetType()
                        : previous.GetType();

                if (extensionType?.IsAssignableFrom(thisType) == false)
                {
                    throw DynamicException.Create
                    (
                        $"ExtensionTypeMismatch",
                        $"Extension's '{next.GetType().ToPrettyString()}' type '{extensionType.ToPrettyString()}' does not match the expression it is extending which is '{previous.GetType().ToPrettyString()}'."
                    );
                }

                return next.Invoke(previous.Context.PushThis(previous));
            });

            return extensionsResult;
        }

        protected abstract Constant<TResult> InvokeCore(IImmutableSession context);

        //private static bool IsExtension<T>(T obj) where T : IExpression => !(typeof(T).GetInterface(typeof(IExtension<>).Name) is null);
    }

    public abstract class ValueExtension<TResult> : Expression<TResult>, IExtension<IExpression>
    {
        protected ValueExtension([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        // This property needs to be abstract because it might be renamed so the JsonPropertyAttribute is necessary.
        public abstract IExpression This { get; set; }

        protected override Constant<TResult> InvokeCore(IImmutableSession context)
        {
            return InvokeCore(context, context.PopThisConstant());
        }

        protected abstract Constant<TResult> InvokeCore(IImmutableSession context, IExpression @this);
    }

    public abstract class CollectionExtension<TResult> : Expression<TResult>, IExtension<IEnumerable<IExpression>>
    {
        protected CollectionExtension([NotNull] ILogger logger, SoftString name) : base(logger, name) { }

        // This property needs to be abstract because it might be renamed so the JsonPropertyAttribute is necessary.
        public abstract IEnumerable<IExpression> This { get; set; }

        protected override Constant<TResult> InvokeCore(IImmutableSession context)
        {
            return InvokeCore(context, context.PopThisCollection().Enabled());
        }

        protected abstract Constant<TResult> InvokeCore(IImmutableSession context, IEnumerable<IExpression> @this);
    }

    public interface IExpressionSession : ISession
    {
        Stack<IConstant> This { get; }

        IImmutableDictionary<SoftString, IEqualityComparer<object>> Comparers { get; }

        IImmutableDictionary<SoftString, IExpression> Expressions { get; }

        TreeNode<ExpressionDebugView> DebugView { get; }
    }
}