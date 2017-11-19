using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Reusable.CommandLine
{
    [PublicAPI]
    public class SoftKeySet : KeySet<SoftString>
    {
        protected SoftKeySet([NotNull] params SoftString[] keys) : base(keys)
        {
        }

        [NotNull]
        public new static readonly SoftKeySet Empty = Create(string.Empty);
        
        #region Factories

        [NotNull]
        public new static SoftKeySet Create([NotNull] IEnumerable<SoftString> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            return Create(values.ToArray());
        }

        [NotNull]
        public new static SoftKeySet Create([NotNull] params SoftString[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            return new SoftKeySet(keys);
        }
        
        [NotNull]
        public new static SoftKeySet Create([NotNull] params string[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            return SoftKeySet.Create(keys.Select(SoftString.Create));
        }

        #endregion

        public static implicit operator SoftKeySet(string value) => Create(value);
    }
}