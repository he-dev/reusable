using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public class ExpressValidationResultLookup<T> : ILookup<bool, ExpressValidationResult<T>>
    {
        private readonly ILookup<bool, ExpressValidationResult<T>> _results;

        public ExpressValidationResultLookup([CanBeNull] T value, [NotNull] ILookup<bool, ExpressValidationResult<T>> results)
        {
            Value = value;
            _results = results;
        }

        private T Value { get; }

        public bool Success => _results[false].Empty();

        /// <summary>
        /// Gets failed rules.
        /// </summary>
        public IEnumerable<ExpressValidationResult<T>> False => this[false];
        
        /// <summary>
        /// Gets successful rules.
        /// </summary>
        public IEnumerable<ExpressValidationResult<T>> True => this[true];

        public static implicit operator bool(ExpressValidationResultLookup<T> lookup) => lookup.Success;

        #region ILookup

        public int Count => _results.Count;

        public IEnumerable<ExpressValidationResult<T>> this[bool key] => _results[key];

        public bool Contains(bool key) => this[key].Any();

        public IEnumerator<IGrouping<bool, ExpressValidationResult<T>>> GetEnumerator() => _results.GetEnumerator();

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        [CanBeNull]
        public static implicit operator T(ExpressValidationResultLookup<T> lookup) => lookup.Value;
    }
}