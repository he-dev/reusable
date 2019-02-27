using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Collections;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    public interface IConstant
    {
        string Name { get; }

        object Value { get; }
    }

    public class Constant<TValue> : Expression, IEquatable<Constant<TValue>>, IConstant
    {
        public Constant(string name) : base(name) { }

        [JsonConstructor]
        public Constant(string name, TValue value) : this(name) => Value = value;

        [AutoEqualityProperty]
        [CanBeNull]
        public TValue Value { get; }

        [CanBeNull]
        object IConstant.Value => Value;

        public override IExpression Invoke(IExpressionContext context)
        {
            using (context.Scope(this))
            {
                return this;
            }
        }

        public override string ToString() => $"\"{Name}\" = \"{Value}\"";

        public static implicit operator Constant<TValue>((string name, TValue value) t) => new Constant<TValue>(t.name, t.value);

        public static implicit operator TValue(Constant<TValue> constant) => constant.Value;

        #region IEquatable

        public override int GetHashCode() => AutoEquality<Constant<TValue>>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => obj is Constant<TValue> constant && Equals(constant);

        public bool Equals(Constant<TValue> other) => AutoEquality<Constant<TValue>>.Comparer.Equals(this, other);

        #endregion
    }

    public class Zero : Constant<double>
    {
        public Zero(string name) : base(name, 0.0) { }
    }

    public class One : Constant<double>
    {
        public One(string name) : base(name, 1.0) { }
    }

    public class True : Constant<bool>
    {
        public True(string name) : base(name, true) { }
    }

    public class False : Constant<bool>
    {
        public False(string name) : base(name, false) { }
    }

    public class String : Constant<string>
    {
        [JsonConstructor]
        public String(string name, string value) : base(name, value) { }
    }

    public class Double : Constant<double>
    {
        [JsonConstructor]
        public Double(string name, double value) : base(name, value) { }
    }

    /// <summary>
    /// This class provides factory methods.
    /// </summary>
    public class Constant
    {
        private static volatile int _counter;

        [NotNull]
        public static Constant<TValue> Create<TValue>(string name, TValue value) => new Constant<TValue>(name, value);

        [NotNull]
        public static Constant<TValue> Create<TValue>(TValue value) => new Constant<TValue>($"{typeof(Constant<TValue>).ToPrettyString()}{_counter++}", value);

        [NotNull, ItemNotNull]
        public static IList<Constant<TValue>> CreateMany<TValue>(string name, params TValue[] values) => values.Select(value => Create(name, value)).ToList();

        [NotNull, ItemNotNull]
        public static IList<Constant<TValue>> CreateMany<TValue>(params TValue[] values) => values.Select(Create).ToList();

        #region Predefined

        [NotNull]
        public static readonly IExpression Zero = new Zero(nameof(Zero));

        [NotNull]
        public static readonly IExpression One = new One(nameof(One));

        [NotNull]        
        public static readonly IExpression True = new True(nameof(True));

        [NotNull]
        public static readonly IExpression False = new False(nameof(False));

        [NotNull]
        public static readonly IExpression EmptyString = Create(nameof(EmptyString), string.Empty);

        [NotNull]
        public static readonly IExpression Null = Create(nameof(Null), default(object));

        #endregion
    }
}