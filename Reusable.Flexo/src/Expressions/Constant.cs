using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Collections;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Extensions;

namespace Reusable.Flexo.Expressions
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
        public override string Name => base.Name;

        [AutoEqualityProperty]
        [CanBeNull]
        public TValue Value { get; }

        object IConstant.Value => Value;

        public override IExpression Invoke(IExpressionContext context)
        {
            using (context.Scope(this))
            {
                return this.Log();
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

    public class One : Constant<double>
    {
        public One(string name) : base(name, 1.0) { }
    }

    public class Zero : Constant<double>
    {
        public Zero(string name) : base(name, 0.0) { }
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

    /// <summary>
    /// This class provides factory methods.
    /// </summary>
    public class Constant
    {
        public static Constant<TValue> Create<TValue>(string name, TValue value) => new Constant<TValue>(name, value);

        public static IList<Constant<TValue>> CreateMany<TValue>(string name, params TValue[] values) => values.Select(value => Create(name, value)).ToList();
    }
}