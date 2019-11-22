using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public interface IConstant : IExpression, IEquatable<IConstant>
    {
        [AutoEqualityProperty]
        object? Value { get; }

        IImmutableContainer? Context { get; }
    }

    public interface IConstant<out TValue> : IExpression
    {
        [CanBeNull]
        TValue Value { get; }
    }

    public class Constant<TValue> : Expression<TValue>, IConstant, IConstant<TValue>, IEquatable<Constant<TValue>>
    {
        public Constant(string id, TValue value, IImmutableContainer? context = default) : base(default)
        {
            Id = id!;
            Value = value;
            Context = context;
        }

        object? IConstant.Value => Value;

        [AutoEqualityProperty]
        public TValue Value { get; }

        public IImmutableContainer? Context { get; }

        protected override Constant<TValue> ComputeConstantGeneric(IImmutableContainer context)
        {
            return (Id.ToString(), Value, context);
        }

        public void Deconstruct(out SoftString name, out TValue value, out IImmutableContainer? context)
        {
            name = Id;
            value = Value;
            context = Context;
        }

        public override string ToString() => $"{Id}: '{Value}'";

        public static implicit operator Constant<TValue>((string Id, TValue Value) t) => new Constant<TValue>(t.Id, t.Value);

        public static implicit operator Constant<TValue>((string Id, TValue Value, IImmutableContainer Context) t) => new Constant<TValue>(t.Id, t.Value, t.Context);

        public static implicit operator TValue(Constant<TValue> constant) => constant.Value;

        #region IEquatable

        public override int GetHashCode() => AutoEquality<Constant<TValue>>.Comparer.GetHashCode(this);

        public override bool Equals(object obj)
        {
            return obj switch
            {
                Constant<TValue> constant => Equals(constant),
                IConstant constant => Equals(constant),
                _ => false
            };
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
        public static Constant<TValue> FromValue<TValue>(string name, TValue value, IImmutableContainer? context = default)
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
            return values.Select((x, i) => FromValue($"{x?.ToString()}[{i}]"));
        }

        public static Constant<IEnumerable<IExpression>> FromEnumerable(string name, IEnumerable<object> values, IImmutableContainer? context = default)
        {
            return FromValue(name, values.Select((x, i) => x is IConstant constant ? constant : FromValue($"{name}.Items[{i}]", x)).ToList().Cast<IExpression>(), context);
        }

        #region Predefined

        [NotNull]
        public static readonly IExpression Zero = FromValue(nameof(Zero), 0.0);

        [NotNull]
        public static readonly IExpression One = FromValue(nameof(One), 1.0);

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

        public static bool HasContext(this IConstant constant) => constant.Context.IsNotNull();
    }
}