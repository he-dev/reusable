using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Controllers;
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
            if (type == typeof(ResourceMiddleware)) throw new ArgumentException(paramName: nameof(type), message: $"{nameof(ResourceMiddleware)} is added implicitly.");
            _middleware.Add(new MiddlewareInfo{Type = type, Args = args});
            return this;
        }
        
        public ResourceRepositoryBuilder Use<T>(params object[] args)
        {
            return Use(typeof(T), args);
        }

        public ResourceRepositoryBuilder Register<T>(T instance)
        {
            _services.Add(new KeyValuePair<Type, object>(typeof(T), instance));
            return this;
        }

        public IResourceRepository Build(IServiceProvider? services = default)
        {
            services = new ImmutableServiceProvider(_services, services ?? ImmutableServiceProvider.Empty).Add<IEnumerable<IResourceController>>(_controllers);

            var pipelineBuilder = new PipelineBuilder<ResourceContext>(services);

            foreach (var (type, args) in _middleware)
            {
                pipelineBuilder.UseMiddleware(type, args);
            }

            // This is the default middleware that is always the last one.
            pipelineBuilder.UseMiddleware<ResourceMiddleware>();

            return new ResourceRepository(pipelineBuilder.Build());
        }
    }
}