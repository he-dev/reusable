using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Flexo
{
    public static class EnumerableExtensions
    {
        public static object ToList(this IEnumerable<object> values, Type enumerableType)
        {
            // The new collection cannot be assigned without casting because it's IEnumerable<object> and we need IEnumerable<T>.
            var cast = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast)).MakeGenericMethod(enumerableType);
            var casted = cast.Invoke(null, new object[] { values });
            var toList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList)).MakeGenericMethod(enumerableType);
            return toList.Invoke(null, new object[] { casted });
        }
    }
}