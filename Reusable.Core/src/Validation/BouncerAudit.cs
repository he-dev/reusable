using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public class BouncerAudit<T> : IEnumerable<BouncerPolicyAudit<T>>, ILookup<bool, BouncerPolicyAudit<T>>
    {
        private readonly IList<BouncerPolicyAudit<T>> _results;

        public BouncerAudit([CanBeNull] T value, [NotNull] IEnumerable<BouncerPolicyAudit<T>> results)
        {
            Value = value;
            _results = results.ToList();
        }

        private T Value { get; }

        public bool Success => _results.All(result => result.IsFollowed);

        /// <summary>
        /// Gets failed rules.
        /// </summary>
        public IEnumerable<BouncerPolicyAudit<T>> False => this[false];
        
        /// <summary>
        /// Gets successful rules.
        /// </summary>
        public IEnumerable<BouncerPolicyAudit<T>> True => this[true];

        public static implicit operator bool(BouncerAudit<T> result) => result.Success;

        #region ILookup

        public int Count => _results.Count;

        public IEnumerable<BouncerPolicyAudit<T>> this[bool key] => _results.Where(x => x == key);

        public bool Contains(bool key) => this[key].Any();

        public IEnumerator<IGrouping<bool, BouncerPolicyAudit<T>>> GetEnumerator() => _results.GroupBy(x => x.IsFollowed).GetEnumerator();

        #endregion

        #region IEnumerable

        IEnumerator<BouncerPolicyAudit<T>> IEnumerable<BouncerPolicyAudit<T>>.GetEnumerator() => _results.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        [CanBeNull]
        public static implicit operator T(BouncerAudit<T> result) => result.Value;
    }
}