using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public class AppSettings : SettingProvider
    {
        public AppSettings(ISettingConverter converter) : base(new SettingNameFactory(), converter) { }

        protected override ISetting ReadCore(SettingName name)
        {
            var exeConfig = OpenExeConfiguration();
            var value = exeConfig.AppSettings.Settings[name.ToString()];
            return new Setting(name, value);
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