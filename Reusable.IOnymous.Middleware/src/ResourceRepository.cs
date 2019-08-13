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
using Reusable.Data;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.IOnymous
{
    public class ResourceRepository
    {
        private readonly RequestCallback<ResourceContext> _requestCallback;

        public ResourceRepository(RequestCallback<ResourceContext> requestCallback)
        {
            _requestCallback = requestCallback;
        }

        public async Task<IResource> InvokeAsync(Request request)
        {
            var context = new ResourceContext
            {
                Request = request
            };

            await _requestCallback(context);

            return context.Response;
        }
    }

    // Provides CRUD APIs.
    public static class ResourceRepositoryExtensions
    {
        public static Task<IResource> CreateAsync(this ResourceRepository resourceRepository, UriString uri, object body, CreateStreamCallback createBodyStreamCallback, IImmutableContainer context = default)
        {
            return resourceRepository.InvokeAsync(new Request.Get(uri)
            {
                Body = body,
                Context = context.ThisOrEmpty(),
                CreateBodyStreamCallback = createBodyStreamCallback
            });
        }

        public static Task<IResource> ReadAsync(this ResourceRepository resourceRepository, Request request)
        {
            return default;
        }

        public static Task<IResource> UpdateAsync(this ResourceRepository resourceRepository, Request request)
        {
            return default;
        }

        public static Task<IResource> DeleteAsync(this ResourceRepository resourceRepository, Request request)
        {
            return default;
        }
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

    [UsedImplicitly]
    public class TelemetryMiddleware
    {
        private readonly RequestCallback<ResourceContext> _next;
        private readonly ILogger<TelemetryMiddleware> _logger;

        public TelemetryMiddleware(RequestCallback<ResourceContext> next, ILogger<TelemetryMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            using (_logger.UseStopwatch())
            {
                await _next(context);
                _logger.Log(Abstraction.Layer.IO().Routine(nameof(InvokeAsync)).Completed());
            }
        }
    }

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
                return;
            }

            var filtered = Filters.Aggregate(_providers.AsEnumerable(), (providers, filter) => filter(providers, context.Request));

            // GET can search multiple providers.
            if (context.Request.Method == RequestMethod.Get)
            {
                foreach (var provider in filtered)
                {
                    if (await InvokeAsync(provider, context.Request) is var resource && resource.Exists)
                    {
                        _cache.Set(providerKey, provider);
                        context.Response = resource;
                        return;
                    }
                }

                context.Response = Resource.DoesNotExist.FromRequest(context.Request);
            }
            // Other methods are allowed to use only a single provider.
            else
            {
                context.Response = await InvokeAsync(_cache.Set(providerKey, filtered.SingleOrThrow()), context.Request);
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

        public static MiddlewareBuilder UseResources(this MiddlewareBuilder builder, IEnumerable<IResourceProvider> providers)
        {
            return builder.Add<ResourceMiddleware>(providers);
        }
    }
}