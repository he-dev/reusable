using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public class BouncerPolicyCheckLookup<T> : ILookup<bool, BouncerPolicyCheck<T>>
    {
        private readonly ILookup<bool, BouncerPolicyCheck<T>> _results;

        public BouncerPolicyCheckLookup([CanBeNull] T value, [NotNull] ILookup<bool, BouncerPolicyCheck<T>> results)
        {
            Value = value;
            _results = results;
        }

        private T Value { get; }

        public bool Success => _results[false].Empty();

        /// <summary>
        /// Gets failed rules.
        /// </summary>
        public IEnumerable<BouncerPolicyCheck<T>> False => this[false];
        
        /// <summary>
        /// Gets successful rules.
        /// </summary>
        public IEnumerable<BouncerPolicyCheck<T>> True => this[true];

        public static implicit operator bool(BouncerPolicyCheckLookup<T> lookup) => lookup.Success;

        #region ILookup

        public int Count => _results.Count;

        public IEnumerable<BouncerPolicyCheck<T>> this[bool key] => _results[key];

        public bool Contains(bool key) => this[key].Any();

        public IEnumerator<IGrouping<bool, BouncerPolicyCheck<T>>> GetEnumerator() => _results.GetEnumerator();

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        [CanBeNull]
        public static implicit operator T(BouncerPolicyCheckLookup<T> lookup) => lookup.Value;
    }
}