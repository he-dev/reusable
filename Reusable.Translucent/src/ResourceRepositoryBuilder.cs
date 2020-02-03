using System;
using System.Collections.Generic;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public class ResourceRepositoryBuilder
    {
        private readonly List<IResourceController> _controllers = new List<IResourceController>();
        private readonly List<IMiddlewareInfo> _middleware = new List<IMiddlewareInfo>();
        private readonly List<KeyValuePair<Type, object>> _services = new List<KeyValuePair<Type, object>>();

        public ResourceRepositoryBuilder Add(params IResourceController[] controllers)
        {
            _controllers.AddRange(controllers);
            return this;
        }

        public ResourceRepositoryBuilder Use(Type type, object[] args)
        {
            if (type == typeof(ResourceProvider)) throw new ArgumentException(paramName: nameof(type), message: $"{nameof(ResourceProvider)} is added implicitly.");
            _middleware.Add(new MiddlewareInfo<ResourceContext> { Type = type, Args = args });
            return this;
        }

        public ResourceRepositoryBuilder Register<T>(T instance)
        {
            _services.Add(new KeyValuePair<Type, object>(typeof(T), instance));
            return this;
        }

        public IResourceRepository Build(IServiceProvider? services = default)
        {
            services = new ImmutableServiceProvider(_services, services ?? ImmutableServiceProvider.Empty).Add<IEnumerable<IResourceController>>(_controllers);
            
            _middleware.Add(MiddlewareInfo<ResourceContext>.Create<ResourceProvider>());

            var pipeline = PipelineFactory.CreatePipeline<ResourceContext>(_middleware, services);
            return new ResourceRepository(pipeline);
        }
    }

    public static class RepositoryBuilderExtensions
    {
        public static ResourceRepositoryBuilder Use<T>(this ResourceRepositoryBuilder builder, params object[] args)
        {
            return builder.Use(typeof(T), args);
        }
    }
}