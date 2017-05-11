using System;
using System.Collections;
using System.Collections.Generic;

namespace Reusable.Collections
{
    public class AutoKeyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Func<TValue, TKey> _keySelector;

        private readonly IDictionary<TKey, TValue> _values;

        public AutoKeyDictionary(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            _keySelector = keySelector;
            _values = new Dictionary<TKey, TValue>(comparer);
        }

        public AutoKeyDictionary(Func<TValue, TKey> keySelector)
            : this(keySelector, EqualityComparer<TKey>.Default)
        { }

        public AutoKeyDictionary(Func<TValue, TKey> keySelector, IDictionary<TKey, TValue> other)
            : this(keySelector)
        {
            _values = new Dictionary<TKey, TValue>(other);
        }

        public TValue this[TKey key]
        {
            get => _values[key];
            set => _values[key] = value;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(_keySelector(item.Value));

        public int Count => _values.Count;

        public bool IsReadOnly => _values.IsReadOnly;

        public ICollection<TKey> Keys => _values.Keys;

        public ICollection<TValue> Values => _values.Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _values.Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _values.Values;

        public void Add(TValue item) => _values.Add(_keySelector(item), item);

        public void Add(TKey key, TValue item) => _values.Add(_keySelector(item), item);

        public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

        public void Clear() => _values.Clear();

        public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(_keySelector(item.Value));

        public bool Contains(TValue item) => _values.ContainsKey(_keySelector(item));

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);

        public bool TryGetValue(TKey key, out TValue item) => _values.TryGetValue(key, out item);

        public bool ContainsKey(TKey key) => _values.ContainsKey(key);

        public bool ContainsKey(TValue value) => _values.ContainsKey(_keySelector(value));

        public bool Remove(TValue item) => _values.Remove(_keySelector(item));

        public bool Remove(TKey key) => _values.Remove(key);        

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static AutoKeyDictionary<TKey, TValue> operator +(AutoKeyDictionary<TKey, TValue> dic, TValue item)
        {
            dic.Add(item);
            return dic;
        }

        public static AutoKeyDictionary<TKey, TValue> operator -(AutoKeyDictionary<TKey, TValue> dic, TValue item)
        {
            dic.Remove(item);
            return dic;
        }
    }

    public class ProjectionKeyDictionary<TValue>
    {
        public static AutoKeyDictionary<TKey, TValue> Create<TKey>(Func<TValue, TKey> keySelector)
        {
            return new AutoKeyDictionary<TKey, TValue>(keySelector, new ProjectionComparer<TKey, TKey>(k => k));
        }
    }
}
