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
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Abstractions.Data
{
    public class LogEntry : IEnumerable<KeyValuePair<ItemKey<SoftString>, object>>
    {
        public static readonly string DefaultItemTag = Tags.Loggable;

        private readonly IDictionary<ItemKey<SoftString>, object> _data;

        [DebuggerStepThrough]
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

        public bool ContainsKey(SoftString name, SoftString tag) => _data.ContainsKey((name, tag ?? DefaultItemTag));

        public LogEntry SetItem(ItemKey<SoftString> key, object value)
        {
            this[key] = value;
            return this;
        }

        public LogEntry SetItem(SoftString name, SoftString tag, object value)
        {
            this[name, tag ?? DefaultItemTag] = value;
            return this;
        }

        public T GetItemOrDefault<T>(SoftString name, SoftString tag, T defaultValue = default)
        {
            return _data.TryGetValue((name, tag ?? DefaultItemTag), out var obj) && obj is T value ? value : defaultValue;
        }

        public bool TryGetItem<T>(ItemKey<SoftString> key, out T value)
        {
            if (_data.TryGetValue(key, out var obj))
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

        public bool TryGetItem<T>(SoftString name, SoftString tag, out T value)
        {
            return TryGetItem((name, tag ?? DefaultItemTag), out value);
        }

        public bool RemoveItem(SoftString name, SoftString tag) => _data.Remove((name, tag ?? DefaultItemTag));

        public override string ToString()
        {
            return @"[{Timestamp:HH:mm:ss:fff}] [{Logger:u}] {Message}".Format(this);
        }

        public IEnumerator<KeyValuePair<ItemKey<SoftString>, object>> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
        public static class Names
        {
            public static readonly string Timestamp = nameof(Timestamp);
            public static readonly string Logger = nameof(Logger);
            public static readonly string Level = nameof(Level);
            public static readonly string Message = nameof(Message);
            public static readonly string Exception = nameof(Exception);
            public static readonly string CallerMemberName = nameof(CallerMemberName);
            public static readonly string CallerLineNumber = nameof(CallerLineNumber);
            public static readonly string CallerFilePath = nameof(CallerFilePath);
            
            public static readonly string Object = nameof(Object);
            public static readonly string Snapshot = nameof(Snapshot);
            
            public static readonly string Scope = nameof(Scope);
        }

        public static class Tags
        {
            /// <summary>
            /// Item is suitable for logging.
            /// </summary>
            public static readonly string Loggable = nameof(Loggable);

            //public static readonly string Metadata = nameof(Metadata);

            /// <summary>
            /// Item needs to be exploded.
            /// </summary>
            public static readonly string Explodable = nameof(Explodable);
            
            /// <summary>
            /// Item needs to be mapped or serialized.
            /// </summary>
            public static readonly string Serializable = nameof(Serializable);

            public static readonly string Copyable = nameof(Copyable);
        }
    }

    public static class LogEntryExtensions
    {
        public static IEnumerable<ItemKey<SoftString>> Keys(this LogEntry logEntry) => logEntry.Select(le => le.Key);
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