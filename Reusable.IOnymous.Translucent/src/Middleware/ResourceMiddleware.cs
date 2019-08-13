using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;

namespace Reusable.IOnymous.Middleware
{
    [UsedImplicitly]
    public class ResourceMiddleware
    {
        private static readonly IEnumerable<ResourceProviderFilterCallback> Filters = new ResourceProviderFilterCallback[]
        {
            ResourceProviderFilters.FilterByName,
            ResourceProviderFilters.FilterByScheme
        };

        private readonly IImmutableList<IResourceProvider> _providers;
        private readonly RequestCallback<ResourceContext> _next;
        private readonly IMemoryCache _cache;

        public ResourceMiddleware(RequestCallback<ResourceContext> next, IEnumerable<IResourceProvider> providers)
        {
            _next = next;
            _providers = providers.ToImmutableList();
            _cache = new MemoryCache(new MemoryCacheOptions { });
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            var providerKey = context.Request.Uri.ToString();

            // Used cached provider if already resolved.
            if (_cache.TryGetValue<IResourceProvider>(providerKey, out var entry))
            {
                context.Response = await InvokeAsync(entry, context.Request);
            }
            else
            {
                var filtered = Filters.Aggregate(_providers.AsEnumerable(), (providers, filter) => filter(providers, context.Request));

                // GET can search multiple providers.
                if (context.Request.Method == RequestMethod.Get)
                {
                    context.Response = Resource.DoesNotExist.FromRequest(context.Request);
                    foreach (var provider in filtered)
                    {
                        if (await InvokeAsync(provider, context.Request) is var resource && resource.Exists)
                        {
                            _cache.Set(providerKey, provider);
                            context.Response = resource;
                            break;
                        }
                    }
                }
                // Other methods are allowed to use only a single provider.
                else
                {
                    context.Response = await InvokeAsync(_cache.Set(providerKey, filtered.SingleOrThrow()), context.Request);
                }
            }

            await _next(context);
        }

        private Task<IResource> InvokeAsync(IResourceProvider provider, Request request)
        {
            var method =
                provider
                    .GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .SingleOrDefault(m => m.GetCustomAttribute<ResourceActionAttribute>()?.Method == request.Method);

            return (Task<IResource>)method.Invoke(provider, new object[] { request });
        }

        // crud
    }
}