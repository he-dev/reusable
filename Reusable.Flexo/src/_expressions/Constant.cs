using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
    }

    public interface IConstant<out TValue> : IExpression
    {
        [CanBeNull]
        TValue Value { get; }
    }

    public class Constant<TValue> : Expression<TValue>, IConstant, IConstant<TValue>, IEquatable<Constant<TValue>>
    {
        public Constant(SoftString name, TValue value, IImmutableContainer context = default)
            : base(LoggerDummy.Instance, name ?? value.GetType().ToPrettyString())
        {
            Value = value;
            Context = context ?? ImmutableContainer.Empty;
        }

        object IConstant.Value => Value;

        [AutoEqualityProperty]
        public TValue Value { get; set; }

        public IImmutableContainer Context { get; }

        protected override Constant<TValue> InvokeCore()
        {
            return (Name, Value);
        }

        public void Deconstruct(out SoftString name, out TValue value)
        {
            name = Name;
            value = Value;
        }

        public override string ToString() => $"{Name.ToString()}: '{Value}'";

        public static implicit operator Constant<TValue>((SoftString Name, TValue Value) t) => new Constant<TValue>(t.Name, t.Value, ImmutableContainer.Empty);

        public static implicit operator Constant<TValue>((SoftString Name, TValue Value, IImmutableContainer Context) t) => new Constant<TValue>(t.Name, t.Value, t.Context);

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

    public class EnumerableConstant<T> : Constant<IEnumerable<IExpression>>
    {
        public EnumerableConstant
        (
            SoftString name,
            IEnumerable<T> values,
            IImmutableContainer context = default
        )
            : base
            (
                name,
                values
                    .Select((x, i) => Constant.FromValue($"{name.ToString()}.Items[{i}]", context))
                    .ToList(),
                context
            ) { }
    }

    /// <summary>
    /// This class provides factory methods.
    /// </summary>
    public static class Constant
    {
        private static volatile int _counter;

        [NotNull]
        public static Constant<TValue> FromValue<TValue>(SoftString name, TValue value)
        {
            return new Constant<TValue>(name, value);
        }

        [NotNull]
        internal static Constant<TValue> FromValue<TValue>(TValue value)
        {
            return FromValue($"{typeof(Constant<TValue>).ToPrettyString()}-{_counter++}", value);
        }

        [NotNull, ItemNotNull]
        internal static IEnumerable<IExpression> CreateMany<TValue>(string name, params TValue[] values)
        {
            return values.Select(value => FromValue(name, value));
        }

        [NotNull, ItemNotNull]
        internal static IEnumerable<IExpression> CreateMany<TValue>(params TValue[] values)
        {
            return values.Select(FromValue);
        }

        public static Constant<IEnumerable<IExpression>> FromEnumerable(SoftString name, IEnumerable<object> values, IImmutableContainer context = default)
        {
            return FromValue(name, values.Select((x, i) => x is IConstant constant ? constant : FromValue($"{name.ToString()}.Items[{i}]", x)).ToList().Cast<IExpression>());
        }

        #region Predefined

        //[NotNull] public static readonly IExpression Zero = new Zero(nameof(Zero));

        //[NotNull] public static readonly IExpression One = new One(nameof(One));

        [NotNull]
        public static readonly IExpression True = FromValue(nameof(True), true);

        [NotNull]
        public static readonly IExpression False = FromValue(nameof(False), false);

        [NotNull]
        public static readonly IExpression EmptyString = FromValue(nameof(EmptyString), string.Empty);

        [NotNull]
        public static readonly IExpression Null = FromValue(nameof(Null), default(object));

        #endregion
    }
}