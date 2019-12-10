using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public interface IConstant : IExpression, IEnumerable<object> // , IEquatable<IConstant>
    {
        IImmutableContainer? Context { get; }

        Type ValueType { get; }

        IEnumerable<T> AsEnumerable<T>();
    }

    [JsonObject]
    public class Constant<TValue> : Expression<TValue>, IConstant //, IEquatable<Constant<TValue>>
    {
        private readonly IList<TValue> _values;

        public Constant(string id, IEnumerable<TValue> values, IImmutableContainer? context = default) : base(default)
        {
            Id = id!;
            Context = context;
            _values = values.ToList();
        }

        public Constant(string id, IImmutableContainer? context = default) : this(id, Enumerable.Empty<TValue>(), context) { }

        public static Constant<TValue> Empty(string id, IImmutableContainer? context = default) => new Constant<TValue>(id, Enumerable.Empty<TValue>(), context);

        public Type ValueType => _values.FirstOrDefault()?.GetType() ?? typeof(TValue);

        public IImmutableContainer? Context { get; }

        public IEnumerable<T> AsEnumerable<T>() => _values.Cast<T>();

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            return new Constant<TValue>(Id.ToString(), _values, context);
        }

        public override string ToString() => $"{Id}: {_values.Take(10).Join(", ").EncloseWith("[]")}";

        public static implicit operator Constant<TValue>((string Id, TValue Value) t) => new Constant<TValue>(t.Id, new[] { t.Value });

        public static implicit operator Constant<TValue>((string Id, TValue Value, IImmutableContainer Context) t) => new Constant<TValue>(t.Id, new[] { t.Value }, t.Context);

        //public static implicit operator TValue(Constant<TValue> constant) => constant.Value;

        #region IEquatable

//        public override int GetHashCode() => AutoEquality<Constant<TValue>>.Comparer.GetHashCode(this);
//        
//        public override bool Equals(object obj)
//        {
//            return obj switch
//            {
//                Constant<TValue> constant => Equals(constant),
//                IConstant constant => Equals(constant),
//                _ => false
//            };
//        }
//
//        public bool Equals(Constant<TValue> other) => AutoEquality<Constant<TValue>>.Comparer.Equals(this, other);
//
//        public bool Equals(IConstant other) => AutoEquality<IConstant>.Comparer.Equals(this, other);

        #endregion

        #region IEnumerable<T>

        IEnumerator<object> IEnumerable<object>.GetEnumerator() => _values.Cast<object>().GetEnumerator();

        //IEnumerator<object> IEnumerable<object>.GetEnumerator() => _values.Cast<object>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_values).GetEnumerator();

        #endregion
    }

    /// <summary>
    /// This class provides factory methods.
    /// </summary>
    public static class Constant
    {
        [NotNull]
        public static Constant<TValue> Single<TValue>(string name, TValue value, IImmutableContainer? context = default)
        {
            return new Constant<TValue>(name, new[] { value }, context);
        }

        [NotNull]
        internal static Constant<TValue> Single<TValue>(TValue value, IImmutableContainer? context = default)
        {
            return Single($"{typeof(Constant<TValue>).ToPrettyString()}", value, context);
        }

        [NotNull, ItemNotNull]
        internal static IEnumerable<IExpression> CreateMany<TValue>(string name, params TValue[] values)
        {
            return values.Select(value => Single(name, value));
        }

        [NotNull, ItemNotNull]
        internal static IEnumerable<IExpression> CreateMany<TValue>(params TValue[] values)
        {
            return values.Select((x, i) => Single($"{x?.ToString()}[{i}]"));
        }

        public static Constant<IEnumerable<IExpression>> FromEnumerable(string name, IEnumerable<object> values, IImmutableContainer? context = default)
        {
            return Single(name, values.Select((x, i) => x is IConstant constant ? constant : Single($"{name}.Items[{i}]", x)).ToList().Cast<IExpression>(), context);
        }

        #region Predefined

        [NotNull]
        public static readonly IExpression Zero = Single(nameof(Zero), 0.0);

        [NotNull]
        public static readonly IExpression One = Single(nameof(One), 1.0);

        [NotNull]
        public static readonly IExpression True = Single(nameof(True), true);

        [NotNull]
        public static readonly IExpression False = Single(nameof(False), false);

        [NotNull]
        public static readonly IExpression EmptyString = Single(nameof(EmptyString), string.Empty);

        [NotNull]
        public static readonly IExpression Null = Single(nameof(Null), default(object));

        [NotNull]
        public static readonly IExpression Unit = Single(nameof(Unit), default(object));

        public static readonly IExpression DefaultComparer = Single(Filter.Properties.Comparer, "Default");

        #endregion

        public static bool HasContext(this IConstant constant) => constant.Context.IsNotNull();
    }
}