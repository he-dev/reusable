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

        private readonly IImmutableList<IResourceProvider> _resourceProviders;

        public CompositeResourceProvider
        (
            [NotNull] IList<IResourceProvider> resourceProviders,
            [NotNull] ResourceProviderMetadata metadata
        )
            : base(
                metadata
                    .Add(ResourceProviderMetadataKeyNames.CanGet, resourceProviders.Any(x => x.Metadata.ContainsKey(ResourceProviderMetadataKeyNames.CanGet)))
                    .Add(ResourceProviderMetadataKeyNames.CanPut, resourceProviders.Any(x => x.Metadata.ContainsKey(ResourceProviderMetadataKeyNames.CanPut)))
                    .Add(ResourceProviderMetadataKeyNames.CanDelete, resourceProviders.Any(x => x.Metadata.ContainsKey(ResourceProviderMetadataKeyNames.CanDelete)))
            )
        {
            if (resourceProviders == null) throw new ArgumentNullException(nameof(resourceProviders));
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            _valueProviderCache = new Dictionary<SimpleUri, IResourceProvider>();
            _resourceProviders = resourceProviders.ToImmutableList();
        }

        public override async Task<IResourceInfo> GetAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            await _valueProviderCacheLock.WaitAsync();
            try
            {
                // Use either the cached value-provider of find a new one.

                if (_valueProviderCache.TryGetValue(uri, out var cachedValueProvider))
                {
                    return await cachedValueProvider.GetAsync(uri, metadata);
                }
                else
                {
                    var resouceProviders = _resourceProviders.AsEnumerable();

                    // In provider-name specified then try to get the value from this provider without using caching.
                    var providerCustomName = (ImplicitString)metadata.ProviderCustomName();
                    var providerDefaultName = (ImplicitString)metadata.ProviderDefaultName();
                    if (providerCustomName || providerDefaultName) 
                    {
                        resouceProviders =
                            _resourceProviders
                                .Where(p =>
                                    (providerCustomName && SoftString.Comparer.Equals(p.Metadata.ProviderCustomName(), (string)providerCustomName)) ||
                                    (providerDefaultName && SoftString.Comparer.Equals(p.Metadata.ProviderDefaultName(), (string)providerDefaultName))
                                );                      
                    }

                    foreach (var valueProvider in resouceProviders)
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
            finally
            {
                _valueProviderCacheLock.Release();
            }

            return new InMemoryResourceInfo(uri);
        }

        public override async Task<IResourceInfo> PutAsync(SimpleUri uri, Stream data, ResourceProviderMetadata metadata = null)
        {
            var valueProvider = await GetValueProviderAsync(uri, metadata);

            if (!valueProvider.Metadata.TryGetValue(ResourceProviderMetadataKeyNames.CanPut, out bool _))
            {
                throw DynamicException.Create("SerializeNotSupported", $"Value-provider '{valueProvider.GetType().ToPrettyString()}' doesn't support '{nameof(PutAsync)}'.");
            }

            return await valueProvider.PutAsync(uri, data, metadata);

        }

        public override async Task<IResourceInfo> DeleteAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            var valueProvider = await GetValueProviderAsync(uri, metadata);

            if (!valueProvider.Metadata.TryGetValue(ResourceProviderMetadataKeyNames.CanDelete, out bool _))
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
                if (metadata.TryGetValue(ResourceProviderMetadataKeyNames.ProviderCustomName, out string providerNameToFind))
                {
                    return
                        _resourceProviders
                            .SingleOrDefault(p =>
                                p.Metadata.TryGetValue(ResourceProviderMetadataKeyNames.ProviderCustomName, out string providerName)
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

        public IEnumerator<IResourceProvider> GetEnumerator() => _resourceProviders.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_resourceProviders).GetEnumerator();
    }
}