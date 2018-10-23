using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Collections
{
    internal class EquatableAdapter<T> : IEquatable<T>
    {
        private readonly IEqualityComparer<T> _comparer;

        private readonly T _obj;

        public EquatableAdapter([CanBeNull] T obj, [NotNull] IEqualityComparer<T> comparer)
        {
            _obj = obj;
            _comparer = comparer;
        }

        public bool Equals(T other) => _comparer.Equals(_obj, other);

        public override bool Equals(object obj) => obj is T t && Equals(t);

        public override int GetHashCode() => _comparer.GetHashCode(_obj);
    }

    public static class ObjectExtensions
    {
        public static IEquatable<T> AsEquatable<T>([CanBeNull] T obj, [NotNull] Func<T, T, bool> equals, [NotNull] Func<T, int> getHashCode)
        {
            if (equals == null) throw new ArgumentNullException(nameof(equals));
            if (getHashCode == null) throw new ArgumentNullException(nameof(getHashCode));

            return new EquatableAdapter<T>(obj, EqualityComparerFactory<T>.Create(equals, getHashCode));
        }
        
        public static IEquatable<T> AsEquatable<T>([CanBeNull] T obj, [NotNull] Func<T, T, bool> equals)
        {
            if (equals == null) throw new ArgumentNullException(nameof(equals));

            return new EquatableAdapter<T>(obj, EqualityComparerFactory<T>.Create(equals));
        }
    }
}