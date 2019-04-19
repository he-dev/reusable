using System;
using System.Collections.Generic;

namespace Reusable.Collections
{
    public static class ProjectionEqualityComparer<T>
    {
        public static IEqualityComparer<T> Create<TProjection>(Func<T, TProjection> projection)
        {
            var comparer = EqualityComparer<TProjection>.Default;

            return EqualityComparerFactory<T>.Create(
                getHashCode: obj => comparer.GetHashCode(projection(obj)),
                equals: (x, y) => comparer.Equals(projection(x), projection(y))
            );
        }
    }
}