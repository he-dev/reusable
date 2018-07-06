using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public class DuckValidationResult<T> : List<IDuckValidationRuleResult<T>>
    {
        public DuckValidationResult([CanBeNull] T value, [NotNull] IEnumerable<IDuckValidationRuleResult<T>> results) : base(results)
        {
            Value = value;
        }

        private T Value { get; }

        public bool Success => this.All(result => result.Success);

        //public static implicit operator bool(DuckValidationResult<T> context) => context.Results.All(result => result.Success);

        [CanBeNull]
        public static implicit operator T(DuckValidationResult<T> result) => result.Value;       
    }
}