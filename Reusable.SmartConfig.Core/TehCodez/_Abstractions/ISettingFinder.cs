using System;
using System.Collections.Generic;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingFinder
    {
        (ISettingDataStore DataStore, ISetting Setting) FindSetting(IEnumerable<ISettingDataStore> dataStores, SoftString settingName, Type settingType, SoftString dataStoreName);
    }
}