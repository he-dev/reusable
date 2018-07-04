using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingFinder
    {
        bool TryFindSetting(
            [NotNull] IEnumerable<ISettingProvider> providers,
            [NotNull] SoftString settingName,
            [CanBeNull] Type settingType, 
            [CanBeNull] SoftString providerName,
            out (ISettingProvider SettingProvider, ISetting Setting) result);
    }
}