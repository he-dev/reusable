using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;

// ReSharper disable once CheckNamespace
namespace Reusable
{
    /// <summary>
    /// Name set used for command and argument names.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    // ReSharper disable once InheritdocConsiderUsage
    public class KeySet<TKey> : IImmutableSet<TKey>, IEquatable<IImmutableSet<TKey>> where TKey : IEquatable<TKey>
    {
        [NotNull]
        private static readonly IEqualityComparer<IImmutableSet<TKey>> Comparer;

        [NotNull]
        private readonly IImmutableSet<TKey> _keys;

        static KeySet()
        {
            Comparer = RelayEqualityComparer<IImmutableSet<TKey>>.CreateWithoutHashCode((left, right) => left.Overlaps(right) || (left.Empty() && right.Empty()));
        }

        protected KeySet([NotNull] params TKey[] keys) => _keys = ImmutableHashSet.Create(keys);

        private string DebuggerDisplay => ToString();

        [NotNull]
        public static readonly KeySet<TKey> Empty = Create();

        #region Factories

        [NotNull]
        public static KeySet<TKey> Create([NotNull] IEnumerable<TKey> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            return Create(values.ToArray());
        }

        [NotNull]
        public static KeySet<TKey> Create([NotNull] params TKey[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            return new KeySet<TKey>(keys);
        }

        #endregion

        #region IImmutableSet

        public int Count => _keys.Count;
        public IImmutableSet<TKey> Clear() => _keys.Clear();
        public bool Contains(TKey value) => _keys.Contains(value);
        public IImmutableSet<TKey> Add(TKey value) => _keys.Add(value);
        public IImmutableSet<TKey> Remove(TKey value) => _keys.Remove(value);
        public bool TryGetValue(TKey equalValue, out TKey actualValue) => _keys.TryGetValue(equalValue, out actualValue);
        public IImmutableSet<TKey> Intersect(IEnumerable<TKey> other) => _keys.Intersect(other);
        public IImmutableSet<TKey> Except(IEnumerable<TKey> other) => _keys.Except(other);
        public IImmutableSet<TKey> SymmetricExcept(IEnumerable<TKey> other) => _keys.SymmetricExcept(other);
        public IImmutableSet<TKey> Union(IEnumerable<TKey> other) => _keys.Union(other);
        public bool SetEquals(IEnumerable<TKey> other) => _keys.SetEquals(other);
        public bool IsProperSubsetOf(IEnumerable<TKey> other) => _keys.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<TKey> other) => _keys.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<TKey> other) => _keys.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<TKey> other) => _keys.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<TKey> other) => _keys.Overlaps(other);
        public IEnumerator<TKey> GetEnumerator() => _keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _keys.GetEnumerator();

        #endregion

        #region IEquatable

        public bool Equals(IImmutableSet<TKey> other) => Comparer.Equals(this, other);

        public override bool Equals(object obj) => (obj is KeySet<TKey> keys && Equals(keys));

        public override int GetHashCode() => Comparer.GetHashCode(this);

        #endregion

        public override string ToString() => this.Join(", ").EncloseWith("[]");

        #region Operators

        public static bool operator ==(KeySet<TKey> left, KeySet<TKey> right) => Comparer.Equals(left, right);

        public static bool operator !=(KeySet<TKey> left, KeySet<TKey> right) => !(left == right);

        #endregion       
    }
}