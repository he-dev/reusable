using System;
using System.Collections.Immutable;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable
{
    public class ImmutableServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider? _parent;
        private readonly IImmutableDictionary<Type, object> _services;

        public ImmutableServiceProvider(IImmutableDictionary<Type, object>? services = default, IServiceProvider? parent = default)
        {
            _services = services ?? ImmutableDictionary<Type, object>.Empty;
            _parent = parent;
        }

        public static ImmutableServiceProvider Empty => new ImmutableServiceProvider(ImmutableDictionary<Type, object>.Empty);

        /// <summary>
        /// Adds a non-null service to the collection. 
        /// </summary>
        public ImmutableServiceProvider Add(Type type, object? service)
        {
            return service switch { {} => new ImmutableServiceProvider(_services.Add(type, service), _parent), _ => this };
        }

        public object? GetService(Type type) => _services.TryGetValue(type, out var service) ? service : _parent?.GetService(type);
    }

    public static class ImmutableServiceProviderExtensions
    {
        public static ImmutableServiceProvider Add<T>(this ImmutableServiceProvider services, T service) => services.Add(typeof(T), service);
    }
}