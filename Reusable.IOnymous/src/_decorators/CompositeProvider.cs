using System;
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
using Microsoft.Extensions.Caching.Memory;
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
        //private readonly ConcurrentDictionary<SoftString, IResourceProvider> _providerCache;
        private readonly IMemoryCache _cache;

        public CompositeProvider([NotNull] IEnumerable<IResourceProvider> providers)
            : base(ImmutableContainer
                .Empty
                .SetScheme(UriSchemes.Custom.IOnymous))
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));

            _providers = providers.ToImmutableList();
            _cache = new MemoryCache(new MemoryCacheOptions { });

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

            // Used cached provider if already resolved.
            if (_cache.TryGetValue<IResourceProvider>(providerKey, out var entry))
            {
                return await entry.InvokeAsync(request);
            }

            var filtered = Filters.Aggregate(_providers.AsEnumerable(), (providers, filter) => filter(providers, request));

            // GET can search multiple providers.
            if (request.Method == RequestMethod.Get)
            {
                foreach (var provider in filtered)
                {
                    if (await provider.InvokeAsync(request) is var resource && resource.Exists)
                    {
                        _cache.Set(providerKey, provider);
                        return resource;
                    }
                }

                return DoesNotExist(request);
            }
            // Other methods are allowed to use only a single provider.
            else
            {
                return await _cache.Set(providerKey, filtered.SingleOrThrow()).InvokeAsync(request);
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

        public override void Dispose()
        {
            _cache.Dispose();
            base.Dispose();
        }
    }
}