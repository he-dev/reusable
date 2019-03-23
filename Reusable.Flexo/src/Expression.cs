using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Utilities.JsonNet;
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

        [NotNull]
        IConstant Invoke([NotNull] IExpressionContext context);
    }

    public interface IExtension<T> { }

    public static class Expression
    {
        // ReSharper disable RedundantNameQualifier - Use full namespace to avoid conflicts with other types.
        public static readonly Type[] Types =
        {
            //typeof(Reusable.Flexo.ObjectEqual),
            //typeof(Reusable.Flexo.StringEqual),
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
            //typeof(Reusable.Flexo.ToList),
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
            typeof(Reusable.Flexo.SoftStringComparer),
            typeof(Reusable.Flexo.IsEqual),
            typeof(Reusable.Flexo.RegexComparer),
        };
        // ReSharper restore RedundantNameQualifier
    }

    [Namespace("Flexo")]
    public abstract class Expression<TResult> : IExpression
    {
        protected Expression([NotNull] SoftString name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public virtual SoftString Name { get; }

        public bool Enabled { get; set; } = true;

        public List<IExpression> This { get; set; } = new List<IExpression>();

        public virtual IConstant Invoke(IExpressionContext context)
        {
            var extensions = This ?? Enumerable.Empty<IExpression>();
            var result = (IConstant)InvokeCore(context);
            return
                extensions
                    .Enabled()
                    .Aggregate(result, (previous, next) =>
                    {
                        var extensionType = next.GetType().GetInterface(typeof(IExtension<>).Name)?.GetGenericArguments().Single();
                        var thisType = previous.Value is IExpression expression ? expression.GetType().GetGenericArguments().Single() : previous.Value.GetType();

                        if (extensionType?.IsAssignableFrom(thisType) == true)
                        {
                            return next.Invoke(previous.Context.ExtensionInput(previous));
                        }
                        else
                        {
                            throw DynamicException.Create
                            (
                                "ExtensionTypeMismatch",
                                $"Extension '{next.GetType().ToPrettyString()}' does not match the expression it is extending which is '{previous.GetType().ToPrettyString()}'."
                            );
                        }
                    });
        }

        protected abstract Constant<TResult> InvokeCore(IExpressionContext context);

        /// <summary>
        /// Determines whether the expression is invoked as an extension and removes the Input from the context.
        /// </summary>
        private bool IsExtension(ref IExpressionContext context, out IConstant input)
        {
            var inputKey = ExpressionContext.CreateKey(Item.For<IExtensionContext>(), x => x.Input);
            if (context.TryGetValue(inputKey, out var value))
            {
                input = value;
                // The Input must be removed so that subsequent expression doesn't 'think' it's called as extension when it isn't.
                context = context.Remove(inputKey);
                return true;
            }
            else
            {
                input = default;
                return false;
            }
        }

        [NotNull]
        protected TInput ExtensionInputOrDefault<TInput>(ref IExpressionContext context, TInput defaultInput)
        {
            return
                IsExtension(ref context, out var input)
                    ? typeof(IExpression).IsAssignableFrom(typeof(TInput)) ? (TInput)input : (TInput)(input.Value ?? throw new ArgumentNullException("Input", "Input must not be null."))
                    : defaultInput;
        }
    }

    public interface IExtensionContext
    {
        IConstant Input { get; }
    }
}