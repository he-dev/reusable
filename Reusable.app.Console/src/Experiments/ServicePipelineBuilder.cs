using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Essentials;

namespace Reusable.Experiments.Synergy1;

public static class ServicePipelineDemo
{
    public static async Task Test()
    {
        // Compose the container.
        var builder = new ContainerBuilder();

        builder.RegisterInstance(new MemoryCache(new MemoryCacheOptions())).As<IMemoryCache>().SingleInstance();

        // Register service-pipeline and associate it with ReadFile.Text.
        builder.Register(c => new ServicePipelineBuilder<string>
        {
            //new Constant.Text("This is not a real file!"),
            new EnvironmentVariableService<string>(PropertyService.For<ReadFile.Text>.Select(x => x.Name)),
            new CacheLifetimeService<string>(TimeSpan.FromMinutes(15))
            {
                Rules =
                {
                    // Override cache-lifetime for all txt-files.
                    ConditionService.For<ReadFile.Text>.When(x => x.Name.EndsWith(".txt"), ".txt").Then(TimeSpan.FromMinutes(30))
                }
            },
            new CacheService<string>(c.Resolve<IMemoryCache>(), PropertyService.For<ReadFile.Text>.Select(x => x.Name))
        }).InstancePerDependency().Named<ServicePipelineBuilder<string>>(nameof(ReadFile.Text));

        await using var container = builder.Build();
        await using var scope = container.BeginLifetimeScope();

        // Set some environment variable.
        Environment.SetEnvironmentVariable("HOME", @"c:\temp");

        var result = await new ReadFile.Text(@"%HOME%\notes.txt").CacheLifetime(TimeSpan.FromMinutes(10)).InvokeAsync(scope);

        Console.WriteLine(result); // --> c:\temp\notes.txt
    }
}

// Builds service pipeline.
public class ServicePipelineBuilder<T> : IEnumerable<IService<T>>
{
    private IService<T> First { get; } = new Service<T>.Empty();

    // Adds the specified service at the end of the pipeline.
    public ServicePipelineBuilder<T> Add(IService<T> last) => this.Also(b => b.First.Enumerate().Last().Next = last);

    public IEnumerator<IService<T>> GetEnumerator() => First.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IService<T> Build() => First;
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

        return
            // Try to resolve the pipeline for this service.
            services.ResolveOptionalNamed<ServicePipelineBuilder<T>>(service.Tag()) is { } pipeline
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
    public static string Tag<T>(this T service) where T : IItems
    {
        // To resolve the pipeline use either a custom tag or the typename.
        return service.GetItemOrDefault(nameof(Tag), service.GetType().Name);
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
    private IService<T>? _last;

    public IService<T>? Next { get; set; }

    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

    protected IService<T> Last => _last ??= this.Last();

    public abstract Task<T> InvokeAsync();

    public IEnumerator<IService<T>> GetEnumerator() => this.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected async Task<T> InvokeNext() => await Next?.InvokeAsync();

    // Null-Service.
    public class Empty : Service<T>
    {
        public override async Task<T> InvokeAsync() => await Next?.InvokeAsync();
    }
}

public abstract class ReadFile<T> : Service<T>
{
    protected ReadFile(string name) => Name = name;

    public string Name { get; set; }
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

public static class Constant
{
    // Reads files as string.
    public class Text : Service<string>
    {
        public Text(string value) => Value = value;

        private string Value { get; }

        public override Task<string> InvokeAsync()
        {
            return Task.FromResult(Value);
        }
    }
}

// -- Nodes

// Provides caching for service results.
public class CacheService<T> : Service<T>
{
    public CacheService(IMemoryCache cache, IPropertyService<string> key) => (Cache, Key) = (cache, key);

    private IMemoryCache Cache { get; }

    private IPropertyService<string> Key { get; }

    public override async Task<T> InvokeAsync()
    {
        if (Last.CacheLifetime() is var cacheLifetime && cacheLifetime > TimeSpan.Zero)
        {
            Console.WriteLine($"Cache-lifetime: {cacheLifetime}");
        }

        return await InvokeNext();
    }
}

// Allows to associate cache-lifetime to with a service.
public class CacheLifetimeService<T> : Service<T>
{
    public CacheLifetimeService(TimeSpan fallback)
    {
        if (fallback == TimeSpan.Zero) throw new ArgumentException("Fallback value needs to be greater than zero.");
        Fallback = fallback;
    }

