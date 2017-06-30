using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Datastores.AppConfig
{
    public class AppSettings : Datastore
    {
        //private readonly System.Configuration.Configuration _exeConfiguration;
        //private readonly AppSettingsSection _appSettingsSection;

        public AppSettings(string name) : base(name, new[] { typeof(string) })
        {
            //_exeConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);            
            //_appSettingsSection = _exeConfiguration.AppSettings;
        }

        public AppSettings() : base(CreateDefaultName<AppSettings>(), new[] { typeof(string) }) { }

        protected override ICollection<IEntity> ReadCore(IIdentifier id)
        {
            var exeConfig = OpenExeConfiguration();

            var settingName = id.ToString();
            var keys = exeConfig.AppSettings.Settings.AllKeys.Where(key => key.StartsWith(settingName, StringComparison.OrdinalIgnoreCase));
            var settings =
                from k in keys
                select new Entity
                {
                    Id = Identifier.Parse(k),
                    Value = exeConfig.AppSettings.Settings[k].Value
                };
            return settings.Cast<IEntity>().ToList();
        }

        protected override int WriteCore(IGrouping<IIdentifier, IEntity> settings)
        {
            var exeConfig = OpenExeConfiguration();

            // If we are saving an itemized setting its keys might have changed.
            // Since we don't know the old keys we need to delete all keys that are alike first.

            var settingsAffected = 0;

            void DeleteSettingGroup(AppSettingsSection appSettings)
            {
                var settingName = settings.Key.ToString();
                var keys = appSettings.Settings.AllKeys.Where(key => key.StartsWith(settingName, StringComparison.OrdinalIgnoreCase));
                foreach (var key in keys)
                {
                    appSettings.Settings.Remove(key);
                    settingsAffected++;
                }
            }

            DeleteSettingGroup(exeConfig.AppSettings);

            foreach (var setting in settings)
            {
                var settingName = setting.Id.ToString();
                exeConfig.AppSettings.Settings.Add(settingName, (string)setting.Value);
                settingsAffected++;
            }
            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return settingsAffected;
        }


        private System.Configuration.Configuration OpenExeConfiguration() => ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
    }
}
