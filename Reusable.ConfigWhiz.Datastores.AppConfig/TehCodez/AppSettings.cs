using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Reusable.Collections;
using Reusable.Extensions;
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

        protected override IEntity ReadCore(IEnumerable<CaseInsensitiveString> names)
        {
            var exeConfig = OpenExeConfiguration();

            var result =
                (from n in names
                 let v = exeConfig.AppSettings.Settings[n.ToString()].Value
                 where !string.IsNullOrEmpty(v)
                 select (Name: n, Value: v)).FirstOrDefault();

            return
                result.Value.IsNullOrEmpty() ? null : new Entity
                {
                    Name = result.Name,
                    Value = result.Value
                };
        }

        protected override void WriteCore(IEntity setting)
        {
            var exeConfig = OpenExeConfiguration();

            // If we are saving an itemized setting its keys might have changed.
            // Since we don't know the old keys we need to delete all keys that are alike first.

            var settingsAffected = 0;

            void DeleteSettingGroup(AppSettingsSection appSettings)
            {
                //var settingName = settings.Key.ToString();
                var keys = appSettings.Settings.AllKeys.Select(Identifier.Parse).Where(x => x.StartsWith(settings.Key));
                foreach (var key in keys)
                {
                    appSettings.Settings.Remove(key.ToString());
                    settingsAffected++;
                }
            }

            DeleteSettingGroup(exeConfig.AppSettings);

            foreach (var setting in settings)
            {
                var settingName = setting.Name.ToString();
                exeConfig.AppSettings.Settings.Add(settingName, (string)setting.Value);
                settingsAffected++;
            }
            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return settingsAffected;
        }


        private System.Configuration.Configuration OpenExeConfiguration() => ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
    }
}
