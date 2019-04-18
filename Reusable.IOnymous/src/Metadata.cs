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
    public readonly struct Metadata
    {
        [CanBeNull]
        private readonly IImmutableDictionary<SoftString, object> _metadata;

        public Metadata(IImmutableDictionary<SoftString, object> metadata) => _metadata = metadata;

        public static Metadata Empty => new Metadata(ImmutableDictionary<SoftString, object>.Empty);

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayMember(x => x.Count);
            builder.DisplayCollection(x => x.Keys);
        });

        private IImmutableDictionary<SoftString, object> Value => _metadata ?? ImmutableDictionary<SoftString, object>.Empty;

        public object this[SoftString key] => Value[key];

        public int Count => Value.Count;

        public IEnumerable<SoftString> Keys => Value.Keys;

        public IEnumerable<object> Values => Value.Values;

        public bool ContainsKey(SoftString key) => Value.ContainsKey(key);

        public bool Contains(KeyValuePair<SoftString, object> pair) => Value.Contains(pair);

        public bool TryGetKey(SoftString equalKey, out SoftString actualKey) => Value.TryGetKey(equalKey, out actualKey);

        public bool TryGetValue(SoftString key, out object value) => Value.TryGetValue(key, out value);

        public Metadata Add(SoftString key, object value) => new Metadata(Value.Add(key, value));

        public Metadata TryAdd(SoftString key, object value) => Value.ContainsKey(key) ? this : new Metadata(Value.Add(key, value));

        //public IImmutableDictionary<SoftString, object> Clear() => new ResourceProviderMetadata(_metadata.Clear());
        //public IImmutableDictionary<SoftString, object> AddRange(IEnumerable<KeyValuePair<SoftString, object>> pairs) => new ResourceProviderMetadata(_metadata.AddRange(pairs));
        public Metadata SetItem(SoftString key, object value) => new Metadata(Value.SetItem(key, value));
        //public ResourceMetadata SetItems(IEnumerable<KeyValuePair<SoftString, object>> items) => new ResourceProviderMetadata(_metadata.SetItems(items));
        //public IImmutableDictionary<SoftString, object> RemoveRange(IEnumerable<SoftString> keys) => new ResourceProviderMetadata(_metadata.RemoveRange(keys));
        //public IImmutableDictionary<SoftString, object> Remove(SoftString key) => new ResourceProviderMetadata(_metadata.Remove(key));

        //public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _metadata.GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_metadata).GetEnumerator();
    }
}