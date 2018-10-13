using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    /// <summary>
    /// The same setting can be defined in multiple data-stores. This setting-finder picks the first setting it finds.
    /// </summary>
    public class FirstSettingFinder : ISettingFinder
    {
        public bool TryFindSetting
        (
            SelectQuery query,
            IEnumerable<ISettingProvider> providers,
            out (ISettingProvider SettingProvider, ISetting Setting) result
        )
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));
            if (query == null) throw new ArgumentNullException(nameof(query));

            if (!(query.ProviderName is null))
            {
                providers = new[]
                {
                    providers.SingleOrDefault(p => p.Name == query.ProviderName)
                    ?? throw DynamicException.Create("ProviderNotFound", $"There is no such provider as {query.ProviderName.ToString().QuoteWith("'")}.")
                };
            }

            var findSetting =
                from provider in providers
                let setting = provider.Read(query)
                where setting.IsNotNull()
                select (provider, setting);

            result = findSetting.FirstOrDefault();
            return !result.IsEmpty();
        }
    }

    internal static class FirstSettingFinderHelper
    {
        public static bool IsEmpty(this (ISettingProvider SettingProvider, ISetting Setting) result)
        {
            return
                result.SettingProvider is null &&
                result.Setting is null;
        }
    }
}