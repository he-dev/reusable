using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
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
        List<IExpression> This { get; }
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

    public interface IExtension<in T> { }

    public static class Expression
    {
        // ReSharper disable RedundantNameQualifier - Use full namespace to avoid conflicts with other types.
        public static readonly Type[] Types =
        {
            typeof(Reusable.Flexo.IsEqual),
            typeof(Reusable.Flexo.IsGreaterThan),
            typeof(Reusable.Flexo.IsGreaterThanOrEqual),
            typeof(Reusable.Flexo.IsLessThan),
            typeof(Reusable.Flexo.IsLessThanOrEqual),
            typeof(Reusable.Flexo.Not),
            typeof(Reusable.Flexo.All),
            typeof(Reusable.Flexo.Any),
            typeof(Reusable.Flexo.IIf),
            typeof(Reusable.Flexo.Switch),
            typeof(Reusable.Flexo.ToDouble),
            typeof(Reusable.Flexo.ToString),
            typeof(Reusable.Flexo.GetValue),
            typeof(Reusable.Flexo.GetCollection),
            typeof(Reusable.Flexo.Ref),
            typeof(Reusable.Flexo.Contains),
            typeof(Reusable.Flexo.Overlaps),
            typeof(Reusable.Flexo.Matches),
            typeof(Reusable.Flexo.Min),
            typeof(Reusable.Flexo.Max),
            typeof(Reusable.Flexo.Count),
            typeof(Reusable.Flexo.Sum),
            typeof(Reusable.Flexo.Constant<>),
            typeof(Reusable.Flexo.Double),
            typeof(Reusable.Flexo.Integer),
            typeof(Reusable.Flexo.Decimal),
            typeof(Reusable.Flexo.DateTime),
            typeof(Reusable.Flexo.TimeSpan),
            typeof(Reusable.Flexo.String),
            typeof(Reusable.Flexo.True),
            typeof(Reusable.Flexo.False),
            typeof(Reusable.Flexo.Collection),
            typeof(Reusable.Flexo.Select),
            typeof(Reusable.Flexo.Throw),
        };
        // ReSharper restore RedundantNameQualifier

        public static IImmutableSession DefaultSession =>
            ImmutableSession
                .Empty
                .Set(Use<IExpressionSession>.Scope, x => x.ExtensionInputs, new Stack<object>())
                .Set(Use<IExpressionSession>.Scope, x => x.Comparers, ImmutableDictionary<SoftString, IEqualityComparer<object>>.Empty)
                .Set(Use<IExpressionSession>.Scope, x => x.DebugView, TreeNode.Create(ExpressionDebugView.Root))
                .WithDefaultComparer()
                .WithSoftStringComparer()
                .WithRegexComparer();
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

        public List<IExpression> This { get; set; } = new List<IExpression>();

        public virtual IConstant Invoke(IImmutableSession context)
        {
            var scope = Use<IExpressionSession>.Scope;

//            if (IsExtension(GetType()) && context.This() is var @this && @this is null)
//            {
//                var value = GetType().GetProperties().Single(p => p.IsDefined(typeof(ThisAttribute), true)).GetValue(this);
//                switch (value)
//                {
//                    case IExpression e:
//                        @this = e.Invoke(context);
//                        break;
//                    
//                    case IEnumerable<IExpression> c:
//                        @this = Constant.FromValue("This", c.Select(e => e.Invoke(context).Value).ToList());
//                        break;
//                }
//                context = context.Set(scope, x => x.This, @this);
//            }

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

            var enabledExtensions = (This ?? Enumerable.Empty<IExpression>()).Enabled();
            var extensionsResult = enabledExtensions.Aggregate(seed, (previous, next) =>
            {
                if (next is Ref @ref)
                {
                    next = @ref.Invoke(previous.Context).Value<IExpression>();
                }

                var extensionType = next.GetType().GetInterface(typeof(IExtension<>).Name)?.GetGenericArguments().Single();
                var thisType = previous.Value is IExpression expression ? expression.GetType().GetGenericArguments().Single() : previous.Value?.GetType();

                if (extensionType?.IsAssignableFrom(thisType) == true)
                {
                    var innerContext =
                        previous
                            .Context
                            //.Set(scope, x => x.This, Constant.FromValue("This", previous.Value))
                            .PushExtensionInput(previous.Value);

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
            return type.GetInterface(typeof(IExtension<>).Name)?.GetGenericArguments().Any() == true;
        }
    }

    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class ExpressionDebugView
    {
        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayValue(x => x.Name);
            b.DisplayValue(x => x.Result);
            b.DisplayValue(x => x.Description);
        });

        public static ExpressionDebugView Root => new ExpressionDebugView
        {
            Name = "Root",
            Description = "This is the root node of the expression debug-view."
        };

        public static RenderValueCallback<ExpressionDebugView> DefaultRender
        {
            get { return (dv, d) => $"[{dv.Type}] as [{dv.Name}]: '{dv.Result}' ({dv.Description})"; }
        }

        public string Type { get; set; } = $"<{nameof(Type)}>";

        public string Name { get; set; } = $"<{nameof(Name)}>";

        public string Description { get; set; } = $"<{nameof(Description)}>";

        public object Result { get; set; } = $"<{nameof(Result)}>";

        //public ExpressionInvokeConvention InvokeConvention { get; set; }        
    }

    public enum ExpressionInvokeConvention
    {
        Normal,
        Extension
    }

    public interface IExpressionSession : ISession
    {
        Stack<object> ExtensionInputs { get; }

        //IConstant This { get; }

        IImmutableDictionary<SoftString, IEqualityComparer<object>> Comparers { get; }

        IImmutableDictionary<SoftString, IExpression> Expressions { get; }

        TreeNode<ExpressionDebugView> DebugView { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ThisAttribute : Attribute { }
}