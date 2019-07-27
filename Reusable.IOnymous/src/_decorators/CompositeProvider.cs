﻿using System;
using System.Collections;
using System.Collections.Concurrent;
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
    public interface ICompositeProvider : IResourceProvider, IEnumerable<IResourceProvider> { }

    public class CompositeProvider : ResourceProvider, ICompositeProvider
    {
        private readonly IImmutableList<IResourceProvider> _providers;

        /// <summary>
        /// Resource provider cache.
        /// </summary>
        private readonly ConcurrentDictionary<SoftString, IResourceProvider> _providerCache;

        /// <summary>
        /// Resource provider cache lock.
        /// </summary>
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        public CompositeProvider([NotNull] IEnumerable<IResourceProvider> providers)
            : base(ImmutableContainer
                .Empty
                .SetScheme(UriSchemes.Custom.IOnymous)
                .SetItem(ResourceProviderProperty.AllowRelativeUri, true))
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));

            _providers = providers.ToImmutableList();
            _providerCache = new ConcurrentDictionary<SoftString, IResourceProvider>();

            Methods =
                _providers
                    .SelectMany(x => x.Methods.Select(y => y.Method))
                    .Distinct()
                    .Aggregate(MethodCollection.Empty, (current, next) => current.Add(next, InvokeAsync));
        }

        public static CompositeProvider Empty => new CompositeProvider(ImmutableList<IResourceProvider>.Empty);

        public IEnumerable<ResourceProviderFilterCallback> Filters { get; set; } = new ResourceProviderFilterCallback[]
        {
            ResourceProviderFilters.FilterByName,
            ResourceProviderFilters.FilterByScheme
        };

        public override async Task<IResource> InvokeAsync(Request request)
        {
            var providerKey = request.Uri.ToString();

            await _cacheLock.WaitAsync();
            try
            {
                // Used cached provider if already resolved.
                if (_providerCache.TryGetValue(providerKey, out var cached))
                {
                    return await cached.InvokeAsync(request);
                }

                var filtered = Filters.Aggregate(_providers.AsEnumerable(), (providers, filter) => filter(providers, request));

                // GET can search multiple providers.
                if (request.Method == RequestMethod.Get)
                {
                    foreach (var provider in filtered)
                    {
                        if (await provider.InvokeAsync(request) is var resource && resource.Exists)
                        {
                            _providerCache[providerKey] = provider;
                            return resource;
                        }
                    }

                    return DoesNotExist(request);
                }
                // Other methods are allowed to use only a single provider.
                else
                {
                    var match = _providerCache[providerKey] = filtered.SingleOrThrow();
                    return await match.InvokeAsync(request);
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

        public IEnumerator<IResourceProvider> GetEnumerator() => _providers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_providers).GetEnumerator();

        #endregion
    }

    
}