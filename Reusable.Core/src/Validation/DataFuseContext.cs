using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public class DataFuseContext<T>
    {
        public DataFuseContext(T obj, [NotNull] IList<IDataFuseResult<T>> results)
        {
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }
            //if (results == null) { throw new ArgumentNullException(nameof(results)); }

            Object = obj;
            Results = results;
        }

        [CanBeNull]
        public T Object { get; }

        [NotNull]
        public IList<IDataFuseResult<T>> Results { get; }

        public static implicit operator bool(DataFuseContext<T> context) => context.Results.All(result => result.Success);
    }
}