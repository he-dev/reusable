using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Collections;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    public interface IConstant
    {
        [NotNull]
        string Name { get; }

        [CanBeNull]
        object Value { get; }
    }

    public class Constant<TValue> : Expression, IEquatable<Constant<TValue>>, IConstant
    {
        public Constant(string name, TValue value, IExpressionContext context) : base(name, context) => Value = value;

        [JsonConstructor]
        public Constant(string name, TValue value) : this(name, value, ExpressionContext.Empty) => Value = value;


        [AutoEqualityProperty]
        [CanBeNull]
        public TValue Value { get; }

        object IConstant.Value => Value;

        public override IExpression Invoke(IExpressionContext context)
        {
            using (context.Scope(this))
            {
                return Constant.FromResult<TValue>(Name, (Value, context));
            }
        }

        public override string ToString() => $"{Name}: '{Value}'";

        public static implicit operator Constant<TValue>((string name, TValue value) t) => new Constant<TValue>(t.name, t.value);

        public static implicit operator TValue(Constant<TValue> constant) => constant.Value;

        #region IEquatable

        public override int GetHashCode() => AutoEquality<Constant<TValue>>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => obj is Constant<TValue> constant && Equals(constant);

        public bool Equals(Constant<TValue> other) => AutoEquality<Constant<TValue>>.Comparer.Equals(this, other);

        #endregion
    }

    /// <summary>
    /// This class provides factory methods.
    /// </summary>
    public class Constant
    {
        private static volatile int _counter;

        [NotNull]
        public static Constant<TValue> FromValue<TValue>(string name, TValue value)
        {
            return new Constant<TValue>(name, value);
        }

        public static Constant<TValue> FromResult<TValue>(string name, InvokeResult<TValue> result)
        {
            return new Constant<TValue>(name, result.Value, result.Context);
        }

        [NotNull]
        public static Constant<TValue> Create<TValue>(TValue value)
        {
            return new Constant<TValue>($"{typeof(Constant<TValue>).ToPrettyString()}{_counter++}", value);
        }

        [NotNull, ItemNotNull]
        public static IList<Constant<TValue>> CreateMany<TValue>(string name, params TValue[] values)
        {
            return values.Select(value => FromValue(name, value)).ToList();
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<Constant<TValue>> CreateMany<TValue>(params TValue[] values)
        {
            return values.Select(Create);
        }

        #region Predefined

        [NotNull] public static readonly IExpression Zero = new Zero(nameof(Zero));

        [NotNull] public static readonly IExpression One = new One(nameof(One));

        [NotNull] public static readonly IExpression True = new True(nameof(True));

        [NotNull] public static readonly IExpression False = new False(nameof(False));

        [NotNull] public static readonly IExpression EmptyString = FromValue(nameof(EmptyString), string.Empty);

        [NotNull] public static readonly IExpression Null = FromValue(nameof(Null), default(object));

        #endregion
    }
}