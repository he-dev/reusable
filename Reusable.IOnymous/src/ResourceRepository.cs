using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;

namespace Reusable.IOnymous
{
    public class ResourceRepository
    {
        private static readonly IEnumerable<ResourceProviderFilterCallback> Filters = new ResourceProviderFilterCallback[]
        {
            ResourceProviderFilters.FilterByName,
            ResourceProviderFilters.FilterByScheme
        };

        private readonly IImmutableList<IResourceProvider> _providers;
        private readonly RequestCallback<ResourceContext> _requestCallback;
        private readonly IMemoryCache _cache;

        public ResourceRepository(IEnumerable<IResourceProvider> providers, RequestCallback<ResourceContext> requestCallback)
        {
            _providers = providers.ToImmutableList();
            _requestCallback = requestCallback;
            _cache = new MemoryCache(new MemoryCacheOptions { });
        }

        public async Task<IResource> InvokeAsync(Request request)
        {
            var resourceContext = new ResourceContext
            {
                Request = request
            };

            await _requestCallback(resourceContext);

            var providerKey = request.Uri.ToString();

            // Used cached provider if already resolved.
            if (_cache.TryGetValue<IResourceProvider>(providerKey, out var entry))
            {
                return await InvokeAsync(entry, request);
            }

            var filtered = Filters.Aggregate(_providers.AsEnumerable(), (providers, filter) => filter(providers, request));

            // GET can search multiple providers.
            if (request.Method == RequestMethod.Get)
            {
                foreach (var provider in filtered)
                {
                    if (await InvokeAsync(provider, request) is var resource && resource.Exists)
                    {
                        _cache.Set(providerKey, provider);
                        return resource;
                    }
                }

                return Resource.DoesNotExist.FromRequest(request);
            }
            // Other methods are allowed to use only a single provider.
            else
            {
                return await InvokeAsync(_cache.Set(providerKey, filtered.SingleOrThrow()), request);
            }
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

    public static class ResourceRepositoryExtensions
    {
        public static IResource CreateAsync(Request request)
        {
            return default;
        }

        public static IResource ReadAsync(Request request)
        {
            return default;
        }

        public static IResource UpdateAsync(Request request)
        {
            return default;
        }

        public static IResource DeleteAsync(Request request)
        {
            return default;
        }
    }

    public abstract class ResourceActionAttribute : Attribute
    {
        protected ResourceActionAttribute(RequestMethod method) => Method = method;

        public RequestMethod Method { get; }
    }

    public class ResourceGetAttribute : ResourceActionAttribute
    {
        public ResourceGetAttribute() : base(RequestMethod.Get) { }
    }

    public class ResourcePostAttribute : ResourceActionAttribute
    {
        public ResourcePostAttribute() : base(RequestMethod.Post) { }
    }

    public class ResourcePutAttribute : ResourceActionAttribute
    {
        public ResourcePutAttribute() : base(RequestMethod.Put) { }
    }

    public class ResourceDeleteAttribute : ResourceActionAttribute
    {
        public ResourceDeleteAttribute() : base(RequestMethod.Delete) { }
    }

    public class ResourceContext
    {
        public Request Request { get; set; }

        public IResource Response { get; set; }
    }

    public class EnvironmentVariableMiddleware
    {
        private readonly RequestCallback<ResourceContext> _next;

        public EnvironmentVariableMiddleware(RequestCallback<ResourceContext> next)
        {
            _next = next;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            if (context.Request.Uri.Scheme.Equals("file"))
            {
                context.Request.Uri = Resolve(context.Request.Uri);
            }

            await _next(context);
        }

        private static UriString Resolve(UriString uri)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(uri.Path.Decoded.ToString());
            var normalizedPath = UriStringHelper.Normalize(expandedPath);
            uri = uri.With(x => x.Path, new UriStringComponent(normalizedPath));
            if (!uri.Scheme && Path.IsPathRooted(uri.Path.Decoded.ToString()))
            {
                uri = uri.With(x => x.Scheme, "file");
            }

            return uri;
        }
    }

    [UsedImplicitly]
    public class BaseUriMiddleware
    {
        private readonly UriString _baseUri;
        private readonly RequestCallback<ResourceContext> _next;

        public BaseUriMiddleware(RequestCallback<ResourceContext> next, UriString baseUri)
        {
            _next = next;
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            if (context.Request.Uri.Scheme.Equals("file") && !Path.IsPathRooted(context.Request.Uri.Path.Decoded.ToString()))
            {
                context.Request.Uri = _baseUri + context.Request.Uri;
            }

            await _next(context);
        }
    }

    [UsedImplicitly]
    public class LambdaMiddleware
    {
        private readonly RequestCallback<ResourceContext> _next;
        private readonly Func<ResourceContext, RequestCallback<ResourceContext>, Task> _lambda;

        public LambdaMiddleware(RequestCallback<ResourceContext> next, Func<ResourceContext, RequestCallback<ResourceContext>, Task> lambda)
        {
            _next = next;
            _lambda = lambda;
        }

        public Task InvokeAsync(ResourceContext context)
        {
            return _lambda(context, _next);
        }
    }

    public static class MiddlewareBuilderExtensions
    {
        public static MiddlewareBuilder Use<T>(this MiddlewareBuilder builder, params object[] parameters)
        {
            return builder.Add<T>(parameters);
        }

        public static MiddlewareBuilder Use(this MiddlewareBuilder builder, Func<ResourceContext, RequestCallback<ResourceContext>, Task> lambda)
        {
            return builder.Add<LambdaMiddleware>(lambda);
        }
    }
}