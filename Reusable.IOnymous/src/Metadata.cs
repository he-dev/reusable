using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Diagnostics;

namespace Reusable.IOnymous
{
    // With a 'struct' we don't need any null-checks.
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [PublicAPI]
    public readonly struct Metadata : IEnumerable<KeyValuePair<SoftString, object>>
    {
        [CanBeNull]
        private readonly IImmutableDictionary<SoftString, object> _data;

        public Metadata([NotNull] IImmutableDictionary<SoftString, object> metadata) => _data = metadata ?? throw new ArgumentNullException(nameof(metadata));

        public static Metadata Empty => new Metadata(ImmutableDictionary<SoftString, object>.Empty);

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x.Count);
            builder.DisplayValues(x => x.Keys);
        });
        
        // A struct cannot be initialized so the field remains null when using 'default'.
        private IImmutableDictionary<SoftString, object> Data => _data ?? ImmutableDictionary<SoftString, object>.Empty;

        public object this[SoftString key] => Data[key];

        public int Count => Data.Count;

        public IEnumerable<SoftString> Keys => Data.Keys;

        public IEnumerable<object> Values => Data.Values;

        public bool ContainsKey(SoftString key) => Data.ContainsKey(key);

        public bool Contains(KeyValuePair<SoftString, object> pair) => Data.Contains(pair);

        public bool TryGetKey(SoftString equalKey, out SoftString actualKey) => Data.TryGetKey(equalKey, out actualKey);

        public bool TryGetValue(SoftString key, out object value) => Data.TryGetValue(key, out value);

        public Metadata Add(SoftString key, object value) => new Metadata(Data.Add(key, value));

        public Metadata TryAdd(SoftString key, object value) => Data.ContainsKey(key) ? this : new Metadata(Data.Add(key, value));

        //public IImmutableDictionary<SoftString, object> Clear() => new ResourceProviderMetadata(_metadata.Clear());
        //public IImmutableDictionary<SoftString, object> AddRange(IEnumerable<KeyValuePair<SoftString, object>> pairs) => new ResourceProviderMetadata(_metadata.AddRange(pairs));
        public Metadata SetItem(SoftString key, object value) => new Metadata(Data.SetItem(key, value));
        //public ResourceMetadata SetItems(IEnumerable<KeyValuePair<SoftString, object>> items) => new ResourceProviderMetadata(_metadata.SetItems(items));
        //public IImmutableDictionary<SoftString, object> RemoveRange(IEnumerable<SoftString> keys) => new ResourceProviderMetadata(_metadata.RemoveRange(keys));
        //public IImmutableDictionary<SoftString, object> Remove(SoftString key) => new ResourceProviderMetadata(_metadata.Remove(key));

        //public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _metadata.GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_metadata).GetEnumerator();


        public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Data).GetEnumerator();
    }
}