using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public class ResourceRepositoryBuilder
    {
        private object? _setup;
        private MethodInfo? _configureResourcesMethod;
        private MethodInfo? _configurePipelineMethod;

        public static ResourceRepositoryBuilder Empty => new ResourceRepositoryBuilder();

        public ResourceRepositoryBuilder UseSetup<TSetup>() where TSetup : new()
        {
            _setup = new TSetup();
            _configureResourcesMethod = GetConfigureMethod<TSetup>(nameof(QuickSetup<ReflectionContext>.ConfigureResources), isOptional: false);
            _configurePipelineMethod = GetConfigureMethod<TSetup>(nameof(QuickSetup<ReflectionContext>.ConfigurePipeline), isOptional: true);

            ValidateMethodHasMandatoryParameters(_configureResourcesMethod, new[] { typeof(IResourceCollection) });
            ValidateMethodHasMandatoryParameters(_configurePipelineMethod, new[] { typeof(IPipelineBuilder<ResourceContext>) });

            return this;
        }

        private static MethodInfo? GetConfigureMethod<T>(string methodName, bool isOptional)
        {
            return typeof(T).GetMethod(methodName) switch
            {
                {} m => m,
                _ when isOptional => default,
                _ => throw DynamicException.Create
                (
                    $"{methodName}MethodNotFound",
                    $"'{typeof(T).ToPrettyString()}' does not define the '{methodName}' method."
                )
            };
        }

        private static void ValidateMethodHasMandatoryParameters(MethodInfo? method, IEnumerable<Type> mandatoryParameterTypes)
        {
            if (method is {})
            {
                var missingParameterTypes = mandatoryParameterTypes.Except(method.GetParameters().Select(p => p.ParameterType)).ToList();
                if (missingParameterTypes.Any())
                {
                    throw DynamicException.Create
                    (
                        "MethodParameterNotFound",
                        $"One or more '{method.Name}' method parameters not found: [{missingParameterTypes.Select(t => t.ToPrettyString()).Join(", ")}]."
                    );
                }
            }
        }

        public RequestDelegate<ResourceContext> Build(IServiceProvider serviceProvider)
        {
            var resources = new ResourceCollection();
            var pipelineBuilder = new PipelineBuilder<ResourceContext>(new ImmutableServiceProvider(parent: serviceProvider).Add<IResourceCollection>(resources));

            serviceProvider =
                new ImmutableServiceProvider(parent: serviceProvider)
                    .Add<IResourceCollection>(resources)
                    .Add<IPipelineBuilder<ResourceContext>>(pipelineBuilder);

            InvokeSetupMethod<IResourceCollection>(_setup, _configureResourcesMethod, serviceProvider);
            InvokeSetupMethod<IPipelineBuilder<ResourceContext>>(_setup, _configurePipelineMethod, serviceProvider);

            // This is the default middleware that is always the last one.
            pipelineBuilder.UseMiddleware<ResourceMiddleware>();

            return pipelineBuilder.Build();
        }

        private static void InvokeSetupMethod<T>(object setup, MethodInfo configureMethod, IServiceProvider services)
        {
            if (configureMethod is null)
            {
                return;
            }

            var parameterValues =
                configureMethod
                    .GetParameters()
                    .Select(p => services.Resolve(p.ParameterType))
                    .ToArray();

            configureMethod.Invoke(setup, parameterValues);
        }
    }
}