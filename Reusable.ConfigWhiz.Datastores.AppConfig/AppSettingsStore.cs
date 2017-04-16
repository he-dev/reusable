using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.ConfigWhiz.Data;

namespace Reusable.ConfigWhiz.Datastores.AppConfig
{
    public class AppSettingsStore : Datastore
    {
        //private readonly System.Configuration.Configuration _exeConfiguration;
        //private readonly AppSettingsSection _appSettingsSection;

        public AppSettingsStore(object handle) : base(handle, new[] { typeof(string) })
        {
            //_exeConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);            
            //_appSettingsSection = _exeConfiguration.AppSettings;
        }

        public override Result<IEnumerable<ISetting>> Read(SettingPath setting)
        {
            var exeConfig = OpenExeConfiguration();

            var settingName = setting.ToString(SettingPathFormat.FullWeak, SettingPathFormatter.Instance);
            var keys = exeConfig.AppSettings.Settings.AllKeys.Where(key => key.StartsWith(settingName, StringComparison.OrdinalIgnoreCase));
            var settings =
                from k in keys
                select new Setting
                {
                    Path = SettingPath.Parse(k),
                    Value = exeConfig.AppSettings.Settings[k].Value
                };
            return settings.ToList();
        }

        public override Result Write(IGrouping<SettingPath, ISetting> settings)
        {
            var exeConfig = OpenExeConfiguration();

            // If we are saving an itemized setting its keys might have changed.
            // Since we don't know the old keys we need to delete all keys that are alike first.

            void DeleteSettingGroup(AppSettingsSection appSettings)
            {
                var settingName = settings.Key.ToString(SettingPathFormat.FullWeak, SettingPathFormatter.Instance);
                var keys = appSettings.Settings.AllKeys.Where(key => key.StartsWith(settingName, StringComparison.OrdinalIgnoreCase));
                foreach (var key in keys)
                {
                    appSettings.Settings.Remove(key);
                }
            }

            foreach (var group in settings)
            {
                DeleteSettingGroup(exeConfig.AppSettings);

                foreach (var setting in settings)
                {
                    var settingName = settings.Key.ToString(SettingPathFormat.FullStrong, SettingPathFormatter.Instance);
                    exeConfig.AppSettings.Settings.Add(settingName, (string)setting.Value);
                }
            }
            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return Result.Ok();
        }


        private System.Configuration.Configuration OpenExeConfiguration() => ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
    }
}
