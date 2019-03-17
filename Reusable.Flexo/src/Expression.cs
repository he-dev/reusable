using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
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

    public interface IExtension<T>
    {
        //IExpression This { get; }
    }

    [Namespace("Flexo")]
    public abstract class Expression : IExpression
    {
        // ReSharper disable RedundantNameQualifier - Use full namespace to avoid conflicts with other types.
        public static readonly Type[] Types =
        {
            typeof(Reusable.Flexo.ObjectEqual),
            typeof(Reusable.Flexo.StringEqual),
            typeof(Reusable.Flexo.GreaterThan),
            typeof(Reusable.Flexo.GreaterThanOrEqual),
            typeof(Reusable.Flexo.LessThan),
            typeof(Reusable.Flexo.LessThanOrEqual),
            typeof(Reusable.Flexo.Not),
            typeof(Reusable.Flexo.All),
            typeof(Reusable.Flexo.Any),
            typeof(Reusable.Flexo.IIf),
            typeof(Reusable.Flexo.Switch),
            typeof(Reusable.Flexo.SwitchToDouble),
            typeof(Reusable.Flexo.GetContextItem),
            typeof(Reusable.Flexo.Contains),
            typeof(Reusable.Flexo.Matches),
            typeof(Reusable.Flexo.Min),
            typeof(Reusable.Flexo.Max),
            typeof(Reusable.Flexo.Count),
            typeof(Reusable.Flexo.Sum),
            typeof(Reusable.Flexo.ToList),
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
        };
        // ReSharper restore RedundantNameQualifier

        protected Expression(SoftString name) : this(name, ExpressionContext.Empty) { }

        protected Expression(SoftString name, IExpressionContext context)
        {
            Name = name;
            Context = context;
        }

        public virtual SoftString Name { get; }

        public IExpressionContext Context { get; }

        public bool Enabled { get; set; } = true;

        public List<IExpression> This { get; set; } = new List<IExpression>();

        public virtual IExpression Invoke(IExpressionContext context)
        {
            var extensions = This ?? Enumerable.Empty<IExpression>();
            var result = InvokeCore(context);
            return
                extensions
                    .Enabled()
                    .Aggregate
                    (
                        result,
                        (current, next) => next.Invoke(context.Set(Item.For<IExtensionContext>(), x => x.Input, current))
                    );
        }

        protected abstract IExpression InvokeCore(IExpressionContext context);
    }

    public interface IExtensionContext
    {
        IExpression Input { get; }
    }

    public readonly struct InvokeResult<T>
    {
        public InvokeResult(T value, IExpressionContext context)
        {
            Value = value;
            Context = context;
        }

        public T Value { get; }

        public IExpressionContext Context { get; }

        public static implicit operator InvokeResult<T>((T Value, IExpressionContext Context) t) => new InvokeResult<T>(t.Value, t.Context);
    }

    public static class InvokeResult
    {
        public static InvokeResult<TValue> From<TValue>(TValue value, IExpressionContext context)
        {
            return new InvokeResult<TValue>(value, context);
        }
    }
}