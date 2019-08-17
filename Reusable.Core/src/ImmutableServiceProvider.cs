using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable
{
    public class ImmutableServiceProvider : IServiceProvider
    {
        private readonly IImmutableDictionary<Type, object> _services;

        private ImmutableServiceProvider(IImmutableDictionary<Type, object> services)
        {
            _services = services;
        }

        public static ImmutableServiceProvider Empty => new ImmutableServiceProvider(ImmutableDictionary<Type, object>.Empty);

        public ImmutableServiceProvider Add(Type type, object service) => new ImmutableServiceProvider(_services.Add(type, service));

        //public bool IsRegistered(Type type) => _services.ContainsKey(type);

        public object GetService(Type type)
        {
            return
                _services.TryGetValue(type, out var service)
                    ? service
                    : throw DynamicException.Create("ServiceNotFound", $"There is no service of type '{type.ToPrettyString()}'.");
        }
    }

    public static class ImmutableServiceProviderExtensions
    {
        public static ImmutableServiceProvider Add<T>(this ImmutableServiceProvider services, T service)
        {
            return services.Add(typeof(T), service);
        }
    }
}