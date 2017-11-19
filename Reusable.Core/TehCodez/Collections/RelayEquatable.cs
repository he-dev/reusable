using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Collections
{
    public class RelayEquatable<T> : IEquatable<T>
    {
        private readonly IEqualityComparer<T> _comparer;
        
        private readonly T _obj;

        public RelayEquatable([CanBeNull] T obj, [NotNull] Func<T, T, bool> equals, [NotNull] Func<T, int> getHashCode)
        {
            if (equals == null) throw new ArgumentNullException(nameof(equals));
            if (getHashCode == null) throw new ArgumentNullException(nameof(getHashCode));
            
            _obj = obj;
            _comparer = RelayEqualityComparer<T>.Create(equals, getHashCode);
        }

        public bool Equals(T other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(_obj, other)) return true;
            return _comparer.Equals(_obj, other);                       
        }

        public override bool Equals(object obj) => obj is T t && Equals(t);

        // Force custom comparison.
        public override int GetHashCode() => _comparer.GetHashCode(_obj);
    }
}