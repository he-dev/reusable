using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingFinder
    {
        bool TryFindSetting(
            [NotNull] IEnumerable<ISettingDataStore> dataStores,
            [NotNull] SoftString settingName,
            [CanBeNull] Type settingType, 
            [CanBeNull] SoftString dataStoreName,
            out (ISettingDataStore DataStore, ISetting Setting) result);
    }
}