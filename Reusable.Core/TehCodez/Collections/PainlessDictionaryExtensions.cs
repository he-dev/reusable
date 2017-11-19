using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Collections
{
    public static class PainlessDictionaryExtensions
    {
        public static TDictionary AddRange<TKey, TValue, TDictionary>(
            [NotNull] this TDictionary dictionary,
            [NotNull] IEnumerable<(TKey Key, TValue Value)> source
        ) where TDictionary : IDictionary<TKey, TValue>
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (source == null) throw new ArgumentNullException(nameof(source));

            foreach (var item in source)
            {
                dictionary.PainlessAdd(item.Key, item.Value);
            }

            return dictionary;
        }

        public static TDictionary AddRange<TKey, TValue, TDictionary>(
            [NotNull] this TDictionary dictionary,
            [NotNull] IEnumerable<KeyValuePair<TKey, TValue>> source
        ) where TDictionary : IDictionary<TKey, TValue>
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (source == null) throw new ArgumentNullException(nameof(source));

            return dictionary.AddRange(source.Select(x => (x.Key, x.Value)));
        }

        public static TDictionary PainlessAdd<TKey, TValue, TDictionary>(
            [NotNull] this TDictionary target,
            TKey key,
            TValue value
        ) where TDictionary : IDictionary<TKey, TValue>
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            try
            {
                target.Add(key, value);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException("Dictionary key cannot be 'null'.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"The '{(key == null ? "null" : key.ToString())}' key has already been added.", ex);
            }

            return target;
        }

        public static TValue PainlessItem<TKey, TValue>(
            [NotNull] this IDictionary<TKey, TValue> source,
            TKey key)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            try
            {
                return source[key];
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException($"The '{key}' key was not present in the dictionary", ex);
            }
        }
    }
}