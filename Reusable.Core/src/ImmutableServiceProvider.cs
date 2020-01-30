using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable
{
    public class ImmutableServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider? _child;
        private readonly IImmutableDictionary<Type, object> _services;

        public ImmutableServiceProvider(IEnumerable<KeyValuePair<Type, object>> services)
        {
            _services = services.ToImmutableDictionary() ?? ImmutableDictionary<Type, object>.Empty;
        }
        
        public ImmutableServiceProvider(IServiceProvider services)
        {
            _child = services;
            _services = ImmutableDictionary<Type, object>.Empty;
        }

        public ImmutableServiceProvider(IEnumerable<KeyValuePair<Type, object>> services, IServiceProvider child)
        {
            _services = services.ToImmutableDictionary();
            _child = child;
        }

        public static ImmutableServiceProvider Empty => new ImmutableServiceProvider(ImmutableDictionary<Type, object>.Empty);

        /// <summary>
        /// Adds a non-null service to the collection. 
        /// </summary>
        public ImmutableServiceProvider Add(Type type, object service)
        {
            return new ImmutableServiceProvider(_services.Add(type, service), _child);
        }

        public object? GetService(Type type)
        {
            if (type == typeof(IServiceProvider)) return this;
            return _services.TryGetValue(type, out var service) ? service : _child?.GetService(type);
        }
    }

    public static class ImmutableServiceProviderExtensions
    {
        public static ImmutableServiceProvider Add<T>(this ImmutableServiceProvider services, T service) => services.Add(typeof(T), service);
    }
}