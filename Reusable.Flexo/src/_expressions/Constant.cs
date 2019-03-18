using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    }

    public class Constant<TValue> : Expression, IConstant, IEquatable<Constant<TValue>>
    {
        public Constant(SoftString name, TValue value, IExpressionContext context) : base(name ?? value.GetType().ToPrettyString(), context) => Value = value;

        [JsonConstructor]
        public Constant(SoftString name, TValue value) : this(name ?? value.GetType().ToPrettyString(), value, ExpressionContext.Empty) => Value = value;

        object IConstant.Value => Value;

        [AutoEqualityProperty]
        [CanBeNull]
        public TValue Value { get; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            //using (context.Scope(this))
            {
                return new Constant<TValue>(Name, Value, context);
            }
        }

        public void Deconstruct(out SoftString name, out TValue value, out IExpressionContext context)
        {
            name = Name;
            value = Value;
            context = Context;
        }

        public override string ToString() => $"{Name.ToString()}: '{Value}'";

        public static implicit operator Constant<TValue>((string Name, TValue Value) t) => (t.Name, t.Value, ExpressionContext.Empty);
        
        public static implicit operator Constant<TValue>((string Name, TValue Value, IExpressionContext Context) t) => new Constant<TValue>(t.Name, t.Value, t.Context);
        
        public static implicit operator Constant<TValue>((string Name, CalculateResult<TValue> Result) t) => new Constant<TValue>(t.Name, t.Result.Value, t.Result.Context);

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

        /// <summary>
        /// Creates Constant with the specified object as value or does nothing if it's already an Expression. 
        /// </summary>
        public static Func<object, IExpression> FromValueOrDefault(SoftString name)
        {
            return obj => FromValueOrDefault(name, obj);
        }

        /// <summary>
        /// Creates Constant with the specified object as value or does nothing if it's already an Expression. 
        /// </summary>
        [ContractAnnotation("obj: null => null")]
        [CanBeNull]
        public static IExpression FromValueOrDefault(SoftString name, object obj)
        {
            switch (obj)
            {
                case null: return default;
                case IExpression expression: return expression;
                default: return FromValue(name, obj);
            }
        }

        [NotNull]
        public static Constant<TValue> FromValue<TValue>(SoftString name, TValue value)
        {
            return new Constant<TValue>(name, value);
        }

        public static Constant<TValue> FromResult<TValue>(SoftString name, CalculateResult<TValue> result)
        {
            return new Constant<TValue>(name, result.Value, result.Context);
        }

        [NotNull]
        public static Constant<TValue> Create<TValue>(TValue value)
        {
            return new Constant<TValue>($"{typeof(Constant<TValue>).ToPrettyString()}{_counter++}", value);
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

        [NotNull] public static readonly IExpression True = new True(nameof(True));

        [NotNull] public static readonly IExpression False = new False(nameof(False));

        [NotNull] public static readonly IExpression EmptyString = FromValue(nameof(EmptyString), string.Empty);

        [NotNull] public static readonly IExpression Null = FromValue(nameof(Null), default(object));

        #endregion
    }
}