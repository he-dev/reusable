using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public class CompositeProvider : ResourceProvider //, IEnumerable<IResourceProvider>
    {
        private readonly IImmutableList<IResourceProvider> _providers;

        /// <summary>
        /// Resource provider cache.
        /// </summary>
        private readonly IDictionary<SoftString, IResourceProvider> _cache;

        /// <summary>
        /// Resource provider cache lock.
        /// </summary>
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        public CompositeProvider([NotNull] IEnumerable<IResourceProvider> providers)
            : base(ImmutableSession
                .Empty
                .SetScheme(ResourceSchemes.IOnymous)
                .SetItem(From<IProviderMeta>.Select(x => x.AllowRelativeUri), true))
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));

            _providers = providers.ToImmutableList();
            _cache = new Dictionary<SoftString, IResourceProvider>();
        }

        public static CompositeProvider Empty => new CompositeProvider(ImmutableList<IResourceProvider>.Empty);

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            return await DoAsync(uri, metadata, true, async resourceProvider => await resourceProvider.GetAsync(uri, metadata));
        }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            return await DoAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.PostAsync(uri, value, metadata)));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            return await DoAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.PutAsync(uri, value, metadata)));
        }

        protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            return await DoAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.DeleteAsync(uri, metadata)));
        }

        [ItemNotNull]
        private async Task<IResourceInfo> DoAsync(UriString uri, IImmutableSession metadata, bool isGet, Func<IResourceProvider, Task<IResourceInfo>> doAsync)
        {
            var cacheKey = uri.ToString();

            await _cacheLock.WaitAsync();
            try
            {
                // Used cached provider if already resolved.
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    return await doAsync(cached);
                }

                var resourceProviders = _providers.AsEnumerable();

                // When provider-name is specified then filter them by name first.
                if (metadata.GetNames().Any()) // this means there is a custom name
                {
                    resourceProviders = resourceProviders.Where(p => p.Metadata.GetNames().Overlaps(metadata.GetNames()));
                }

                // Check if there is a provider that matches the scheme of the absolute uri.
                if (uri.IsAbsolute)
                {
                    var ignoreScheme = uri.Scheme == ResourceSchemes.IOnymous;
                    resourceProviders = resourceProviders.Where(p => ignoreScheme || p.Metadata.GetSchemes().Contains(ResourceSchemes.IOnymous) || p.Metadata.GetSchemes().Contains(uri.Scheme));
                }

                // GET can search multiple providers.
                if (isGet)
                {
                    foreach (var resourceProvider in resourceProviders)
                    {
                        if (await doAsync(resourceProvider) is var resource && resource.Exists)
                        {
                            _cache[cacheKey] = resourceProvider;
                            return resource;
                        }
                    }

                    return new InMemoryResourceInfo(uri, metadata ?? ImmutableSession.Empty);
                }
                // Other methods are allowed to use only a single provider.
                else
                {
                    var match = _cache[cacheKey] = resourceProviders.Single();
                    return await doAsync(match);
                }
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        #region Collection initializers

        public CompositeProvider Add([NotNull] IResourceProvider resourceProvider)
        {
            return new CompositeProvider(_providers.Add(resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider))));
        }

        #endregion

        #region IEnumerable

        //public IEnumerator<IResourceProvider> GetEnumerator() => _resourceProviders.GetEnumerator();

        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_resourceProviders).GetEnumerator();

        #endregion
    }
}