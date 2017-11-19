using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Collections
{
    public class PainlessDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;

        public static readonly IEqualityComparer<TKey> DefaultComparer = EqualityComparer<TKey>.Default;

        public PainlessDictionary([NotNull] IEnumerable<(TKey Key, TValue Value)> source, [NotNull] IEqualityComparer<TKey> comparer)
        : this(comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            _dictionary.AddRange(source);
        }

        public PainlessDictionary([NotNull] IEqualityComparer<TKey> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            _dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public PainlessDictionary([NotNull] IEnumerable<KeyValuePair<TKey, TValue>> source)
        : this(source, DefaultComparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
        }

        public PainlessDictionary([NotNull] IEnumerable<KeyValuePair<TKey, TValue>> source, [NotNull] IEqualityComparer<TKey> comparer)
        : this(comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            _dictionary.AddRange(source);
        }

        public PainlessDictionary()
        : this(DefaultComparer)
        { }

        public PainlessDictionary(IEnumerable<(TKey Key, TValue Value)> source)
            : this(source, DefaultComparer)
        { }

        public TValue this[TKey key]
        {
            get => _dictionary.PainlessItem(key);
            set => _dictionary[key] = value;
        }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;

        public void Add(TKey key, TValue value) => _dictionary.PainlessAdd(key, value);

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Clear() => _dictionary.Clear();

        public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _dictionary.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        public bool Remove(TKey key) => _dictionary.Remove(key);

        public bool Remove(KeyValuePair<TKey, TValue> item) => _dictionary.Remove(item);

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        public IDictionary<TKey, TValue> ToDictionary() => new Dictionary<TKey, TValue>(_dictionary);
    }
}
