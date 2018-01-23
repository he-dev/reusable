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
    public interface IConfiguration
    {
        [CanBeNull]
        object Select([NotNull] SoftString settingName, [NotNull] Type settingType, [CanBeNull] SoftString datastoreName);

        void Update([NotNull] SoftString settingName, [CanBeNull] object value);
    }

    public interface IConfigurationProperties
    {
        [NotNull] 
        DatastoreCollection Datastores { get; set; }

        [NotNull]
        ISettingConverter Converter { get; set; }

        [NotNull]
        ISettingNameGenerator SettingNameGenerator { get; set; }
    }

    public class Configuration : IConfiguration, IEnumerable<ISettingDataStore>
    {
        private readonly IConfigurationProperties _properties;
        private readonly IDictionary<SoftString, (SoftString ActualName, ISettingDataStore Datastore)> _settings = new Dictionary<SoftString, (SoftString ActualName, ISettingDataStore Datastore)>();

        public Configuration(Action<IConfigurationProperties> propertiesAction)
        {
            _properties = ObjectFactory.CreateInstance<IConfigurationProperties>();
            _properties.Datastores = new DatastoreCollection();
            _properties.SettingNameGenerator = new SettingNameGenerator();
            propertiesAction(_properties);
        }

        //public Action<string> Log { get; set; } // for future use

        //private void OnLog(string message) => Log?.Invoke(message); // for future use        

        public object Select(SoftString settingName, Type settingType, SoftString datastoreName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));
            if (settingType == null) throw new ArgumentNullException(nameof(settingType));

            var setting = GetSetting(settingName, datastoreName);
            return setting.Value == null ? null : _properties.Converter.Deserialize(setting.Value, settingType);
        }

        private ISetting GetSetting([NotNull] SoftString settingFullName, [CanBeNull] SoftString datastoreName)
        {
            // We search for the setting by all names so we need a list of all available names.
            var names = _properties.SettingNameGenerator.GenerateSettingNames(SettingName.Parse(settingFullName.ToString())).Select(name => (SoftString)(string)name).ToList();

            var settingQuery =
                from datastore in _properties.Datastores
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
            _settings[settingFullName] = (settingActualName, settingDataStore);
        }

        public void Update(SoftString settingName, object value)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            if (_settings.TryGetValue(settingName, out var t))
            {
                var setting = new Setting
                {
                    Name = t.ActualName,
                    Value = value.IsNull() ? null : _properties.Converter.Serialize(value, t.Datastore.CustomTypes)
                };
                t.Datastore.Write(setting);
            }
            else
            {
                throw ("SettingNotInitializedException", $"Setting {settingName.ToString().QuoteWith("'")} needs to be initialized before you can update it.").ToDynamicException();
            }
        }

        #region IEnumerable 

        public IEnumerator<ISettingDataStore> GetEnumerator()
        {
            return _properties.Datastores.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}