    private TimeSpan Fallback { get; }

    public List<ConditionBag<TimeSpan>> Rules { get; } = new();

    public override async Task<T> InvokeAsync()
    {
        var lifetime = Rules.Where(c => c.Evaluate(Last)).Select(c => c.GetValue()).FirstOrDefault();

        Last.CacheLifetime(lifetime > TimeSpan.Zero ? lifetime : Fallback);

        return await InvokeNext();
    }
}

public class EnvironmentVariableService<T> : Service<T>
{
    public EnvironmentVariableService(IPropertyService<string> property) => Property = property;

    private IPropertyService<string> Property { get; }

    public override async Task<T> InvokeAsync()
    {
        Property.SetValue(Last, Environment.ExpandEnvironmentVariables(Property.GetValue(Last)));
        
        return await InvokeNext()!;
    }
}

public interface IPropertyService<TValue>
{
    Func<object, TValue> GetValue { get; }

    Action<object, TValue> SetValue { get; }
}

public record PropertyService<TValue>(Func<object, TValue> GetValue, Action<object, TValue> SetValue) : IPropertyService<TValue>;

public static class PropertyService
{
    private static readonly ConcurrentDictionary<(Type, string), object> Cache = new();

    public abstract class For<TSource>
    {
        public static IPropertyService<TValue> Select<TValue>(Expression<Func<TSource, TValue>> expression)
        {
            if (expression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException($"Expression must be a {nameof(MemberExpression)}");
            }

            return (IPropertyService<TValue>)Cache.GetOrAdd((typeof(TSource), memberExpression.Member.Name), _ =>
            {
                var targetParameter = Expression.Parameter(typeof(object), "target");
                var valueParameter = Expression.Parameter(typeof(TValue), "value");

                var casted = ParameterConverter<TSource>.Rewrite(memberExpression, targetParameter);

                // ((T)target).Property
                var getter =
                    Expression.Lambda<Func<object, TValue>>(
                        casted,
                        targetParameter
                    ).Compile();

                // ((T)target).Property = value
                var setter =
                    Expression.Lambda<Action<object, TValue>>(
                        Expression.Assign(casted, valueParameter),
                        targetParameter, valueParameter
                    ).Compile();

                return new PropertyService<TValue>(getter, setter);
            });
        }
    }
}

// --- Condition service

public interface IConditionService
{
    Func<object, bool> Evaluate { get; }
}

public record ConditionService(Func<object, bool> Evaluate) : IConditionService
{
    private static readonly ConcurrentDictionary<(Type, string), object> Cache = new();

    public abstract class For<TSource>
    {
        public static IConditionService When(Expression<Func<TSource, bool>> expression, string tag)
        {
            return (IConditionService)Cache.GetOrAdd((typeof(TSource), tag), _ =>
            {
                var targetParameter = Expression.Parameter(typeof(object), "target");

                var casted = ParameterConverter<TSource>.Rewrite(expression.Body, targetParameter);

                // ((T)target)() -> bool
                var evaluate =
                    Expression.Lambda<Func<object, bool>>(
                        casted,
                        targetParameter
                    ).Compile();

                return new ConditionService(evaluate);
            });
        }
    }
}

public record ConditionBag<T>(Func<object, bool> Evaluate, Func<T> GetValue) : IConditionService;

public static class ConditionBagFactory
{
    public static ConditionBag<T> Then<T>(this IConditionService condition, T value)
    {
        return new ConditionBag<T>(condition.Evaluate, () => value);
    }
}

// Casts the target object to T and exchanges the parameter for an 'object'.
public class ParameterConverter<T> : ExpressionVisitor
{
    private ParameterExpression ObjectParameter { get; init; } = null!;

    public static Expression Rewrite(Expression expression, ParameterExpression parameter)
    {
        return new ParameterConverter<T> { ObjectParameter = parameter }.Visit(expression);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return Expression.Convert(ObjectParameter, typeof(T));
    }
}