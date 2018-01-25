using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingFinder
    {
        (ISettingDataStore DataStore, ISetting Setting) FindSetting(
            [NotNull] IEnumerable<ISettingDataStore> dataStores,
            [NotNull] SoftString settingName,
            [NotNull] Type settingType, 
            [CanBeNull] SoftString dataStoreName);
    }
}