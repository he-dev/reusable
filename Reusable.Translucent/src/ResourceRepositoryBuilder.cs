using System;

namespace Reusable.Translucent
{
    public interface IResourceRepositoryBuilder<in TContext>
    {
        IServiceProvider ServiceProvider { get; }
        
        IResourceRepositoryBuilder<TContext> UseMiddleware<T>(params object[] args);

        RequestDelegate<TContext> Build();
    }
    
    internal class ResourceRepositoryBuilder<TContext> : IResourceRepositoryBuilder<TContext>
    {
        private readonly RequestDelegateBuilder<TContext> _requestDelegateBuilder;

        public ResourceRepositoryBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            _requestDelegateBuilder = new RequestDelegateBuilder<TContext>(serviceProvider);
        }

        public IServiceProvider ServiceProvider { get; }

        public IResourceRepositoryBuilder<TContext> UseMiddleware<T>(params object[] args)
        {
            _requestDelegateBuilder.UseMiddleware<T>(args);
            return this;
        }

        public RequestDelegate<TContext> Build()
        {
            return _requestDelegateBuilder.Build();
        }
    }
}