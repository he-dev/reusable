using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public class ValidationContext<T>
    {
        public ValidationContext(T obj, [NotNull] IList<IValidationResult<T>> results)
        {
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }
            //if (results == null) { throw new ArgumentNullException(nameof(results)); }

            Object = obj;
            Results = results;
        }

        [CanBeNull]
        public T Object { get; }

        [NotNull]
        public IList<IValidationResult<T>> Results { get; }

        public static implicit operator bool(ValidationContext<T> context) => context.Results.All(result => result.Success);
    }
}