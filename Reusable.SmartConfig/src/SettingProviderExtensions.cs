using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using Reusable.Extensions;
using Reusable.SmartConfig.Annotations;

namespace Reusable.SmartConfig
{
    public static class SettingProviderExtensions
    {
        public static SettingProviderNaming Naming<T>(this T settingProvider, SettingQuery query) where T : ISettingProvider
        {
            var attributes =
                AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .SelectMany(x => x.GetCustomAttributes<SettingProviderAttribute>())
                    .ToList();

            var current = attributes.Where(x => x.Contains(settingProvider)).ToList();

            var settingNameComplexity =
                current
                    .Select(x => x.SettingNameStrength)
                    .Prepend(query.Strength)
                    .Append(SettingNameStrength.Medium)
                    .First(x => x != SettingNameStrength.Inherit);

            var prefix =
                query.PrefixHandling == PrefixHandling.Inherit
                    ? current
                        .Select(x => x.Prefix)
                        .FirstOrDefault(Conditional.IsNotNullOrEmpty)
                    : query.SettingName.Prefix;

            return new SettingProviderNaming
            {
                Strength = settingNameComplexity, 
                Prefix = prefix, 
                PrefixHandling = prefix.IsNullOrEmpty() ? query.PrefixHandling : PrefixHandling.Enable
            };
        }
        
        private static readonly ConcurrentDictionary<SoftString, int> SettingProviderCounters = new ConcurrentDictionary<SoftString, int>();
        
        internal static string CreateDefaultName<T>(this T settingProvider) where T : ISettingProvider
        {
            var providerType = settingProvider.GetType();
            var count = SettingProviderCounters.AddOrUpdate(providerType.ToPrettyString(), name => 1, (name, counter) => counter + 1);
            return providerType.ToPrettyString() + count;
        }
    }
}