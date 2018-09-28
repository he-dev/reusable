using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Commander
{
    public class SoftKeySet : ImmutableKeySet<SoftString>
    {
        public SoftKeySet(params SoftString[] keys) : base(keys) { }

        public SoftKeySet(IEnumerable<SoftString> keys) : base(keys) { }

        public override string ToString() => string.Join(", ", this.Select(x => x.ToString()));

        public static implicit operator SoftKeySet(SoftString[] keys) => keys is null ? null : new SoftKeySet(keys);

        public static implicit operator SoftKeySet(SoftString key) => key is null ? null : new SoftKeySet(key);

        public static implicit operator SoftKeySet(string key) => key is null ? null : new SoftKeySet(key);
    }

    public static class SoftKeySetExtensions
    {
        /// <summary>
        /// Gets the longest name because this one is most likely the full-name.
        /// </summary>
        [NotNull]
        public static SoftString FirstLongest([NotNull] this SoftKeySet keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            return keys.OrderByDescending(key => key.Length).First();
        }
    }
}