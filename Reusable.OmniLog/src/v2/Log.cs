using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Collections;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.v2.Experimental
{
    public class Log : IEnumerable<(ItemKey<LogItemTag> Key, object Value)>
    {
        private readonly IDictionary<ItemKey<LogItemTag>, object> _data = new Dictionary<ItemKey<LogItemTag>, object>();

        public Log() { }

        private Log(IDictionary<ItemKey<LogItemTag>, object> data)
        {
            _data = new Dictionary<ItemKey<LogItemTag>, object>(data);
        }

        public static Log Empty => new Log();

        public Log Copy() => new Log(_data);

        public Log SetItem((string Name, LogItemTag Tag) key, object value)
        {
            _data[key] = value;
            return this;
        }

        public T GetItemOrDefault<T>((string Name, LogItemTag Tag) key, T defaultValue = default)
        {
            return _data.TryGetValue(key, out var obj) && obj is T value ? value : defaultValue;
        }

        public bool TryGetItem<T>((string Name, LogItemTag Tag) key, out T value)
        {
            if (_data.TryGetValue(key, out var obj) && obj is T result)
            {
                value = result;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public bool RemoveItem((string Name, LogItemTag Tag) key) => _data.Remove(key);

        public IEnumerator<(ItemKey<LogItemTag> Key, object Value)> GetEnumerator()
        {
            return _data.Select(x => (x.Key, x.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class LogExtensions
    {
        public static Log SetProperty(this Log log, string name, object value)
        {
            return log.SetItem((name, LogItemTag.Property), value);
        }

        public static Log SetMetadata(this Log log, string name, object value)
        {
            return log.SetItem((name, LogItemTag.Metadata), value);
        }

        public static bool TryGetProperty<T>(this Log log, string name, out T value)
        {
            return log.TryGetItem((name, LogItemTag.Property), out value);
        }

        public static bool TryGetMetadata<T>(this Log log, string name, out T value)
        {
            return log.TryGetItem((name, LogItemTag.Metadata), out value);
        }
    }

    public readonly struct ItemKey<T> : IEquatable<ItemKey<T>> where T : Enum
    {
        public ItemKey(SoftString name, T tag)
        {
            Name = name;
            Tag = tag;
        }

        [AutoEqualityProperty]
        public SoftString Name { get; }

        [AutoEqualityProperty]
        public T Tag { get; }

        public override int GetHashCode() => AutoEquality<ItemKey<T>>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => obj is ItemKey<T> && Equals(obj);

        public bool Equals(ItemKey<T> other) => AutoEquality<ItemKey<T>>.Comparer.Equals(this, other);

        public static implicit operator ItemKey<T>((string name, T tag) key) => new ItemKey<T>(key.name, key.tag);
    }

    public enum LogItemTag
    {
        Property,
        Metadata, // These items need to be processed before they can be a 'Property'
    }

    public static class LogItemTags
    {
        /// <summary>
        /// These items are ready to be logged.
        /// </summary>
        public static readonly string Property = nameof(Property);
        
        /// <summary>
        /// These items need to be processed before they can be a 'Property'
        /// </summary>
        public static readonly string Metadata = nameof(Metadata);
        
        /// <summary>
        /// These items need to be processed by LoggerSerialize before they can be a 'Property'
        /// </summary>
        public static readonly string Serializable = nameof(Serializable);
        
        
    }
}