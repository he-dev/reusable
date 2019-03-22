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
        IExpressionContext Context { get; }

        [NotNull]
        IExpression Invoke([NotNull] IExpressionContext context);
    }

    public interface IExtension<T> { }

    public static class Expression
    {
        // ReSharper disable RedundantNameQualifier - Use full namespace to avoid conflicts with other types.
        public static readonly Type[] Types =
        {
            typeof(Reusable.Flexo.ObjectEqual),
            typeof(Reusable.Flexo.StringEqual),
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
        protected Expression([NotNull] SoftString name, [NotNull] IExpressionContext context)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual SoftString Name { get; }

        public IExpressionContext Context { get; }

        public bool Enabled { get; set; } = true;

        public List<IExpression> This { get; set; } = new List<IExpression>();

        public virtual IExpression Invoke(IExpressionContext context)
        {
            var extensions = This ?? Enumerable.Empty<IExpression>();
            var result = InvokeCore(context).ToExpression(Name);
            return
                extensions
                    .Enabled()
                    .Aggregate(result, (previous, next) =>
                    {
                        var extensionType = next.GetType().GetInterface(typeof(IExtension<>).Name)?.GetGenericArguments().Single();
                        var thisType = previous is IConstant constant ? constant.Value.GetType() : previous.GetType().GetGenericArguments().Single();

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

        protected abstract ExpressionResult<TResult> InvokeCore(IExpressionContext context);

        /// <summary>
        /// Determines whether the expression is invoked as an extension and removes the Input from the context.
        /// </summary>
        private bool IsExtension(ref IExpressionContext context, out IExpression input)
        {
            var inputKey = ExpressionContext.CreateKey(Item.For<IExtensionContext>(), x => x.Input);
            if (context.TryGetValue(inputKey, out var value))
            {
                input = (IExpression)value;
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

        protected TInput ExtensionInputOrDefault<TInput>(ref IExpressionContext context, TInput defaultInput)
        {
            return
                IsExtension(ref context, out var input)
                    //? defaultInput is IEnumerable<IExpression> ? input.Value<T>() : (T)input
                    ? typeof(IEnumerable<IExpression>).IsAssignableFrom(typeof(TInput)) ? input.Value<TInput>() : (TInput)input
                    : defaultInput;
        }

        protected TInput ExtensionInput<TInput>(ref IExpressionContext context)
        {
            return
                IsExtension(ref context, out var input)
                    //? defaultInput is IEnumerable<IExpression> ? input.Value<T>() : (T)input
                    ? typeof(IEnumerable<IExpression>).IsAssignableFrom(typeof(TInput)) ? input.Value<TInput>() : (TInput)input
                    : throw new InvalidOperationException("Extension input not specified.");
        }
    }

    public interface IExtensionContext
    {
        IExpression Input { get; }
    }

    public readonly struct ExpressionResult<T>
    {
        private ExpressionResult(T value, IExpressionContext context)
        {
            Value = value;
            Context = context;
        }

        public T Value { get; }

        public IExpressionContext Context { get; }

        public void Deconstruct(out T value, out IExpressionContext context)
        {
            value = Value;
            context = Context;
        }

        public static implicit operator ExpressionResult<T>((T Value, IExpressionContext Context) t) => new ExpressionResult<T>(t.Value, t.Context);
    }

    public static class ExpressionResultExtensions
    {
        [NotNull]
        public static IExpression ToExpression<TResult>(this ExpressionResult<TResult> result, SoftString name)
        {
            return result.Value is IExpression expression ? expression : Constant.FromResult(name, result);
        }
    }
}