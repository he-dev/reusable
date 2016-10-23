using System;
using System.Collections;
using System.Collections.Generic;

namespace Reusable.Collections
{
    public class AutoKeyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly Func<TValue, TKey> _keySelector;

        private readonly IEqualityComparer<TKey> _comparer = EqualityComparer<TKey>.Default;

        private readonly IDictionary<TKey, TValue> _values;

        public AutoKeyDictionary(Func<TValue, TKey> keySelector)
        {
            _keySelector = keySelector;
            _values = new Dictionary<TKey, TValue>();
        }

        public AutoKeyDictionary(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
        : this(keySelector)
        {
            _comparer = equalityComparer;
            _values = new Dictionary<TKey, TValue>(equalityComparer);
        }

        public AutoKeyDictionary(Func<TValue, TKey> keySelector, IDictionary<TKey, TValue> other)
        : this(keySelector)
        {
            _keySelector = keySelector;
            _values = new Dictionary<TKey, TValue>(other);
        }

        public TValue this[TKey key]
        {
            get { return _values[key]; }
            set { _values[key] = value; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            ValidateKeyValuePair(item);
            return Remove(_keySelector(item.Value));
        }

        public int Count => _values.Count;

        public bool IsReadOnly => _values.IsReadOnly;

        public ICollection<TKey> Keys => _values.Keys;

        public ICollection<TValue> Values => _values.Values;

        public void Add(TValue item) => _values.Add(_keySelector(item), item);

        public void Add(TKey key, TValue item)
        {
            ValidateKeyValuePair(new KeyValuePair<TKey, TValue>(key, item));
            _values.Add(_keySelector(item), item);
        }

        public void Add(KeyValuePair<TKey, TValue> kvp)
        {
            Add(kvp.Key, kvp.Value);
        }

        public void Clear() => _values.Clear();

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            ValidateKeyValuePair(item);
            return ContainsKey(_keySelector(item.Value));
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public bool TryGetValue(TKey key, out TValue item) => _values.TryGetValue(key, out item);

        public bool Contains(TValue item) => _values.ContainsKey(_keySelector(item));

        public bool ContainsKey(TKey key) => _values.ContainsKey(key);

        public bool Remove(TValue item) => _values.Remove(_keySelector(item));

        public bool Remove(TKey key) => _values.Remove(key);

        private void ValidateKeyValuePair(KeyValuePair<TKey, TValue> item)
        {
            if (!_comparer.Equals(item.Key, _keySelector(item.Value)))
            {
                throw new ArgumentException("Key/Value mismatch.");
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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
}
