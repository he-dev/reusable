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

        protected override ISetting Read(SettingName name)
        {
            var exeConfig = OpenExeConfiguration();
            var element = exeConfig.AppSettings.Settings[name.ToString()];
            return element is null ? default : new Setting(name, element.Value);
        }

        protected override void Write(ISetting setting)
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