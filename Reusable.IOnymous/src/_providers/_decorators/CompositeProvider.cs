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

namespace Reusable.IOnymous
{
    public class CompositeProvider : ResourceProvider //, IEnumerable<IResourceProvider>
    {
        /// <summary>
        /// Resource provider cache.
        /// </summary>
        private readonly Dictionary<SoftString, IResourceProvider> _cache;

        /// <summary>
        /// Resource provider cache lock.
        /// </summary>
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        private readonly IImmutableList<IResourceProvider> _resourceProviders;

        public CompositeProvider
        (
            [NotNull] IEnumerable<IResourceProvider> resourceProviders,
            IImmutableSession metadata = default
        )
            : base(new[] { DefaultScheme }, metadata ?? ImmutableSession.Empty)
        {
            if (resourceProviders == null) throw new ArgumentNullException(nameof(resourceProviders));

            _cache = new Dictionary<SoftString, IResourceProvider>();
            _resourceProviders = resourceProviders.ToImmutableList();
            var duplicateProviderNames =
                _resourceProviders
                    .Where(p => p.CustomName)
                    .GroupBy(p => p.CustomName)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.First())
                    .ToList();

            if (duplicateProviderNames.Any())
            {
                throw new ArgumentException
                (
                    $"Providers must use unique custom names but there are some duplicates: " +
                    $"[{duplicateProviderNames.Select(p => (string)p.CustomName).Join(", ")}]."
                );
            }
        }

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            return await HandleMethodAsync(uri, metadata, true, async resourceProvider => await resourceProvider.GetAsync(uri, metadata));
        }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            return await HandleMethodAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.PostAsync(uri, value, metadata)));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            return await HandleMethodAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.PutAsync(uri, value, metadata)));
        }

        protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            return await HandleMethodAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.DeleteAsync(uri, metadata)));
        }

        [ItemNotNull]
        private async Task<IResourceInfo> HandleMethodAsync(UriString uri, IImmutableSession metadata, bool isGet, Func<IResourceProvider, Task<IResourceInfo>> handleAsync)
        {
            var cacheKey = uri.Path.Decoded;

            await _cacheLock.WaitAsync();
            try
            {
                // Used cached provider if already resolved.
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    return await handleAsync(cached);
                }

                var resourceProviders = _resourceProviders.ToList();

                // Use custom-provider-name if available which has the highest priority.
                if (metadata.Get(Use<IProviderSession>.Namespace, x => x.CustomName) is var customName && customName)
                {
                    var match =
                        resourceProviders
                            .Where(p => p.CustomName?.Equals(customName) == true)
                            // There must be exactly one provider with that name.                
                            .SingleOrThrow
                            (
                                onEmpty: () => DynamicException.Create
                                (
                                    "ProviderNotFound",
                                    $"Could not find any provider that would match the name '{(string)customName}'."
                                ),
                                onMultiple: () => DynamicException.Create
                                (
                                    "MultipleProvidersFound",
                                    $"There is more than one provider that matches the custom name '{(string)customName}'."
                                )
                            );

                    _cache[cacheKey] = match;
                    return await handleAsync(match);
                }

                // Multiple providers can have the same default name.
                if (metadata.Get(Use<IProviderSession>.Namespace, x => x.DefaultName) is var defaultName && defaultName)
                {
                    resourceProviders =
                        resourceProviders
                            .Where(p => p.DefaultName.Equals(defaultName))
                            .ToList();

                    if (resourceProviders.Empty())
                    {
                        throw DynamicException.Create
                        (
                            "ProviderNotFound",
                            $"Could not find any provider that would match the name default name '{(string)defaultName}'."
                        );
                    }
                }

                // Check if there is a provider that matches the scheme of the absolute uri.
                if (uri.IsAbsolute)
                {
                    var ignoreScheme = uri.Scheme == DefaultScheme;
                    resourceProviders =
                        ignoreScheme
                            ? resourceProviders
                            : resourceProviders.Where(p => p.Schemes.Contains(DefaultScheme) || p.Schemes.Contains(uri.Scheme)).ToList();

                    if (resourceProviders.Empty())
                    {
                        throw DynamicException.Create
                        (
                            $"ProviderNotFound",
                            $"Could not find any provider that would match any of the schemes [{metadata.Get(Use<IAnySession>.Namespace, x => x.Schemes).Select(x => (string)x).Join(",")}]."
                        );
                    }
                }

                // GET can search multiple providers.
                if (isGet)
                {
                    var resource = default(IResourceInfo);
                    foreach (var resourceProvider in resourceProviders)
                    {
                        resource = await handleAsync(resourceProvider);
                        if (resource.Exists)
                        {
                            _cache[cacheKey] = resourceProvider;
                            return resource;
                        }
                    }

                    return new InMemoryResourceInfo(uri, resource?.Metadata ?? ImmutableSession.Empty);
                }
                // Other methods are allowed to use only a single provider.
                else
                {
                    var match = _cache[uri.Path.Decoded] = resourceProviders.Single();
                    return await handleAsync(match);
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
            var resourceProviders = _resourceProviders.Add(resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider)));
            return new CompositeProvider
            (
                resourceProviders,
                Metadata
            );
        }

        #endregion

        #region IEnumerable

        //public IEnumerator<IResourceProvider> GetEnumerator() => _resourceProviders.GetEnumerator();

        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_resourceProviders).GetEnumerator();

        #endregion
    }
}