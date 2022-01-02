using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Reusable.Synergy.Annotations;

namespace Reusable.Synergy;

public static class ServiceBinding
{
    private static readonly ConcurrentDictionary<Type, IEnumerable<Dependency>> Properties = new();

    // Resolves dependencies for properties marked with the [DependencyAttribute] and the service-pipeline.
    public static IService<T> Bind<T>(this IService<T> service, IComponentContext services)
    {
        foreach (var dependency in Properties.GetOrAdd(typeof(T), type => type.DependentProperties().ToList()))
        {
            dependency.Resolve(service, services);
        }

        return
            // Try to resolve the pipeline for this service.
            services.ResolveOptionalNamed<Service<T>.PipelineBuilder>(service.Tag()) is { } pipeline
                // Attach current service at the end of the pipeline and return the first service.
                ? pipeline.Add(service).Build()
                // Otherwise use the current service.
                : service;
    }

    // Convenience extension that binds and invokes the service at the same time.
    public static async Task<T> InvokeAsync<T>(this IService<T> service, IComponentContext services)
    {
        return await service.Bind(services).InvokeAsync().ConfigureAwait(false);
    }

    // Gets info about dependent properties.
    private static IEnumerable<Dependency> DependentProperties(this IReflect type)
    {
        return
            from property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            let dependency = property.GetCustomAttribute<DependencyAttribute>()
            where dependency is { }
            select new Dependency(property, dependency.Required);
    }

    // Enumerates service pipeline.
    public static IEnumerable<IService<T>> Enumerate<T>(this IService<T> service)
    {
        for (var current = service; current is { }; current = current.Next)
        {
            yield return current;
        }
    }
    
    public static IEnumerable<INode> Enumerate(this INode service)
    {
        for (var current = service; current is { }; current = current.Next)
        {
            yield return current;
        }
    }

    // Stores property dependency info and resolves it.
    private record Dependency(PropertyInfo Property, bool Required)
    {
        public void Resolve(object target, IComponentContext scope)
        {
            if (Required)
            {
                Property.SetValue(target, scope.Resolve(Property.PropertyType));
            }
            else
            {
                if (scope.TryResolve(Property.PropertyType, out var dependency))
                {
                    Property.SetValue(target, dependency);
                }
            }
        }
    }
}