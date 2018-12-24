using System.Collections.Generic;
using System.Collections.Immutable;

namespace Reusable.IOnymous
{
    using static ResourceMetadataKeys;

    public class ResourceMetadata
    {
        private readonly IImmutableDictionary<SoftString, object> _metadata;

        private ResourceMetadata(IImmutableDictionary<SoftString, object> metadata) => _metadata = metadata;

        public static ResourceMetadata Empty => new ResourceMetadata(ImmutableDictionary<SoftString, object>.Empty);

        public object this[SoftString key] => _metadata[key];
        public int Count => _metadata.Count;
        public IEnumerable<SoftString> Keys => _metadata.Keys;
        public IEnumerable<object> Values => _metadata.Values;
        public bool ContainsKey(SoftString key) => _metadata.ContainsKey(key);
        public bool Contains(KeyValuePair<SoftString, object> pair) => _metadata.Contains(pair);
        public bool TryGetKey(SoftString equalKey, out SoftString actualKey) => _metadata.TryGetKey(equalKey, out actualKey);
        public bool TryGetValue(SoftString key, out object value) => _metadata.TryGetValue(key, out value);

        public ResourceMetadata Add(SoftString key, object value) => new ResourceMetadata(_metadata.Add(key, value));
        public ResourceMetadata TryAdd(SoftString key, object value) => _metadata.ContainsKey(key) ? this : new ResourceMetadata(_metadata.Add(key, value));


        //public IImmutableDictionary<SoftString, object> Clear() => new ResourceProviderMetadata(_metadata.Clear());
        //public IImmutableDictionary<SoftString, object> AddRange(IEnumerable<KeyValuePair<SoftString, object>> pairs) => new ResourceProviderMetadata(_metadata.AddRange(pairs));
        public ResourceMetadata SetItem(SoftString key, object value) => new ResourceMetadata(_metadata.SetItem(key, value));
        //public ResourceMetadata SetItems(IEnumerable<KeyValuePair<SoftString, object>> items) => new ResourceProviderMetadata(_metadata.SetItems(items));
        //public IImmutableDictionary<SoftString, object> RemoveRange(IEnumerable<SoftString> keys) => new ResourceProviderMetadata(_metadata.RemoveRange(keys));
        //public IImmutableDictionary<SoftString, object> Remove(SoftString key) => new ResourceProviderMetadata(_metadata.Remove(key));

        //public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _metadata.GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_metadata).GetEnumerator();
    }
}