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

        public virtual IConstant Invoke(IImmutableSession context)
        {
            var scope = Use<IExpressionSession>.Scope;

            // Invoke the property marked with [This] when this is an extension and this expression wasn't called as one.
            if (IsExtension(GetType())) // && context.PopThis() is var @this && @this is null)
            {
                // There are expressions that can only be used as extensions so they actually don't have a property for "This" (like LessThan etc)
                var thisProperty = GetType().GetProperty(nameof(IExtension<object>.This));
                var value = thisProperty?.GetValue(this);
                if (!(thisProperty is null) && !(value is null))
                {
                    var @this = default(IConstant);
                    switch (value)
                    {
                        case IConstant e:
                            @this = e;
                            break;

                        case IEnumerable<IExpression> c:
                            @this = Constant.FromValue("This", c);
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

            var seed = (IConstant)Constant.FromValue
            (
                thisResult.Name,
                thisResult.Value,
                thisResult.Context.Set(scope, x => x.DebugView, thisNode)
            );

            var enabledExtensions = (Extensions ?? Enumerable.Empty<IExpression>()).Enabled();
            var extensionsResult = enabledExtensions.Aggregate(seed, (previous, next) =>
            {
                if (next is Ref @ref)
                {
                    next = @ref.Invoke(previous.Context).Value<IExpression>();
                }

                var extensionType = next.GetType().GetInterface(typeof(IExtension<>).Name)?.GetGenericArguments().Single();
                var thisType =
                    previous.Value is IEnumerable<IExpression> collection
                        ? collection.GetType()
                        : previous.GetType();

                if (extensionType?.IsAssignableFrom(thisType) == true)
                {
                    var innerContext =
                        previous
                            .Context
                            .PushThis(previous);

                    return next.Invoke(innerContext);
                }
                else
                {
                    throw DynamicException.Create
                    (
                        $"ExtensionTypeMismatch",
                        $"Extension '{next.GetType().ToPrettyString()}' does not match the expression it is extending which is '{previous.GetType().ToPrettyString()}'."
                    );
                }
            });

            return extensionsResult;
        }

        protected abstract Constant<TResult> InvokeCore(IImmutableSession context);

        private static bool IsExtension(Type type)
        {
            return !(type.GetInterface(typeof(IExtension<>).Name) is null);
        }
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
            return InvokeCore(context, context.PopThisCollection());
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