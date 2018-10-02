using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    /// <summary>
    /// The same setting can be defined in multiple data-stores. This setting-finder picks the first setting it finds.
    /// </summary>
    public class FirstSettingFinder : ISettingFinder
    {
        public bool TryFindSetting(
            IEnumerable<ISettingProvider> providers,
            SoftString settingName,
            Type settingType,
            SoftString providerName,
            out (ISettingProvider SettingProvider, ISetting Setting) result)
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            var settingQuery =
                from provider in providers
                where providerName.IsNullOrEmpty() || provider.Name.Equals(providerName)
                let setting = provider.Read(settingName, settingType)
                where setting.IsNotNull()
                select (provider, setting);

            result = settingQuery.FirstOrDefault();
            return !(result.SettingProvider is null && result.Setting is null);
        }
    }
}