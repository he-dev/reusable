using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public class Configuration : IConfiguration
    {
        private readonly IEnumerable<ISettingDataStore> _dataStores;
        private readonly ISettingFinder _settingFinder;
        private readonly IDictionary<SoftString, (SoftString ActualName, ISettingDataStore Datastore)> _settingMap = new Dictionary<SoftString, (SoftString ActualName, ISettingDataStore Datastore)>();
        
        public Configuration([NotNull, ItemNotNull] IEnumerable<ISettingDataStore> dataStores, ISettingFinder settingFinder = null)
        {
            if (dataStores == null) throw new ArgumentNullException(nameof(dataStores));

            _dataStores = dataStores.ToList();
            _settingFinder = settingFinder ?? new FirstSettingFinder();
        }

        public object GetValue(SoftString settingName, Type settingType, SoftString dataStoreName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));
            if (settingType == null) throw new ArgumentNullException(nameof(settingType));

            var result = 
                _settingFinder
                    .FindSetting(_dataStores, settingName, settingType, dataStoreName)
                    .Next(x => CacheSettingName(settingName, x.Setting.Name, x.DataStore));

            return result.Setting;
        }

        private void CacheSettingName(SoftString settingFullName, SoftString settingActualName, ISettingDataStore settingDataStore)
        {
            _settingMap[settingFullName] = (settingActualName, settingDataStore);
        }

        public void SetValue(SoftString settingName, object value)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            if (_settingMap.TryGetValue(settingName, out var item))
            {
                var setting = new Setting
                {
                    Name = item.ActualName,
                    Value = value
                };
                item.Datastore.Write(setting);
            }
            else
            {
                throw ("SettingNotInitializedException", $"Setting {settingName.ToString().QuoteWith("'")} needs to be initialized before you can update it.").ToDynamicException();
            }
        }      
    }
}