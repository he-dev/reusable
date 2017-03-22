using System;
using System.Collections.Generic;

namespace Reusable.Collections
{
    public class AutoEqualityComparerFactory<TArg>
    {
        public static IEqualityComparer<TArg> Create<TProjection>(Func<TArg, TProjection> projection)
        {
            return new AutoEqualityComparer<TArg, TProjection>(projection);
        }
    }
}