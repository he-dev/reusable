using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Diagnostics;

namespace Reusable.Data
{
    public interface IImmutableContainer : IEnumerable<(string Key, object Value)>
    {
        object? this[string key] { get; }

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

        public ImmutableContainer(IImmutableDictionary<string, object> data) => _data = data ?? throw new ArgumentNullException(nameof(data));

        [NotNull]
        public static IImmutableContainer Empty => new ImmutableContainer(ImmutableDictionary.Create<string, object>(SoftString.Comparer));

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplaySingle(x => x.Count);
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
        public IImmutableContainer SetItem(string key, object value) => new ImmutableContainer(_data.SetItem(key, value));
        //public IImmutableContainer SetItem(string key, object value) => _data.ContainsKey(key) ? this : new ImmutableContainer(_data.SetItem(key, value));

        public IImmutableContainer RemoveItem(string key) => new ImmutableContainer(_data.Remove(key));

        public IEnumerator<(string Key, object Value)> GetEnumerator() => _data.Select(x => (x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();
    }
}