using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionizer;

namespace Reusable.IOnymous
{
    public class CompositeResourceProvider : ResourceProvider, IEnumerable<IResourceProvider>
    {
        /// <summary>
        /// Resource provider cache.
        /// </summary>
        private readonly Dictionary<UriString, IResourceProvider> _cache;

        /// <summary>
        /// Resource provider cache lock.
        /// </summary>
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        private readonly IImmutableList<IResourceProvider> _resourceProviders;

        public CompositeResourceProvider
        (
            [NotNull] IEnumerable<IResourceProvider> resourceProviders,
            [CanBeNull] ResourceMetadata metadata = null
        )
            : base(new [] { DefaultScheme }, (metadata ?? ResourceMetadata.Empty))
        {
            if (resourceProviders == null) throw new ArgumentNullException(nameof(resourceProviders));

            _cache = new Dictionary<UriString, IResourceProvider>();
            _resourceProviders = resourceProviders.ToImmutableList();
        }

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return await HandleMethodAsync(uri, metadata, true, async resourceProvider =>
            {
                var resource = await resourceProvider.GetAsync(uri, metadata);
                return (resource, resource.Exists);
            });
        }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            return await HandleMethodAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.PostAsync(uri, value, metadata), true));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            return await HandleMethodAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.PutAsync(uri, value, metadata), true));
        }

        protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            return await HandleMethodAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.DeleteAsync(uri, metadata), true));
        }

        [ItemNotNull]
        private async Task<IResourceInfo> HandleMethodAsync(UriString uri, ResourceMetadata metadata, bool allowMultipleProviders, Func<IResourceProvider, Task<(IResourceInfo Resource, bool Handled)>> handleAsync)
        {
            await _cacheLock.WaitAsync();
            try
            {
                if (_cache.TryGetValue(uri, out var cached))
                {
                    var (resource, _) = await handleAsync(cached);
                    return resource;
                }

                var resourceProviders = _resourceProviders.AsEnumerable();

                // There can be only one provider with that name.                
                if (metadata.ProviderCustomName())
                {
                    var match = resourceProviders.SingleOrDefault(p => metadata.ProviderCustomName().Equals(p.Metadata.ProviderCustomName()));
                    if (match is null)
                    {
                        throw DynamicException.Create
                        (
                            "ProviderNotFound",
                            $"Could not find any provider that would match the name '{metadata.ProviderCustomName().ToString()}'."
                        );
                    }

                    var (resource, handled) = await handleAsync(match);
                    if (handled)
                    {
                        _cache[uri] = match;
                    }

                    return resource;
                }

                // Multiple providers can have the same default name.
                if (metadata.ProviderDefaultName())
                {
                    resourceProviders = resourceProviders.Where(p => p.Metadata.ProviderDefaultName().Equals(metadata.ProviderDefaultName()));
                    if (resourceProviders.Empty())
                    {
                        throw DynamicException.Create
                        (
                            "ProviderNotFound",
                            $"Could not find any provider that would match the name '{metadata.ProviderDefaultName().ToString()}'."
                        );
                    }
                }

                if (uri.IsAbsolute)
                {
                    var ignoreScheme = uri.Scheme == DefaultScheme;
                    resourceProviders = resourceProviders.Where(p => ignoreScheme || p.Schemes.Contains(uri.Scheme));
                    if (resourceProviders.Empty())
                    {
                        throw DynamicException.Create
                        (
                            "ProviderNotFound",
                            $"Could not find any provider that would match any of the schemes [{(metadata.Schemes().Select(x => x.ToString()).Join(","))}]."
                        );
                    }
                }

                if (allowMultipleProviders)
                {
                    foreach (var resourceProvider in resourceProviders)
                    {
                        var (resource, handled) = await handleAsync(resourceProvider);
                        if (handled)
                        {
                            _cache[uri] = resourceProvider;
                            return resource;
                        }
                    }

                    return new InMemoryResourceInfo(uri);
                }
                else
                {
                    var match = resourceProviders.Single();
                    var (resource, handled) = await handleAsync(match);
                    if (handled)
                    {
                        _cache[uri] = match;
                    }

                    return resource;
                }
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        #region IEnumerable

        public IEnumerator<IResourceProvider> GetEnumerator() => _resourceProviders.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_resourceProviders).GetEnumerator();

        #endregion
    }
}