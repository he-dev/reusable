﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.IOnymous
{
    public class CompositeProvider : ResourceProvider//, IEnumerable<IResourceProvider>
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

        public CompositeProvider
        (
            [NotNull] IEnumerable<IResourceProvider> resourceProviders,
            Metadata metadata = default
        )
            : base(new[] { DefaultScheme }, metadata)
        {
            if (resourceProviders == null) throw new ArgumentNullException(nameof(resourceProviders));

            _cache = new Dictionary<UriString, IResourceProvider>();
            _resourceProviders = resourceProviders.ToImmutableList();
            var duplicateProviderNames =
                _resourceProviders
                    .Where(p => p.Metadata.CustomName())
                    .GroupBy(p => p.Metadata.CustomName())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.First())
                    .ToList();

            if (duplicateProviderNames.Any())
            {
                throw new ArgumentException($"Providers must use unique custom names but there are some duplicates: [{duplicateProviderNames.Select(p => (string)p.Metadata.CustomName()).Join(", ")}].");
            }
        }

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, Metadata metadata)
        {
            return await HandleMethodAsync(uri, metadata, true, async resourceProvider => await resourceProvider.GetAsync(uri, metadata));
        }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, Metadata metadata)
        {
            return await HandleMethodAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.PostAsync(uri, value, metadata)));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, Metadata metadata)
        {
            return await HandleMethodAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.PutAsync(uri, value, metadata)));
        }

        protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, Metadata metadata)
        {
            return await HandleMethodAsync(uri, metadata, false, async resourceProvider => (await resourceProvider.DeleteAsync(uri, metadata)));
        }

        [ItemNotNull]
        private async Task<IResourceInfo> HandleMethodAsync(UriString uri, Metadata metadata, bool isGet, Func<IResourceProvider, Task<IResourceInfo>> handleAsync)
        {
            await _cacheLock.WaitAsync();
            try
            {
                if (_cache.TryGetValue(uri, out var cached))
                {
                    return await handleAsync(cached);
                }

                var resourceProviders = _resourceProviders.ToList();

                //if (metadata.ProviderCustomName())
                if (uri.Query.TryGetValue(ResourceQueryStringKeys.ProviderName, out var providerName))
                {
                    var match =
                        resourceProviders
                            .Where(p => p.Metadata.CustomName().Equals(providerName))
                            // There must be exactly one provider with that name.                
                            .SingleOrThrow
                            (
                                onEmpty: () => DynamicException.Create
                                (
                                    "ProviderNotFound",
                                    $"Could not find any provider that would match the name '{(string)providerName}'."
                                ),
                                onMultiple: () => DynamicException.Create
                                (
                                    "MultipleProvidersFound",
                                    $"There is more than one provider that matches the custom name '{(string)providerName}'."
                                )
                            );

                    _cache[uri] = match;
                    return await handleAsync(match);
                }

                // Multiple providers can have the same default name.
                //if (metadata.ProviderDefaultName())
                if (uri.Query.TryGetValue(ResourceQueryStringKeys.ProviderType, out var providerType))
                {
                    resourceProviders =
                        resourceProviders
                            .Where(p => p.Metadata.DefaultName().Equals(providerType))
                            .ToList();

                    if (resourceProviders.Empty())
                    {
                        throw DynamicException.Create
                        (
                            "ProviderNotFound",
                            $"Could not find any provider that would match the name default name '{(string)providerType}'."
                        );
                    }
                }

                if (uri.IsAbsolute)
                {
                    var ignoreScheme = uri.Scheme == DefaultScheme;
                    resourceProviders =
                        resourceProviders
                            .Where(p => ignoreScheme || p.Schemes.Contains(DefaultScheme) || p.Schemes.Contains(uri.Scheme))
                            .ToList();

                    if (resourceProviders.Empty())
                    {
                        throw DynamicException.Create
                        (
                            "ProviderNotFound",
                            $"Could not find any provider that would match any of the schemes [{metadata.Schemes().Select(x => (string)x).Join(",")}]."
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
                            _cache[uri] = resourceProvider;
                            return resource;
                        }
                    }

                    return new InMemoryResourceInfo(uri, resource?.Metadata ?? Metadata.Empty);
                }
                // Other methods are allowed to use only a single provider.
                else
                {
                    var match = _cache[uri] = resourceProviders.Single();
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