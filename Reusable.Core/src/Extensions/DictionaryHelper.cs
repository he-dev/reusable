using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Collections
{
    public static class DictionaryHelper
    {
        public static TValue GetItemSafely<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            try
            {
                return dictionary[key];
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException($"The '{key}' key was not present in the dictionary", ex);
            }
        }

        public static void AddSafely<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            try
            {
                dictionary.Add(key, value);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException("Dictionary key cannot be 'null'.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"The '{(key == null ? "null" : key.ToString())}' key has already been added.", ex);
            }
        }

        public static Dictionary<TKey, TSource> ToDictionarySafely<TSource, TKey>(
            this IEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            return source.ToDictionarySafely(keySelector, x => x, EqualityComparer<TKey>.Default);
        }

        public static Dictionary<TKey, TSource> ToDictionarySafely<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer
        )
        {
            return source.ToDictionarySafely(keySelector, x => x, comparer);
        }

        public static Dictionary<TKey, TElement> ToDictionarySafely<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector
        )
        {
            return source.ToDictionarySafely(keySelector, elementSelector, EqualityComparer<TKey>.Default);
        }

        public static Dictionary<TKey, TElement> ToDictionarySafely<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer
        )
        {
            var dictionary = new Dictionary<TKey, TElement>(comparer);

            foreach (var item in source)
            {
                dictionary.AddSafely(keySelector(item), elementSelector(item));
            }

            return dictionary;
        }

        public static T AddRangeSafely<T, TKey, TValue>(this T dictionary, IEnumerable<KeyValuePair<TKey, TValue>> values) where T : IDictionary<TKey, TValue>
        {
            foreach (var pair in values)
            {
                dictionary.AddSafely(pair.Key, pair.Value);
            }

            return dictionary;
        }

        public static T AddRangeSafely<T, TKey, TValue>(this T dictionary, IEnumerable<(TKey Key, TValue Value)> values) where T : IDictionary<TKey, TValue>
        {
            foreach (var (key, value) in values)
            {
                dictionary.AddSafely(key, value);
            }

            return dictionary;
        }
    } 
}