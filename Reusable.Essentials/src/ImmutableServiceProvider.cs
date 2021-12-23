using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Reusable.Essentials;

[PublicAPI]
public class ImmutableServiceProvider : IServiceProvider
{
    public ImmutableServiceProvider(IEnumerable<KeyValuePair<Type, object>> services)
    {
        Services = services.ToImmutableDictionary();
    }

    public ImmutableServiceProvider(IServiceProvider services)
    {
        Child = services;
        Services = ImmutableDictionary<Type, object>.Empty;
    }

    public ImmutableServiceProvider(IEnumerable<KeyValuePair<Type, object>> services, IServiceProvider? child = default)
    {
        Services = services.ToImmutableDictionary();
        Child = child;
    }

    private IImmutableDictionary<Type, object> Services { get; }

    private IServiceProvider? Child { get; }

    public static ImmutableServiceProvider Empty => new(ImmutableDictionary<Type, object>.Empty);

    /// <summary>
    /// Adds a non-null service to the collection. 
    /// </summary>
    public ImmutableServiceProvider Add(Type type, object service)
    {
        return new ImmutableServiceProvider(Services.Add(type, service), Child);
    }

    public object? GetService(Type type)
    {
        if (type == typeof(IServiceProvider)) return this;
        return Services.TryGetValue(type, out var service) ? service : Child?.GetService(type);
    }
}

public static class ImmutableServiceProviderExtensions
{
    public static ImmutableServiceProvider Add<T>(this ImmutableServiceProvider services, [DisallowNull] T service) => services.Add(typeof(T), service);
}