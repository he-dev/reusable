using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    public interface IConstant : IExpression, IEquatable<IConstant>
    {
        [AutoEqualityProperty]
        [CanBeNull]
        object Value { get; }

        [NotNull]
        IExpressionContext Context { get; }
    }

    public interface IConstant<out TValue> : IExpression
    {
        [CanBeNull]
        TValue Value { get; }
    }

    public class Constant<TValue> : Expression<TValue>, IConstant, IConstant<TValue>, IEquatable<Constant<TValue>>
    {
        public Constant(SoftString name, TValue value, IExpressionContext context = default) : base(name ?? value.GetType().ToPrettyString())
        {
            Value = value;
            Context = context ?? ExpressionContext.Empty;
        }

        object IConstant.Value => Value;

        [AutoEqualityProperty]
        public TValue Value { get; set; }

        public IExpressionContext Context { get; }

        protected override Constant<TValue> InvokeCore(IExpressionContext context)
        {
            //using (context.Scope(this))
            {
                return (Name, Value, context);
            }
        }

        public void Deconstruct(out SoftString name, out TValue value)
        {
            name = Name;
            value = Value;
        }

        public override string ToString() => $"{Name.ToString()}: '{Value}'";

        public static implicit operator Constant<TValue>((SoftString Name, TValue Value) t) => new Constant<TValue>(t.Name, t.Value, ExpressionContext.Empty);

        public static implicit operator Constant<TValue>((SoftString Name, TValue Value, IExpressionContext Context) t) => new Constant<TValue>(t.Name, t.Value, t.Context);

        //public static implicit operator Constant<ExpressionResult<TValue>>((string Name, ExpressionResult<TValue> Result) t) => new Constant<ExpressionResult<TValue>>(t.Name, t.Result);

        public static implicit operator TValue(Constant<TValue> constant) => constant.Value;

        #region IEquatable

        public override int GetHashCode() => AutoEquality<Constant<TValue>>.Comparer.GetHashCode(this);

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Constant<TValue> constant: return Equals(constant);
                case IConstant constant: return Equals(constant);
                default: return false;
            }
        }

        public bool Equals(Constant<TValue> other) => AutoEquality<Constant<TValue>>.Comparer.Equals(this, other);

        public bool Equals(IConstant other) => AutoEquality<IConstant>.Comparer.Equals(this, other);

        #endregion
    }

    /// <summary>
    /// This class provides factory methods.
    /// </summary>
    public static class Constant
    {
        private static volatile int _counter;

        [NotNull]
        public static Constant<TValue> FromValue<TValue>(SoftString name, TValue value, IExpressionContext context = default)
        {
            return new Constant<TValue>(name, value, context ?? ExpressionContext.Empty);
        }

        [NotNull]
        public static Constant<TValue> Create<TValue>(TValue value)
        {
            return FromValue($"{typeof(Constant<TValue>).ToPrettyString()}{_counter++}", value);
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<IExpression> CreateMany<TValue>(string name, params TValue[] values)
        {
            return values.Select(value => FromValue(name, value));
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<IExpression> CreateMany<TValue>(params TValue[] values)
        {
            return values.Select(Create);
        }

        #region Predefined

        //[NotNull] public static readonly IExpression Zero = new Zero(nameof(Zero));

        //[NotNull] public static readonly IExpression One = new One(nameof(One));

        [NotNull] public static readonly IExpression True = FromValue(nameof(True), true);

        [NotNull] public static readonly IExpression False = FromValue(nameof(False), false);

        [NotNull] public static readonly IExpression EmptyString = FromValue(nameof(EmptyString), string.Empty);

        [NotNull] public static readonly IExpression Null = FromValue(nameof(Null), default(object));

        #endregion
    }
}