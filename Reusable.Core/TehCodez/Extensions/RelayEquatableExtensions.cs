using System;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Extensions
{
    public static class RelayEquatableExtensions
    {
        public static IEquatable<T> AsEquatable<T>(this T obj, [NotNull] Func<T, T, bool> equals)
        {
            if (equals == null) throw new ArgumentNullException(nameof(equals));
            
            return new RelayEquatable<T>(obj, equals, _ => 0);
        }
        
        public static IEquatable<T> AsEquatable<T>(this T obj, [NotNull] Func<T, T, bool> equals, [NotNull] Func<T, int> getHashCode)
        {
            if (equals == null) throw new ArgumentNullException(nameof(equals));
            if (getHashCode == null) throw new ArgumentNullException(nameof(getHashCode));

            return new RelayEquatable<T>(obj, equals, getHashCode);
        }
    }
}