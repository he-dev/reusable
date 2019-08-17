using System;

namespace Reusable.Translucent
{
    public interface IResourceRepositoryBuilder
    {
        IServiceProvider ServiceProvider { get; }
        
        IResourceRepositoryBuilder UseMiddleware<T>(params object[] args);

        RequestDelegate<TContext> Build<TContext>();
    }
    
    internal class ResourceRepositoryBuilder : IResourceRepositoryBuilder
    {
        private readonly RequestDelegateBuilder _requestDelegateBuilder;

        public ResourceRepositoryBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            _requestDelegateBuilder = new RequestDelegateBuilder(serviceProvider);
        }

        public IServiceProvider ServiceProvider { get; }

        public IResourceRepositoryBuilder UseMiddleware<T>(params object[] args)
        {
            _requestDelegateBuilder.UseMiddleware<T>(args);
            return this;
        }

        public RequestDelegate<TContext> Build<TContext>()
        {
            return _requestDelegateBuilder.Build<TContext>();
        }
    }
}