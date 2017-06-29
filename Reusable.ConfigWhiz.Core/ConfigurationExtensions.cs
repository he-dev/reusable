using System;
using JetBrains.Annotations;
using Reusable.ConfigWhiz.Paths;

namespace Reusable.ConfigWhiz
{
    public static class ConfigurationExtensions
    {
        [CanBeNull]
        public static TContainer Get<TContainer>(this IConfiguration configuration, IdentifierLength identifierLength = IdentifierLength.Medium) where TContainer : class, new()
        {
            var identifier = Identifier.Create<TContainer>(identifierLength);
            return configuration.Get<TContainer>(identifier);
        }

        [CanBeNull]
        public static TContainer Get<TConsumer, TContainer>(this IConfiguration configuration, IdentifierLength identifierLength = IdentifierLength.Medium) where TContainer : class, new()
        {
            var identifier = Identifier.Create<TConsumer, TContainer>(null, identifierLength);
            return configuration.Get<TContainer>(identifier);
        }

        [CanBeNull]
        public static TContainer Get<TConsumer, TContainer>(this IConfiguration configuration, TConsumer instance, Func<TConsumer, string> getInstanceName, IdentifierLength identifierLength = IdentifierLength.Medium) where TContainer : class, new()
        {
            var identifier = Identifier.Create<TConsumer, TContainer>(getInstanceName(instance), identifierLength);
            return configuration.Get<TContainer>(identifier);
        }
    }
}