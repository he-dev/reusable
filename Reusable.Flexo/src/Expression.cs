using System;
using System.Collections;
using System.Collections.Generic;
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
        IConstant Invoke([NotNull] IExpressionContext context);
    }

    public interface IExtension<T> { }

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
            typeof(Reusable.Flexo.GetContextItem),
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

        public virtual IConstant Invoke(IExpressionContext context)
        {
            var parentNode = context.DebugView();
            var thisView = new ExpressionDebugView
            {
                Type = GetType().ToPrettyString(),
                Name = Name.ToString(),
                Description = Description,
            };
            var thisNode = TreeNode.Create(thisView);
            parentNode.Add(thisNode);
            var thisResult = (IConstant)InvokeCore(context.DebugView(thisNode));
            thisView.Result = thisResult.Value;

            var seed = (IConstant)Constant.FromValue
            (
                thisResult.Name,
                thisResult.Value,
                thisResult.Context.DebugView(thisNode)
            );

            var enabledExtensions = (This ?? Enumerable.Empty<IExpression>()).Enabled();
            var extensionsResult = enabledExtensions.Aggregate(seed, (previous, next) =>
            {
                var extensionType = next.GetType().GetInterface(typeof(IExtension<>).Name)?.GetGenericArguments().Single();
                var thisType = previous.Value is IExpression expression ? expression.GetType().GetGenericArguments().Single() : previous.Value?.GetType();

                if (extensionType?.IsAssignableFrom(thisType) == true)
                {
                    var innerContext =
                        previous
                            .Context
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

        protected abstract Constant<TResult> InvokeCore(IExpressionContext context);
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
}