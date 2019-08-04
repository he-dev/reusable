using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Collections;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class Log : Dictionary<SoftString, object>, ILog
    {
        private readonly IDictionary<SoftString, object> _data = new Dictionary<SoftString, object>();

        public Log() { }

        public Log(IDictionary<SoftString, object> source) : base(source) { }

        public static ILog Empty => new Log();

        public Log SetItem(string name, object value)
        {
            if (value == Log.PropertyNames.Unset)
            {
                Remove(name);
            }
            else
            {
                this[name] = value;
            }

            return this;
        }

        /// <summary>
        /// Provides default property names.
        /// </summary>
        public static class PropertyNames
        {
            public static readonly string Logger = nameof(Logger);
            public static readonly string Category = nameof(Category);
            public static readonly string Level = nameof(Level);
            public static readonly string Message = nameof(Message);
            public static readonly string Exception = nameof(Exception);
            public static readonly string Elapsed = nameof(Elapsed);
            public static readonly string Timestamp = nameof(Timestamp);
            public static readonly string CallerMemberName = nameof(CallerMemberName);
            public static readonly string CallerLineNumber = nameof(CallerLineNumber);
            public static readonly string CallerFilePath = nameof(CallerFilePath);
            public static readonly string OverridesTransaction = nameof(OverridesTransaction);

            //public static readonly SoftString Scope = nameof(Scope);
            //public static readonly SoftString CorrelationId = nameof(CorrelationId);
            //public static readonly SoftString Context = nameof(Context);

            // This field can be used to remove a property from log.
            public static readonly object Unset = new object();
        }
    }

    public class Log2 : IEnumerable<(ItemKey<LogItemType> Key, object Value)>
    {
        private readonly IDictionary<ItemKey<LogItemType>, object> _data = new Dictionary<ItemKey<LogItemType>, object>();

        public static Log2 Empty => new Log2();
        
        public Log2 SetItem(ItemKey<LogItemType> key, object value)
        {
            _data[key] = value;
            return this;
        }

        public T GetItemOrDefault<T>(ItemKey<LogItemType> key, T defaultValue = default)
        {
            return _data.TryGetValue(key, out var obj) && obj is T value ? value : defaultValue;
        }

        public bool TryGetValue<T>(ItemKey<LogItemType> key, out T value)
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

        public bool RemoveItem(ItemKey<LogItemType> key) => _data.Remove(key);

        public IEnumerator<(ItemKey<LogItemType> Key, object Value)> GetEnumerator()
        {
            return _data.Select(x => (x.Key, x.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class Log2Extensions
    {
        public static Log2 SetItem(this Log2 log, string name, object value, LogItemType type)
        {
            return log.SetItem(new ItemKey<LogItemType>(name, type), value);
        }

        public static Log2 SetProperty(this Log2 log, string name, object value)
        {
            return log.SetItem(name, value, LogItemType.Property);
        }

        public static Log2 SetMetadata(this Log2 log, string name, object value)
        {
            return log.SetItem(name, value, LogItemType.Metadata);
        }

        public static bool TryGetProperty<T>(this Log2 log, string name, out T value)
        {
            return log.TryGetValue(new ItemKey<LogItemType>(name, LogItemType.Property), out value);
        }
        
        public static bool TryGetMetadata<T>(this Log2 log, string name, out T value)
        {
            return log.TryGetValue(new ItemKey<LogItemType>(name, LogItemType.Metadata), out value);
        }
    }

    public readonly struct ItemKey<T> : IEquatable<ItemKey<T>>
    {
        public ItemKey(SoftString name, T type)
        {
            Name = name;
            Type = type;
        }

        [AutoEqualityProperty]
        public SoftString Name { get; }

        [AutoEqualityProperty]
        public T Type { get; }

        public override int GetHashCode() => AutoEquality<ItemKey<T>>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => obj is ItemKey<T> && Equals(obj);

        public bool Equals(ItemKey<T> other) => AutoEquality<ItemKey<T>>.Comparer.Equals(this, other);
    }

    public enum LogItemType
    {
        Property,
        Metadata
    }
}