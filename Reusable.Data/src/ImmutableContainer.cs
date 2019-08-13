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
//    public interface IImmutableSession<T> : IEnumerable<(SoftString Key, T Value)>
//    {
//        T this[SoftString key] { get; }
//
//        int Count { get; }
//
//        bool ContainsKey(SoftString key);
//
//        IImmutableSession<T> SetItem(SoftString key, T value);
//
//        bool TryGetValue(SoftString key, out T value);
//    }

    // With a 'struct' we don't need any null-checks.
//    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
//    [PublicAPI]
//    public class ImmutableSession<T> : IImmutableSession<T>
//    {
//        private readonly IImmutableDictionary<SoftString, T> _data;
//
//        public ImmutableSession([NotNull] IImmutableDictionary<SoftString, T> data) => _data = data ?? throw new ArgumentNullException(nameof(data));
//
//        public static ImmutableSession<T> Empty => new ImmutableSession<T>(ImmutableDictionary<SoftString, T>.Empty);
//
//        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
//        {
//            builder.DisplayValue(x => x.Count);
//            builder.DisplayValues(x => x.Select(y => y.Key));
//        });
//
//        // A struct cannot be initialized so the field remains null when using 'default'.
//        protected IImmutableDictionary<SoftString, T> Data => _data ?? ImmutableDictionary<SoftString, T>.Empty;
//
//        public T this[SoftString key] => Data[key];
//
//        public int Count => Data.Count;
//
//        [DebuggerStepThrough]
//        public bool ContainsKey(SoftString key) => Data.ContainsKey(key);
//
//        [DebuggerStepThrough]
//        public bool TryGetValue(SoftString key, out T value) => Data.TryGetValue(key, out value);
//
//        [DebuggerStepThrough]
//        [MustUseReturnValue]
//        public IImmutableSession<T> Add(SoftString key, T value) => new ImmutableSession<T>(Data.Add(key, value));
//
//        [DebuggerStepThrough]
//        [MustUseReturnValue]
//        public IImmutableSession<T> SetItem(SoftString key, T value) => new ImmutableSession<T>(Data.SetItem(key, value));
//
//        public IEnumerator<(SoftString Key, T Value)> GetEnumerator() => Data.Select(x => (x.Key, x.Value)).GetEnumerator();
//
//        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Data).GetEnumerator();
//    }


    public interface IImmutableContainer : IEnumerable<(string Key, object Value)>
    {
        [CanBeNull]
        object this[string key] { get; }

        int Count { get; }

        [NotNull]
        IImmutableContainer SetItem(string key, object value);

        bool ContainsKey(string key);

        bool TryGetItem(string key, out object value);

        [NotNull]
        IImmutableContainer RemoveItem(string key);
    }

    [PublicAPI]
    public class ImmutableContainer : IImmutableContainer
    {
        private readonly IImmutableDictionary<string, object> _data;

        public ImmutableContainer([NotNull] IImmutableDictionary<string, object> data) => _data = data ?? throw new ArgumentNullException(nameof(data));

        [NotNull]
        public static IImmutableContainer Empty => new ImmutableContainer(ImmutableDictionary.Create<string, object>(SoftString.Comparer));

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayScalar(x => x.Count);
            builder.DisplayEnumerable(x => x.Select(y => y.Key));
        });

        public object this[string key] => _data[key];

        public int Count => _data.Count;

        [DebuggerStepThrough]
        public bool ContainsKey(string key) => _data.ContainsKey(key);

        [DebuggerStepThrough]
        public bool TryGetItem(string key, out object value) => _data.TryGetValue(key, out value);

        [DebuggerStepThrough]
        [MustUseReturnValue]
        public IImmutableContainer SetItem(string key, object value) => _data.ContainsKey(key) ? this : new ImmutableContainer(_data.SetItem(key, value));

        public IImmutableContainer RemoveItem(string key) => new ImmutableContainer(_data.Remove(key));

        public IEnumerator<(string Key, object Value)> GetEnumerator() => _data.Select(x => (x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();
    }
}