using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public class WeelidationResult<T> : IEnumerable<WeelidationRuleResult<T>>, ILookup<bool, WeelidationRuleResult<T>>
    {
        private readonly IList<WeelidationRuleResult<T>> _results;

        public WeelidationResult([CanBeNull] T value, [NotNull] IEnumerable<WeelidationRuleResult<T>> results)
        {
            Value = value;
            _results = results.ToList();
        }

        private T Value { get; }

        public bool Success => _results.All(result => result.Success);

        /// <summary>
        /// Gets failed rules.
        /// </summary>
        public IEnumerable<WeelidationRuleResult<T>> False => this[false];
        
        /// <summary>
        /// Gets successful rules.
        /// </summary>
        public IEnumerable<WeelidationRuleResult<T>> True => this[true];

        public static implicit operator bool(WeelidationResult<T> result) => result.Success;

        #region ILookup

        public int Count => _results.Count;

        public IEnumerable<WeelidationRuleResult<T>> this[bool key] => _results.Where(x => x == key);

        public bool Contains(bool key) => this[key].Any();

        public IEnumerator<IGrouping<bool, WeelidationRuleResult<T>>> GetEnumerator() => _results.GroupBy(x => x.Success).GetEnumerator();

        #endregion

        #region IEnumerable

        IEnumerator<WeelidationRuleResult<T>> IEnumerable<WeelidationRuleResult<T>>.GetEnumerator() => _results.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        [CanBeNull]
        public static implicit operator T(WeelidationResult<T> result) => result.Value;
    }
}