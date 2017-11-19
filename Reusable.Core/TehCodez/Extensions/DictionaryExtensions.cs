using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Tries to add the value if the key does not already exist.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="target"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> target, TKey key, TValue value)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (target.ContainsKey(key))
            {
                return false;
            }
            else
            {
                target.Add(key, value);
                return true;
            }
        }

        public static IEnumerable<(TKey Key, TValue Value)> ToTuples<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.Select(x => (x.Key, x.Value));
        }
    }
}