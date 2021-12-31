using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Essentials;

namespace Reusable.Experiments;

public static class ServicePipelineDemo
{
    public static async Task Test()
    {
        // Compose the container.
        var builder = new ContainerBuilder();

        // Register service-pipeline and associate it with ReadFile.Text.
        builder.Register(_ => new ServicePipeline<string>
        {
            new EnvironmentVariableService<string>(),
            // Override cache-lifetime for all txt-files.
            new CacheLifetimeService<string>(x => x.Key.EndsWith(".txt") ? TimeSpan.FromMinutes(30) : TimeSpan.Zero),
            new CacheService<string>(new MemoryCache(new MemoryCacheOptions()))
        }).InstancePerDependency().Named<ServicePipeline<string>>(nameof(ReadFile.Text));

        await using var container = builder.Build();
        await using var scope = container.BeginLifetimeScope();

        // Set some environment variable.
        Environment.SetEnvironmentVariable("HOME", @"c:\temp");

        var result = 
            await new ReadFile.Text(@"%HOME%\notes.txt")
                .CacheLifetime(TimeSpan.FromMinutes(10))
                .Bind(scope)
                .InvokeAsync();

        Console.WriteLine(result); // --> c:\temp\notes.txt
    }
}

// Builds service pipeline.
public class ServicePipeline<T> : IEnumerable<IService<T>>
{
    private IService<T> First { get; } = new Service<T>.Empty();

    // Adds the specified service at the end of the pipeline.
    public void Add(IService<T> last) => First.Enumerate().Last().Next = last;

    public IEnumerator<IService<T>> GetEnumerator() => First.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// Marks property that requires an external dependency.
[AttributeUsage(AttributeTargets.Property)]
public class DependencyAttribute : Attribute
{
    public bool Required { get; set; } = true;
}

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
        
        // To resolve the pipeline use either a custom tag or the typename.
        var serviceTag = service.Tag() is { } tag ? tag : service.GetType().Name;

        return
            // Try to resolve the pipeline for this service.
            services.ResolveOptionalNamed<ServicePipeline<string>>(serviceTag) is ServicePipeline<T> pipeline
            // Attach current service at the end of the pipeline and return the first service.
                ? pipeline.Also(p => p.Last().Next = service).First()
                // Otherwise use the current service.
                : service;
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

public static class ServiceItems
{
    public static T GetItem<T>(this IItems service, string name)
    {
        return
            service.Items.TryGetValue(typeof(T).Name, out var value) && value is T result
                ? result
                : throw DynamicException.Create("ItemNotFound", $"Could not find item '{name}'.");
    }

    public static T GetItemOrDefault<T>(this IItems service, string name, T fallback)
    {
        return
            service.Items.TryGetValue(name, out var value) && value is T result
                ? result
                : fallback;
    }

    // Sets CacheLifetime.
    public static IService<T> CacheLifetime<T>(this IService<T> service, TimeSpan lifetime)
    {
        return service.Also(s => s.Items[nameof(CacheLifetime)] = lifetime);
    }

    // Gets CacheLifetime.
    public static TimeSpan CacheLifetime<T>(this T service) where T : IItems
    {
        return service.GetItemOrDefault(nameof(CacheLifetime), TimeSpan.Zero);
    }

    // Sets Tag.
    public static IService<T> Tag<T>(this IService<T> service, string value)
    {
        return service.Also(s => s.Items[nameof(Tag)] = value);
    }

    // Gets Tag.
    public static string? Tag<T>(this T service) where T : IItems
    {
        return service.GetItemOrDefault(nameof(Tag), default(string));
    }
}

// -- Services

public interface IItems
{
    // Allows to store additional metadata.
    IDictionary<string, object> Items { get; }
}

public interface IService<T> : IItems, IEnumerable<IService<T>>
{
    // Points to the next service in a pipeline.
    IService<T>? Next { get; set; }

    Task<T> InvokeAsync();
}

public abstract class Service<T> : IService<T>
{
    public IService<T>? Next { get; set; }

    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

    public abstract Task<T> InvokeAsync();

    public IEnumerator<IService<T>> GetEnumerator() => this.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Null-Service.
    public class Empty : Service<T>
    {
        public override async Task<T> InvokeAsync() => await Next?.InvokeAsync();
    }
}

public interface IVariable<T>
{
    public T Value { get; set; }
}

public interface IAssociable<out T>
{
    public T Key { get; }
}

public abstract class ReadFile<T> : Service<T>, IVariable<string>, IAssociable<string>
{
    protected ReadFile(string name) => Name = name;

    public string Name { get; set; }

    string IVariable<string>.Value
    {
        get => Name;
        set => Name = value;
    }

    string IAssociable<string>.Key => Name;
}

public static class ReadFile
{
    // Reads files as string.
    public class Text : ReadFile<string>
    {
        public Text(string name) : base(name) { }

        public override Task<string> InvokeAsync()
        {
            return Task.FromResult($"Hallo from '{Name}'.");
        }
    }
}

// -- Nodes

// Provides caching for service results.
public class CacheService<T> : Service<T>
{
    public CacheService(IMemoryCache cache) => Cache = cache;

    private IMemoryCache Cache { get; }

    public override async Task<T> InvokeAsync()
    {
        if (this.Last().CacheLifetime() is var cacheLifetime && cacheLifetime > TimeSpan.Zero)
        {
            Console.WriteLine(cacheLifetime);
        }

        return await Next?.InvokeAsync();
    }
}

// Allows to associate cache-lifetime to a service.
public class CacheLifetimeService<T> : Service<T>
{
    public CacheLifetimeService(Func<IAssociable<string>, TimeSpan> lifetimeFunc) => LifetimeFunc = lifetimeFunc;

    private Func<IAssociable<string>, TimeSpan> LifetimeFunc { get; }

    public override async Task<T> InvokeAsync()
    {
        if (this.Last() is var last)
        {
            if (last is IAssociable<string> associable && LifetimeFunc(associable) is var lifetime)
            {
                last.CacheLifetime(lifetime);
            }
        }

        return await Next?.InvokeAsync();
    }
}

public class EnvironmentVariableService<T> : Service<T>
{
    public override async Task<T> InvokeAsync()
    {
        if (this.Last() is IVariable<string> identifiable)
        {
            identifiable.Value = Environment.ExpandEnvironmentVariables(identifiable.Value);
        }

        return await Next?.InvokeAsync()!;
    }
}