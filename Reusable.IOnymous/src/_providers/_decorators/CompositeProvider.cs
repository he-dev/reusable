using System;
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
    public class CompositeProvider : ResourceProvider //, IEnumerable<IResourceProvider>
    {
        private readonly IImmutableList<IResourceProvider> _providers;

        /// <summary>
        /// Resource provider cache.
        /// </summary>
        private readonly ConcurrentDictionary<SoftString, IResourceProvider> _cache;

        /// <summary>
        /// Resource provider cache lock.
        /// </summary>
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        public CompositeProvider([NotNull] IEnumerable<IResourceProvider> providers)
            : base(ImmutableContainer
                .Empty
                .SetScheme(ResourceSchemes.IOnymous)
                .SetItem(From<IProviderProperties>.Select(x => x.AllowRelativeUri), true))
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));

            _providers = providers.ToImmutableList();
            _cache = new ConcurrentDictionary<SoftString, IResourceProvider>();

            Methods =
                _providers
                    .SelectMany(x => x.Methods.Keys)
                    .Distinct()
                    .Aggregate(MethodDictionary.Empty, (current, next) => current.Add(next, InvokeAsync));
        }

        public static CompositeProvider Empty => new CompositeProvider(ImmutableList<IResourceProvider>.Empty);

        public override async Task<IResource> InvokeAsync(Request request)
        {
            var cacheKey = request.Uri.ToString();

            await _cacheLock.WaitAsync();
            try
            {
                // Used cached provider if already resolved.
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    return await cached.InvokeAsync(request);
                }

                var resourceProviders = _providers.AsEnumerable();

                // When provider-name is specified then filter them by name first.
                if (request.Properties.GetNames().Any()) // this means there is a custom name
                {
                    resourceProviders = resourceProviders.Where(p => p.Properties.GetNames().Overlaps(request.Properties.GetNames()));
                }

                // Check if there is a provider that matches the scheme of the absolute uri.
                if (request.Uri.IsAbsolute && !(request.Uri.Scheme == ResourceSchemes.IOnymous))
                {
                    var schemes = new[] { ResourceSchemes.IOnymous, request.Uri.Scheme };
                    resourceProviders = resourceProviders.Where(p => p.Properties.GetSchemes().Overlaps(schemes));
                }

                // GET can search multiple providers.
                if (request.Method == RequestMethod.Get)
                {
                    foreach (var resourceProvider in resourceProviders)
                    {
                        if (await resourceProvider.InvokeAsync(request) is var resource && resource.Exists)
                        {
                            _cache[cacheKey] = resourceProvider;
                            return resource;
                        }
                    }

                    return new InMemoryResource(request.Properties.Copy(Resource.PropertySelector), Stream.Null);
                }
                // Other methods are allowed to use only a single provider.
                else
                {
                    var match = _cache[cacheKey] = resourceProviders.SingleOrThrow();
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

        //public IEnumerator<IResourceProvider> GetEnumerator() => _resourceProviders.GetEnumerator();

        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_resourceProviders).GetEnumerator();

        #endregion
    }
}