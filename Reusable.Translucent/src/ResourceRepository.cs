using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    [PublicAPI]
    public interface IResourceRepository
    {
        Task<Response> InvokeAsync(Request request);
    }

    [PublicAPI]
    public class ResourceRepository : IResourceRepository //<TSetup> : IResourceRepository where TSetup : new()
    {
        private readonly RequestDelegate<ResourceContext> _requestDelegate;

        internal ResourceRepository(RequestDelegate<ResourceContext> requestDelegate)
        {
            _requestDelegate = requestDelegate; 
        }
        
        public static ResourceRepositoryBuilder Builder() => new ResourceRepositoryBuilder();

        public static IResourceRepository From<TSetup>(IServiceProvider services) where TSetup : IResourceRepositorySetup, new()
        {
            var setup = new TSetup();

            var builder = Builder();

            foreach (var controller in setup.Controllers(services))
            {
                builder.Add(controller);
            }

            foreach (var (type, args) in setup.Middleware(services))
            {
                builder.Use(type, args);
            }

            return builder.Build(services);
        }
        
        public async Task<Response> InvokeAsync(Request request)
        {
            var context = new ResourceContext
            {
                Request = request
            };

            await _requestDelegate(context);

            return context.Response;
        }
    }


    public interface IResourceRepositorySetup
    {
        IEnumerable<IResourceController> Controllers(IServiceProvider services);

        IEnumerable<IMiddlewareInfo> Middleware(IServiceProvider services);
    }

    public abstract class ResourceRepositorySetup : IResourceRepositorySetup
    {
        public abstract IEnumerable<IResourceController> Controllers(IServiceProvider services);

        public virtual IEnumerable<IMiddlewareInfo> Middleware(IServiceProvider services)
        {
            yield break;
        }

        protected static IMiddlewareInfo Use<T>(params object[] args) => MiddlewareInfo.Create<T>(args);
    }

    public interface IMiddlewareInfo
    {
        Type Type { get; }

        object[] Args { get; }

        void Deconstruct(out Type type, out object[] args);
    }

    public class MiddlewareInfo : IMiddlewareInfo
    {
        public Type Type { get; set; }

        public object[] Args { get; set; }

        public static MiddlewareInfo Create<T>(params object[] args) => new MiddlewareInfo
        {
            Type = typeof(T),
            Args = args
        };

        public void Deconstruct(out Type type, out object[] args)
        {
            type = Type;
            args = Args;
        }
    }

    public interface IMiddleware
    {
        Task InvokeAsync(ResourceContext context);
    }

    public abstract class MiddlewareBase : IMiddleware
    {
        protected MiddlewareBase(RequestDelegate<ResourceContext> next, IServiceProvider services)
        {
            Next = next;
            Services = services;
        }

        protected RequestDelegate<ResourceContext> Next { get; }

        protected IServiceProvider Services { get; }

        public abstract Task InvokeAsync(ResourceContext context);

        protected Task InvokeNext(ResourceContext context) => Next?.Invoke(context);
    }
}