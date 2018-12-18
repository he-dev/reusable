using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    using static ResourceMetadataKeys;

    [PublicAPI]
    public interface IResourceProvider
    {
        [NotNull]
        ResourceMetadata Metadata { get; }

        [ItemNotNull]
        Task<IResourceInfo> GetAsync([NotNull] UriString uri, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> PostAsync([NotNull] UriString uri, [NotNull] Stream value, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> PutAsync([NotNull] UriString uri, [NotNull] Stream value, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> DeleteAsync([NotNull] UriString uri, ResourceMetadata metadata = null);
    }

    public abstract class ResourceProvider : IResourceProvider
    {
        public static readonly string Scheme = "ionymous";

        protected ResourceProvider(ResourceMetadata metadata)
        {
            // If this is a decorator then the decorated resource-provider already has set this.
            if (!metadata.ContainsKey(ProviderDefaultName))
            {
                metadata = metadata.Add(ProviderDefaultName, GetType().ToPrettyString());
            }

            Metadata = metadata;
        }

        public virtual ResourceMetadata Metadata { get; }

        public abstract Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null);

        public virtual Task<IResourceInfo> PostAsync(UriString name, Stream value, ResourceMetadata metadata = null) { throw new NotImplementedException(); }

        public abstract Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null);

        public abstract Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null);

        protected static Exception CreateException(IResourceProvider provider, string name, ResourceMetadata metadata, Exception inner, [CallerMemberName] string memberName = null)
        {
            return new Exception();
        }
    }

    public static class ResourceProviderExtensions
    {
        public static async Task<IResourceInfo> PutAsync(this IResourceProvider resourceProvider, UriString uri, object value, ResourceMetadata metadata = null)
        {
            switch (value)
            {
                case string s:
                    using (var streamReader = s.ToStreamReader())
                    {
                        return await resourceProvider.PutAsync
                        (
                            uri, 
                            streamReader.BaseStream, 
                            (metadata ?? ResourceMetadata.Empty).Add(Serializer, nameof(StreamReader))
                        );
                    }
                default:
                    var binaryFormatter = new BinaryFormatter();
                    using (var memoryStream = new MemoryStream())
                    {
                        binaryFormatter.Serialize(memoryStream, value);
                        return await resourceProvider.PutAsync
                        (
                            uri, 
                            memoryStream, 
                            (metadata ?? ResourceMetadata.Empty).Add(Serializer, nameof(BinaryFormatter))
                        );
                    }
            }
        }
    }

    public class ResourceMetadata
    {
        private readonly IImmutableDictionary<SoftString, object> _metadata;

        public ResourceMetadata() : this(ImmutableDictionary<SoftString, object>.Empty) { }

        private ResourceMetadata(IImmutableDictionary<SoftString, object> metadata) => _metadata = metadata;

        public static ResourceMetadata Empty => new ResourceMetadata();

        public object this[SoftString key] => _metadata[key];
        public int Count => _metadata.Count;
        public IEnumerable<SoftString> Keys => _metadata.Keys;
        public IEnumerable<object> Values => _metadata.Values;
        public bool ContainsKey(SoftString key) => _metadata.ContainsKey(key);
        public bool Contains(KeyValuePair<SoftString, object> pair) => _metadata.Contains(pair);
        public bool TryGetKey(SoftString equalKey, out SoftString actualKey) => _metadata.TryGetKey(equalKey, out actualKey);
        public bool TryGetValue(SoftString key, out object value) => _metadata.TryGetValue(key, out value);

        public ResourceMetadata Add(SoftString key, object value) => new ResourceMetadata(_metadata.Add(key, value));
        //public IImmutableDictionary<SoftString, object> Clear() => new ResourceProviderMetadata(_metadata.Clear());
        //public IImmutableDictionary<SoftString, object> AddRange(IEnumerable<KeyValuePair<SoftString, object>> pairs) => new ResourceProviderMetadata(_metadata.AddRange(pairs));
        //public IImmutableDictionary<SoftString, object> SetItem(SoftString key, object value) => new ResourceProviderMetadata(_metadata.SetItem(key, value));
        //public IImmutableDictionary<SoftString, object> SetItems(IEnumerable<KeyValuePair<SoftString, object>> items) => new ResourceProviderMetadata(_metadata.SetItems(items));
        //public IImmutableDictionary<SoftString, object> RemoveRange(IEnumerable<SoftString> keys) => new ResourceProviderMetadata(_metadata.RemoveRange(keys));
        //public IImmutableDictionary<SoftString, object> Remove(SoftString key) => new ResourceProviderMetadata(_metadata.Remove(key));

        //public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _metadata.GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_metadata).GetEnumerator();
    }

    public static class ResourceProviderMetadataExtensions
    {
        public static bool TryGetValue<T>(this ResourceMetadata metadata, SoftString key, out T value)
        {
            if (!(metadata is null) && metadata.TryGetValue(key, out var x) && x is T result)
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

        public static string ProviderDefaultName(this ResourceMetadata metadata)
        {
            return
                metadata
                    .TryGetValue(nameof(ProviderDefaultName), out string name)
                        ? name
                        : string.Empty;
        }

        public static string ProviderCustomName(this ResourceMetadata metadata)
        {
            return
                metadata
                    .TryGetValue(nameof(ProviderCustomName), out string name)
                        ? name
                        : string.Empty;
        }
    }

    public static class ResourceMetadataKeys
    {
        public static string ProviderDefaultName { get; } = nameof(ProviderDefaultName);
        public static string ProviderCustomName { get; } = nameof(ProviderCustomName);
        public static string CanGet { get; } = nameof(CanGet);
        public static string CanPost { get; } = nameof(CanPost);
        public static string CanPut { get; } = nameof(CanPut);
        public static string CanDelete { get; } = nameof(CanDelete);
        public static string Serializer { get; } = nameof(Serializer);
    }
}
