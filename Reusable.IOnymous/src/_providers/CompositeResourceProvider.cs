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
        private readonly Dictionary<UriString, IResourceProvider> _resourceProviderCache;

        private readonly SemaphoreSlim _resourceProviderCacheLock = new SemaphoreSlim(1, 1);

        private readonly IImmutableList<IResourceProvider> _resourceProviders;

        public CompositeResourceProvider
        (
            [NotNull] IList<IResourceProvider> resourceProviders,
            [CanBeNull] ResourceMetadata metadata = null
        )
            : base(
                (metadata ?? ResourceMetadata.Empty)
                    .Add(ResourceMetadataKeys.CanGet, resourceProviders.Any(x => x.Metadata.ContainsKey(ResourceMetadataKeys.CanGet)))
                    .Add(ResourceMetadataKeys.CanPut, resourceProviders.Any(x => x.Metadata.ContainsKey(ResourceMetadataKeys.CanPut)))
                    .Add(ResourceMetadataKeys.CanDelete, resourceProviders.Any(x => x.Metadata.ContainsKey(ResourceMetadataKeys.CanDelete)))
                    .Add(ResourceMetadataKeys.Scheme, DefaultScheme)
            )
        {
            if (resourceProviders == null) throw new ArgumentNullException(nameof(resourceProviders));

            _resourceProviderCache = new Dictionary<UriString, IResourceProvider>();
            _resourceProviders = resourceProviders.ToImmutableList();
        }

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            await _resourceProviderCacheLock.WaitAsync();
            try
            {
                // Use either the cached resource-provider or find a new one.

                if (_resourceProviderCache.TryGetValue(uri, out var cachedResourceProvider))
                {
                    return await cachedResourceProvider.GetAsync(uri, metadata);
                }
                else
                {
                    var resourceProviders = _resourceProviders.AsEnumerable();

                    // If provider-name is specified then search only providers that mach it.
                    var providerCustomName = (ImplicitString)metadata.ProviderCustomName();
                    var providerDefaultName = (ImplicitString)metadata.ProviderDefaultName();
                    if (providerCustomName || providerDefaultName)
                    {
                        resourceProviders =
                            _resourceProviders
                                .Where(p =>
                                    (providerCustomName && SoftString.Comparer.Equals(p.Metadata.ProviderCustomName(), (string)providerCustomName)) ||
                                    (providerDefaultName && SoftString.Comparer.Equals(p.Metadata.ProviderDefaultName(), (string)providerDefaultName))
                                );
                    }

                    foreach (var resourceProvider in resourceProviders)
                    {
                        var resource = await resourceProvider.GetAsync(uri, metadata);
                        if (resource.Exists)
                        {
                            _resourceProviderCache[uri] = resourceProvider;
                            return resource;
                        }
                    }
                }
            }
            finally
            {
                _resourceProviderCacheLock.Release();
            }

            // Apparently we didn't find the resource.
            return new InMemoryResourceInfo(uri);
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream data, ResourceMetadata metadata = null)
        {
            var resourceProvider = await GetResourceProviderAsync(uri, metadata);

            if (!resourceProvider.Metadata.CanPut())
            {
                throw DynamicException.Create("SerializeNotSupported", $"Value-provider '{resourceProvider.GetType().ToPrettyString()}' doesn't support '{nameof(PutAsync)}'.");
            }

            return await resourceProvider.PutAsync(uri, data, metadata);
        }

        protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            var resourceProvider = await GetResourceProviderAsync(uri, metadata);            
            return await resourceProvider.DeleteAsync(uri, metadata);
        }

        [ItemNotNull]
        private async Task<IResourceProvider> GetResourceProviderAsync(UriString uri, ResourceMetadata metadata = null)
        {
            await _resourceProviderCacheLock.WaitAsync();
            try
            {
                if (metadata.TryGetValue(ResourceMetadataKeys.ProviderCustomName, out string providerNameToFind))
                {
                    return
                        _resourceProviders
                            .SingleOrDefault(p =>
                                p.Metadata.TryGetValue(ResourceMetadataKeys.ProviderCustomName, out string providerName)
                                && providerName == providerNameToFind
                            );
                }

                if (_resourceProviderCache.TryGetValue(uri, out var cachedValueProvider))
                {
                    return cachedValueProvider;
                }

                throw DynamicException.Create
                (
                    $"Unknown{nameof(ResourceProvider)}",
                    $"Could not serialize '{uri}' because serializing requires a well-known resource-provider and it could not be determined. " +
                    $"This means that you either need to specify its name via '{nameof(metadata)}' or call '{nameof(GetAsync)}' first."
                );
            }
            finally
            {
                _resourceProviderCacheLock.Release();
            }
        }

        public IEnumerator<IResourceProvider> GetEnumerator() => _resourceProviders.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_resourceProviders).GetEnumerator();
    }
}