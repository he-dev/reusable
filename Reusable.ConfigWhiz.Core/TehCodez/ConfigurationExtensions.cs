using System;
using JetBrains.Annotations;
using Reusable.ConfigWhiz.Data;

namespace Reusable.ConfigWhiz
{
    public static class ConfigurationExtensions
    {
        [CanBeNull]
        public static TContainer Get<TContainer>(this IConfiguration configuration) where TContainer : class, new()
        {
            var identifier = Identifier.Create<TContainer>();
            return configuration.Get<TContainer>(identifier);
        }

        [CanBeNull]
        public static TContainer Get<TConsumer, TContainer>(this IConfiguration configuration) where TContainer : class, new()
        {
            var identifier = Identifier.Create<TConsumer, TContainer>();
            return configuration.Get<TContainer>(identifier);
        }

        [CanBeNull]
        public static TContainer Get<TConsumer, TContainer>(this IConfiguration configuration, TConsumer instance, Func<TConsumer, string> getInstanceName) where TContainer : class, new()
        {
            var identifier = Identifier.Create<TConsumer, TContainer>(instance, getInstanceName);
            return configuration.Get<TContainer>(identifier);
        }
    }
}