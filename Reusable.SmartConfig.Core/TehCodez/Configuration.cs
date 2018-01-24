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
    [PublicAPI]
    public interface IConfiguration
    {
        [CanBeNull]
        object Select([NotNull] SoftString settingName, [NotNull] Type settingType, [CanBeNull] SoftString datastoreName);

        void Update([NotNull] SoftString settingName, [CanBeNull] object value);
    }

    public class Configuration : IConfiguration
    {
        private readonly IEnumerable<ISettingDataStore> _dataStores;
        private readonly IDictionary<SoftString, (SoftString ActualName, ISettingDataStore Datastore)> _settingCache = new Dictionary<SoftString, (SoftString ActualName, ISettingDataStore Datastore)>();
        
        public Configuration([NotNull, ItemNotNull] IEnumerable<ISettingDataStore> dataStores)
        {
            if (dataStores == null) throw new ArgumentNullException(nameof(dataStores));

            _dataStores = dataStores.ToList();
        }

        public object Select(SoftString settingName, Type settingType, SoftString datastoreName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));
            if (settingType == null) throw new ArgumentNullException(nameof(settingType));

            var setting = GetSetting(settingName, settingType, datastoreName);
            return setting.Value;
        }

        private ISetting GetSetting([NotNull] SoftString settingFullName, Type settingType, [CanBeNull] SoftString datastoreName)
        {
            // We search for the setting by all names so we need a list of all available names.
            var name = SettingName.Parse(settingFullName.ToString());

            var settingQuery =
                from datastore in _dataStores
                where datastoreName.IsNullOrEmpty() || datastore.Name.Equals(datastoreName)
                select (Datastore: datastore, Setting: datastore.Read(name, settingType));

            var result = settingQuery.FirstOrDefault(t => t.Setting.IsNotNull());

            if (result.Datastore.IsNull())
            {
                throw ("SettingNotFoundException", $"Setting {settingFullName.ToString().QuoteWith("'")} not found.").ToDynamicException();
            }

            CacheSettingDatastore(settingFullName, result.Setting.Name, result.Datastore);

            return result.Setting;
        }

        private void CacheSettingDatastore([NotNull] SoftString settingFullName, [NotNull] SoftString settingActualName, [NotNull] ISettingDataStore settingDataStore)
        {
            _settingCache[settingFullName] = (settingActualName, settingDataStore);
        }

        public void Update(SoftString settingName, object value)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            if (_settingCache.TryGetValue(settingName, out var t))
            {
                var setting = new Setting
                {
                    Name = t.ActualName,
                    Value = value
                };
                t.Datastore.Write(setting);
            }
            else
            {
                throw ("SettingNotInitializedException", $"Setting {settingName.ToString().QuoteWith("'")} needs to be initialized before you can update it.").ToDynamicException();
            }
        }      
    }
}