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

        IImmutableContainer Context { get; }
    }

    public interface IConstant<out TValue> : IExpression
    {
        [CanBeNull]
        TValue Value { get; }
    }

    public class Constant<TValue> : Expression<TValue>, IConstant, IConstant<TValue>, IEquatable<Constant<TValue>>
    {
        public Constant(SoftString name, TValue value, IImmutableContainer? context = default)
            : base(EmptyLogger.Instance)
        {
            Value = value;
            Context = context ?? ImmutableContainer.Empty;
        }

        object IConstant.Value => Value;

        [AutoEqualityProperty]
        public TValue Value { get; set; }

        public IImmutableContainer Context { get; }

//        public override IConstant Invoke(IImmutableContainer context)
//        {
//            // Must return a new constant because predefined ones don't have context.
//            return Constant.FromValue(Name, Value, context);
//        }

        protected override Constant<TValue> ComputeConstantGeneric(IImmutableContainer context)
        {
            return (Id, Value, context);
        }

        public void Deconstruct(out SoftString name, out TValue value, out IImmutableContainer context)
        {
            name = Id;
            value = Value;
            context = Context;
        }

        public override string ToString() => $"{Id.ToString()}: '{Value}'";

        public static implicit operator Constant<TValue>((SoftString Name, TValue Value) t) => new Constant<TValue>(t.Name, t.Value);

        public static implicit operator Constant<TValue>((SoftString Name, TValue Value, IImmutableContainer Context) t) => new Constant<TValue>(t.Name, t.Value, t.Context);

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
            IEnumerable<T> values
        )
            : base
            (
                name,
                values
                    .Select((x, i) => Constant.FromValue($"{name.ToString()}.Items[{i}]"))
                    .ToList()
            ) { }
    }

    /// <summary>
    /// This class provides factory methods.
    /// </summary>
    public static class Constant
    {
        private static volatile int _counter;

        [NotNull]
        public static Constant<TValue> FromValue<TValue>(SoftString name, TValue value, IImmutableContainer? context = default)
        {
            return new Constant<TValue>(name, value, context);
        }

        [NotNull]
        internal static Constant<TValue> FromValue<TValue>(TValue value, IImmutableContainer? context = default)
        {
            return FromValue($"{typeof(Constant<TValue>).ToPrettyString()}-{_counter++}", value, context);
        }

        [NotNull, ItemNotNull]
        internal static IEnumerable<IExpression> CreateMany<TValue>(string name, params TValue[] values)
        {
            return values.Select(value => FromValue(name, value));
        }

        [NotNull, ItemNotNull]
        internal static IEnumerable<IExpression> CreateMany<TValue>(params TValue[] values)
        {
            return values.Select(value => FromValue(value));
        }

        public static Constant<IEnumerable<IExpression>> FromEnumerable(SoftString name, IEnumerable<object> values, IImmutableContainer? context = default)
        {
            return FromValue(name, values.Select((x, i) => x is IConstant constant ? constant : FromValue($"{name.ToString()}.Items[{i}]", x)).ToList().Cast<IExpression>(), context);
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

        [NotNull]
        public static readonly IExpression Unit = FromValue(nameof(Unit), default(object));

        public static readonly IExpression DefaultComparer = FromValue(Filter.Properties.Comparer, "Default");

        #endregion
    }
}