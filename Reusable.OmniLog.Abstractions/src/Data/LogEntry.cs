using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Abstractions.Data
{
    public class LogEntry : IEnumerable<KeyValuePair<ItemKey<SoftString>, object>>
    {
        public static readonly string DefaultItemTag = "Property";

        private readonly IDictionary<ItemKey<SoftString>, object> _data;

        public LogEntry()
        {
            _data = new Dictionary<ItemKey<SoftString>, object>();
        }

        private LogEntry(IDictionary<ItemKey<SoftString>, object> data)
        {
            _data = new Dictionary<ItemKey<SoftString>, object>(data);
        }

        public static LogEntry Empty() => new LogEntry();

        public object this[ItemKey<SoftString> key]
        {
            get => _data[key];
            set => _data[key] = value;
        }
        
        public object this[SoftString name]
        {
            get => this[name, DefaultItemTag];
            set => this[name, DefaultItemTag] = value;
        }

        public object this[SoftString name, SoftString tag]
        {
            get => _data[(name, tag)];
            set => _data[(name, tag)] = value;
        }

        public LogEntry Clone() => new LogEntry(_data);

        public LogEntry SetItem(SoftString name, SoftString tag, object value)
        {
            this[name, tag ?? DefaultItemTag] = value;
            return this;
        }

        public T GetItemOrDefault<T>(SoftString name, SoftString tag, T defaultValue = default)
        {
            return _data.TryGetValue((name, tag ?? DefaultItemTag), out var obj) && obj is T value ? value : defaultValue;
        }

        public bool TryGetItem<T>(SoftString name, SoftString tag, out T value)
        {
            if (_data.TryGetValue((name, tag ?? DefaultItemTag), out var obj))
            {
                switch (obj)
                {
                    case T t:
                        value = t;
                        return true;

                    case object o:
                        value = (T)o;
                        return true;

                    default:
                        value = default;
                        return false;
                }
            }
            else
            {
                value = default;
                return false;
            }
        }

        public bool RemoveItem(SoftString name, SoftString tag) => _data.Remove((name, tag ?? DefaultItemTag));

        public IEnumerator<KeyValuePair<ItemKey<SoftString>, object>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
        public static class BasicPropertyNames
        {
            public static readonly string Logger = nameof(Logger);
            public static readonly string Level = nameof(Level);
            public static readonly string Message = nameof(Message);
            public static readonly string Exception = nameof(Exception);
            public static readonly string Timestamp = nameof(Timestamp);
            public static readonly string CallerMemberName = nameof(CallerMemberName);
            public static readonly string CallerLineNumber = nameof(CallerLineNumber);
            public static readonly string CallerFilePath = nameof(CallerFilePath);
        }

        public static class ItemTags
        {
            public static readonly string Metadata = nameof(Metadata);
        }
    }

    public static class LogExtensions
    {
        public static LogEntry SetProperty(this LogEntry logEntry, SoftString name, object value)
        {
            return logEntry.SetItem(name, default, value);
        }

        public static bool TryGetProperty<T>(this LogEntry logEntry, SoftString name, out T value)
        {
            return logEntry.TryGetItem(name, default, out value);
        }
    }

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public readonly struct ItemKey<T> : IEquatable<ItemKey<T>>
    {
        public ItemKey(SoftString name, T tag)
        {
            Name = name;
            Tag = tag;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayScalar(x => x.Name.ToString());
            b.DisplayScalar(x => x.Tag.ToString());
        });

        [AutoEqualityProperty]
        public SoftString Name { get; }

        [AutoEqualityProperty]
        public T Tag { get; }

        public override int GetHashCode() => AutoEquality<ItemKey<T>>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => obj is ItemKey<T> key && Equals(key);

        public bool Equals(ItemKey<T> other) => AutoEquality<ItemKey<T>>.Comparer.Equals(this, other);

        public override string ToString() => $"{Name.ToString()}#{Tag.ToString()}";

        public void Deconstruct(out string name, out T tag)
        {
            name = Name.ToString();
            tag = Tag;
        }

        public static implicit operator ItemKey<T>((SoftString name, T tag) key) => new ItemKey<T>(key.name, key.tag);
    }
}