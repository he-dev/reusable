using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.SmartConfig.Collections;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Helpers;

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
        [NotNull] [ItemNotNull] private readonly IEnumerable<ISettingDataStore> _dataStores;
        private readonly ISettingConverter _converter;
        private readonly IDictionary<SoftString, (SoftString ActualName, ISettingDataStore Datastore)> _settingCache = new Dictionary<SoftString, (SoftString ActualName, ISettingDataStore Datastore)>();
        
        public Configuration([NotNull, ItemNotNull] IEnumerable<ISettingDataStore> dataStores, [NotNull] ISettingConverter converter)
        {
            if (dataStores == null) throw new ArgumentNullException(nameof(dataStores));
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            _dataStores = dataStores.ToList();
            _converter = converter;
        }

        [NotNull]
        public ISettingNameGenerator NameGenerator { get; set; } = new SettingNameGenerator();

        public object Select(SoftString settingName, Type settingType, SoftString datastoreName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));
            if (settingType == null) throw new ArgumentNullException(nameof(settingType));

            var setting = GetSetting(settingName, datastoreName);
            return setting.Value == null ? null : _converter.Deserialize(setting.Value, settingType);
        }

        private ISetting GetSetting([NotNull] SoftString settingFullName, [CanBeNull] SoftString datastoreName)
        {
            // We search for the setting by all names so we need a list of all available names.
            var names = NameGenerator.GenerateSettingNames(SettingName.Parse(settingFullName.ToString())).Select(name => (SoftString)(string)name).ToList();

            var settingQuery =
                from datastore in _dataStores
                where datastoreName.IsNullOrEmpty() || datastore.Name.Equals(datastoreName)
                select (Datastore: datastore, Setting: datastore.Read(names));

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
                    Value = value.IsNull() ? null : _converter.Serialize(value, t.Datastore.CustomTypes)
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