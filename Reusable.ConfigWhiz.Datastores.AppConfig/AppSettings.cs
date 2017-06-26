using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Paths;

namespace Reusable.ConfigWhiz.Datastores.AppConfig
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

        protected override ICollection<ISetting> ReadCore(SettingIdentifier settingIdentifier)
        {
            var exeConfig = OpenExeConfiguration();

            var settingName = settingIdentifier.ToString(IdentifierFormat.FullWeak, IdentifierFormatter.Instance);
            var keys = exeConfig.AppSettings.Settings.AllKeys.Where(key => key.StartsWith(settingName, StringComparison.OrdinalIgnoreCase));
            var settings =
                from k in keys
                select new Setting
                {
                    Identifier = SettingIdentifier.Parse(k),
                    Value = exeConfig.AppSettings.Settings[k].Value
                };
            return settings.Cast<ISetting>().ToList();
        }

        protected override int WriteCore(IGrouping<SettingIdentifier, ISetting> settings)
        {
            var exeConfig = OpenExeConfiguration();

            // If we are saving an itemized setting its keys might have changed.
            // Since we don't know the old keys we need to delete all keys that are alike first.

            var settingsAffected = 0;

            void DeleteSettingGroup(AppSettingsSection appSettings)
            {
                var settingName = settings.Key.ToString(IdentifierFormat.FullWeak, IdentifierFormatter.Instance);
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
                var settingName = setting.Identifier.ToFullStrongString();
                exeConfig.AppSettings.Settings.Add(settingName, (string)setting.Value);
                settingsAffected++;
            }
            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return settingsAffected;
        }


        private System.Configuration.Configuration OpenExeConfiguration() => ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
    }
}
