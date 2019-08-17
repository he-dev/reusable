using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public interface IResourceRepository : IDisposable
    {
        Task<Response> InvokeAsync(Request request);
    }

    public class ResourceRepository<TSetup> : IResourceRepository where TSetup : new()
    {
        private readonly RequestDelegate<ResourceContext> _requestDelegate;

        public ResourceRepository(IServiceProvider services)
        {
            var setup = new TSetup();

            var resourceControllerBuilder = new ResourceControllerBuilder(services);
            var resourceRepositoryBuilder = new ResourceRepositoryBuilder(services);

            InvokeMethod<IResourceControllerBuilder>(setup, nameof(ResourceRepository.QuickSetup.ConfigureServices), resourceControllerBuilder, services);
            InvokeMethod<IResourceRepositoryBuilder>(setup, nameof(ResourceRepository.QuickSetup.Configure), resourceRepositoryBuilder, services);

            resourceRepositoryBuilder.UseMiddleware<ControllerMiddleware>(new object[] { resourceControllerBuilder.Controllers.AsEnumerable() });

            _requestDelegate = resourceRepositoryBuilder.Build<ResourceContext>();
        }

        private static void InvokeMethod<T>(TSetup setup, string methodName, T defaultParameter, IServiceProvider services)
        {
            var configure = typeof(TSetup).GetMethod(methodName);
            if (configure is null)
            {
                throw DynamicException.Create($"{methodName}MethodNotFound", $"'{typeof(TSetup).ToPrettyString()}' does not specify the '{methodName}' method;");
            }

            var parameterInfos = configure.GetParameters();

            var defaultParameterInfo = parameterInfos.FirstOrDefault();
            if (defaultParameterInfo is null || defaultParameterInfo.ParameterType != typeof(T))
            {
                throw DynamicException.Create("DefaultParameterNotFound", $"'{methodName}' method's first parameter must be '{typeof(T).ToPrettyString()}'.");
            }

            var parameterValues =
                parameterInfos
                    .Skip(1)
                    .Aggregate(
                        new object[] { defaultParameter }.AsEnumerable(),
                        (current, next) => current.Append(services.Resolve(next.ParameterType)))
                    .ToArray();

            configure.Invoke(setup, parameterValues);
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

        public void Dispose() { }
    }

    public static class ResourceRepository
    {
        public static IResourceRepository Create(Action<IResourceControllerBuilder> controller, Action<IResourceRepositoryBuilder> repository = default)
        {
            return new ResourceRepository<QuickSetup>(new LambdaServiceProvider(services =>
            {
                services.Add(typeof(Action<IResourceControllerBuilder>), controller);
                services.Add(typeof(Action<IResourceRepositoryBuilder>), repository ?? (_ => { }));
            }));
        }

        internal class QuickSetup
        {
            public void ConfigureServices(IResourceControllerBuilder controller, Action<IResourceControllerBuilder> configure)
            {
                configure(controller);
            }

            public void Configure(IResourceRepositoryBuilder repository, Action<IResourceRepositoryBuilder> configure)
            {
                configure(repository);
            }
        }
    }
}