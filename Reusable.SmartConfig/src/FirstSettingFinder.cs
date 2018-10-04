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
            GetValueQuery getValueQuery,
            IEnumerable<ISettingProvider> providers,
            out (ISettingProvider SettingProvider, ISetting Setting) result
        )
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));
            if (getValueQuery == null) throw new ArgumentNullException(nameof(getValueQuery));

            if (!(getValueQuery.ProviderName is null))
            {
                providers = new[]
                {
                    providers.SingleOrDefault(p => p.Name == getValueQuery.ProviderName)
                    ?? throw DynamicException.Create("ProviderNotFound", $"There is no such provider as {getValueQuery.ProviderName.ToString().QuoteWith("'")}.")
                };
            }

            var settings =
                from provider in providers
                let setting = provider.Read(getValueQuery.SettingName, getValueQuery.SettingType, getValueQuery.SettingNameConvention)
                where setting.IsNotNull()
                select (provider, setting);

            result = settings.FirstOrDefault();
            return !(result.SettingProvider is null && result.Setting is null);
        }
    }
}