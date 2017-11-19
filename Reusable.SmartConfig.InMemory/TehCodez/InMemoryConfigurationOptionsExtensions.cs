using System.Collections.Generic;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public static class InMemoryConfigurationOptionsExtensions
    {
        public static IConfigurationProperties UseInMemory(this IConfigurationProperties properties, IEnumerable<ISetting> settings)
        {
            properties.Datastores.Add(new InMemory(settings));
            return properties;
        }
    }
}