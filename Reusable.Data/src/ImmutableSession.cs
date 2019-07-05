using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Data
{
    public interface IImmutableSession<T> : IEnumerable<(SoftString Key, T Value)>
    {
        T this[SoftString key] { get; }

        int Count { get; }

        bool ContainsKey(SoftString key);

        IImmutableSession<T> SetItem(SoftString key, T value);

        bool TryGetValue(SoftString key, out T value);
    }

    public interface IImmutableSession : IEnumerable<(SoftString Key, object Value)>
    {
        [CanBeNull]
        object this[SoftString key] { get; }

        int Count { get; }

        bool ContainsKey(SoftString key);

        bool TryGetValue(SoftString key, out object value);

        //IImmutableSession SetItem(SoftString key, object value);

        [NotNull]
        IImmutableSession SetItem<T>(SoftString key, T value, bool replace = true);
    }

    // With a 'struct' we don't need any null-checks.
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    [PublicAPI]
    public class ImmutableSession<T> : IImmutableSession<T>
    {
        private readonly IImmutableDictionary<SoftString, T> _data;

        public ImmutableSession([NotNull] IImmutableDictionary<SoftString, T> data) => _data = data ?? throw new ArgumentNullException(nameof(data));

        public static ImmutableSession<T> Empty => new ImmutableSession<T>(ImmutableDictionary<SoftString, T>.Empty);

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x.Count);
            builder.DisplayValues(x => x.Select(y => y.Key));
        });

        // A struct cannot be initialized so the field remains null when using 'default'.
        protected IImmutableDictionary<SoftString, T> Data => _data ?? ImmutableDictionary<SoftString, T>.Empty;

        public T this[SoftString key] => Data[key];

        public int Count => Data.Count;

        [DebuggerStepThrough]
        public bool ContainsKey(SoftString key) => Data.ContainsKey(key);

        [DebuggerStepThrough]
        public bool TryGetValue(SoftString key, out T value) => Data.TryGetValue(key, out value);

        [DebuggerStepThrough]
        [MustUseReturnValue]
        public IImmutableSession<T> Add(SoftString key, T value) => new ImmutableSession<T>(Data.Add(key, value));

        [DebuggerStepThrough]
        [MustUseReturnValue]
        public IImmutableSession<T> SetItem(SoftString key, T value) => new ImmutableSession<T>(Data.SetItem(key, value));

        public IEnumerator<(SoftString Key, T Value)> GetEnumerator() => Data.Select(x => (x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Data).GetEnumerator();
    }

    [PublicAPI]
    public class ImmutableSession : IImmutableSession
    {
        private readonly IImmutableDictionary<SoftString, object> _data;

        public ImmutableSession([NotNull] IImmutableDictionary<SoftString, object> data) => _data = data ?? throw new ArgumentNullException(nameof(data));

        public static IImmutableSession Empty => new ImmutableSession(ImmutableDictionary<SoftString, object>.Empty);

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x.Count);
            builder.DisplayValues(x => x.Select(y => y.Key));
        });

        public object this[SoftString key] => _data[key];

        public int Count => _data.Count;

        [DebuggerStepThrough]
        public bool ContainsKey(SoftString key) => _data.ContainsKey(key);

        [DebuggerStepThrough]
        public bool TryGetValue(SoftString key, out object value) => _data.TryGetValue(key, out value);

        [DebuggerStepThrough]
        [MustUseReturnValue]
        //public IImmutableSession SetItem(SoftString key, object value) => new ImmutableSession(_data.Remove(key).SetItem(key, value));
        public IImmutableSession SetItem<T>(SoftString key, T value, bool replace = true)
        {
            if (replace)
            {
                return new ImmutableSession(_data.Remove(key).SetItem(key, value));
            }
            else
            {
                return _data.ContainsKey(key) ? this : new ImmutableSession(_data.SetItem(key, value));
            }
        }

        public IEnumerator<(SoftString Key, object Value)> GetEnumerator()
        {
            return _data.Select(x => (x.Key, x.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();
    }

//    public class SetItemMode : Option<SetItemMode>
//    {
//        public SetItemMode(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }
//
//        public static readonly SetItemMode Overwrite = CreateWithCallerName();
//    }
}