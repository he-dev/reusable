using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Collections
{
    internal class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        internal LambdaEqualityComparer([NotNull] Func<T, T, bool> equals, [NotNull] Func<T, int> getHashCode)
        {
            _equals = equals ?? throw new ArgumentNullException(nameof(equals));
            _getHashCode = getHashCode ?? throw new ArgumentNullException(nameof(getHashCode));
        }        

        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(null, x)) return false;
            if (ReferenceEquals(null, y)) return false;
            if (ReferenceEquals(x, y)) return true;
            return _equals(x, y);
        }

        public int GetHashCode(T obj) => _getHashCode(obj);
    }

    public static class EqualityComparerFactory<T>
    {
        /// <summary>
        /// Creates an equality-comparer with object's hash-code.
        /// </summary>
        [NotNull]
        public static IEqualityComparer<T> Create([NotNull] Func<T, T, bool> equals)
        {
            if (equals == null) throw new ArgumentNullException(nameof(equals));
            
            return Create(equals, obj => obj?.GetHashCode() ?? 0);
        }

        [NotNull]
        public static IEqualityComparer<T> Create([NotNull] Func<T, T, bool> equals, [NotNull] Func<T, int> getHashCode)
        {
            if (equals == null) throw new ArgumentNullException(nameof(equals));
            if (getHashCode == null) throw new ArgumentNullException(nameof(getHashCode));
            
            return new LambdaEqualityComparer<T>(equals, getHashCode);
        }
    }
}