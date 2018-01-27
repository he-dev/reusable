using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.DataStores
{
    public class AppSettings : SettingDataStore
    {
        //private readonly System.Configuration.Configuration _exeConfiguration;
        //private readonly AppSettingsSection _appSettingsSection;

        public AppSettings(ISettingConverter converter) : base(converter) { }

        protected override ISetting ReadCore(IEnumerable<SoftString> names)
        {
            var exeConfig = OpenExeConfiguration();

            var result =
                (from name in names
                 let value = exeConfig.AppSettings.Settings[name.ToString()]?.Value
                 where !string.IsNullOrEmpty(value)
                 select (Name: name, Value: value)).FirstOrDefault();

            return result.Value.IsNullOrEmpty() ? null : new Setting(result.Name)
            {
                Value = result.Value
            };
        }

        protected override void WriteCore(ISetting setting)
        {
            var exeConfig = OpenExeConfiguration();

            if (exeConfig.AppSettings.Settings.AllKeys.Any(k => setting.Name.Equals(k)))
            {
                exeConfig.AppSettings.Settings[setting.ToString()].Value = setting.Value?.ToString();
            }
            else
            {
                exeConfig.AppSettings.Settings.Add(setting.Name.ToString(), (string)setting.Value);
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);
        }

        private static System.Configuration.Configuration OpenExeConfiguration()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
    }
}
