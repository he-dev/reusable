using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public class CompositeResourceProvider : ResourceProvider, IEnumerable<IResourceProvider>
    {
        private readonly Dictionary<SimpleUri, IResourceProvider> _valueProviderCache;

        private readonly SemaphoreSlim _valueProviderCacheLock = new SemaphoreSlim(1, 1);

        private readonly IImmutableList<IResourceProvider> _valueProviders;

        public CompositeResourceProvider
        (
            IImmutableList<IResourceProvider> valueProviders,
            ResourceProviderMetadata metadata
        )
            : base(
                metadata
                    .Add(ValueProviderMetadataKeyNames.CanGet, valueProviders.Any(x => x.Metadata.ContainsKey(ValueProviderMetadataKeyNames.CanGet)))
                    .Add(ValueProviderMetadataKeyNames.CanPut, valueProviders.Any(x => x.Metadata.ContainsKey(ValueProviderMetadataKeyNames.CanPut)))
                    .Add(ValueProviderMetadataKeyNames.CanDelete, valueProviders.Any(x => x.Metadata.ContainsKey(ValueProviderMetadataKeyNames.CanDelete)))
            )
        {
            _valueProviderCache = new Dictionary<SimpleUri, IResourceProvider>();
            _valueProviders = valueProviders;
        }

        public override async Task<IResourceInfo> GetAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            await _valueProviderCacheLock.WaitAsync();
            try
            {
                // In provider-name specified then try to get the value from this provider without using caching.
                if (metadata.TryGetValue(ValueProviderMetadataKeyNames.ProviderName, out string providerNameToFind))
                {
                    var valueProvider =
                        _valueProviders
                            .SingleOrDefault(p =>
                                p.Metadata.TryGetValue(ValueProviderMetadataKeyNames.ProviderName, out string providerName)
                                && providerName == providerNameToFind
                            );

                    if (!(valueProvider is null))
                    {
                        return await valueProvider.GetAsync(uri, metadata);
                    }
                }
                else
                {
                    // Use either the cached value-provider of find a new one.

                    if (_valueProviderCache.TryGetValue(uri, out var cachedValueProvider))
                    {
                        return await cachedValueProvider.GetAsync(uri, metadata);
                    }
                    else
                    {
                        foreach (var valueProvider in _valueProviders)
                        {
                            var value = await valueProvider.GetAsync(uri, metadata);
                            if (value.Exists)
                            {
                                _valueProviderCache[uri] = valueProvider;
                                return value;
                            }
                        }
                    }
                }
            }
            finally
            {
                _valueProviderCacheLock.Release();
            }

            return new InMemoryResourceInfo(uri);
        }

        public override async Task<IResourceInfo> PutAsync(SimpleUri uri, Stream data, ResourceProviderMetadata metadata = null)
        {
            var valueProvider = await GetValueProviderAsync(uri, metadata);

            if (!valueProvider.Metadata.TryGetValue(ValueProviderMetadataKeyNames.CanPut, out bool _))
            {
                throw DynamicException.Create("SerializeNotSupported", $"Value-provider '{valueProvider.GetType().ToPrettyString()}' doesn't support '{nameof(PutAsync)}'.");
            }

            return await valueProvider.PutAsync(uri, data, metadata);

        }

        public override async Task<IResourceInfo> PutAsync(SimpleUri uri, object value, ResourceProviderMetadata metadata = null)
        {
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, value);
                return await PutAsync(uri, memoryStream, metadata);
            }
        }

        public override async Task<IResourceInfo> DeleteAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            var valueProvider = await GetValueProviderAsync(uri, metadata);

            if (!valueProvider.Metadata.TryGetValue(ValueProviderMetadataKeyNames.CanDelete, out bool _))
            {
                throw DynamicException.Create("DeleteNotSupported", $"Value-provider '{valueProvider.GetType().ToPrettyString()}' doesn't support '{nameof(DeleteAsync)}'.");
            }

            return await valueProvider.DeleteAsync(uri, metadata);
        }

        [ItemNotNull]
        private async Task<IResourceProvider> GetValueProviderAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            await _valueProviderCacheLock.WaitAsync();
            try
            {
                if (metadata.TryGetValue(ValueProviderMetadataKeyNames.ProviderName, out string providerNameToFind))
                {
                    return
                        _valueProviders
                            .SingleOrDefault(p =>
                                p.Metadata.TryGetValue(ValueProviderMetadataKeyNames.ProviderName, out string providerName)
                                && providerName == providerNameToFind
                            );
                }

                if (_valueProviderCache.TryGetValue(uri, out var cachedValueProvider))
                {
                    return cachedValueProvider;
                }

                throw DynamicException.Create
                (
                    "UnknownValueProvider",
                    $"Could not serialize '{uri}' because serializing requires a well-known-value-provider and it could be determined. " +
                    $"This means that it needs to be either specified via '{nameof(metadata)}' or be already determined by calling '{nameof(GetAsync)}'."
                );
            }
            finally
            {
                _valueProviderCacheLock.Release();
            }
        }

        public IEnumerator<IResourceProvider> GetEnumerator() => _valueProviders.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_valueProviders).GetEnumerator();
    }
}